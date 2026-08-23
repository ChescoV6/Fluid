using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bloxstrap
{
    public class StoredAccount
    {
        public string Name { get; set; } = "";
        public long UserId { get; set; } = 0;
        public string ProtectedToken { get; set; } = "";

        [JsonIgnore]
        public bool IsCurrent => ProtectedToken == AccountManager.Protect(AccountManager.CurrentCookie);
    }

    /// <summary>
    /// Stores Roblox accounts for quick switching. Tokens are encrypted with DPAPI (current user).
    /// </summary>
    public class AccountManager : JsonManager<List<StoredAccount>>
    {
        public override string ClassName => "Accounts";

        public static AccountManager Instance { get; } = new();

        public static string CurrentCookie => App.Cookies.GetAuthCookie() ?? "";

        private const int TokenEntropyLength = 16;

        public static string Protect(string token)
        {
            byte[] entropy = RandomNumberGenerator.GetBytes(TokenEntropyLength);

            var payload = new JsonObject
            {
                ["e"] = Convert.ToBase64String(entropy),
                ["t"] = Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(token), entropy, DataProtectionScope.CurrentUser))
            };

            return payload.ToJsonString();
        }

        public static string? Unprotect(string stored)
        {
            try
            {
                var payload = JsonNode.Parse(stored)!.AsObject();

                byte[] entropy = Convert.FromBase64String(payload["e"]!.GetValue<string>());
                byte[] data = Convert.FromBase64String(payload["t"]!.GetValue<string>());

                return Encoding.UTF8.GetString(ProtectedData.Unprotect(data, entropy, DataProtectionScope.CurrentUser));
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("AccountManager::Unprotect", $"Failed to decrypt token: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Saves the currently logged in account under the given name. Requires cookie access to be enabled and cookies loaded.
        /// </summary>
        public async Task<string> SaveCurrent(string name)
        {
            string? cookie = CurrentCookie;

            if (String.IsNullOrEmpty(cookie))
                throw new InvalidOperationException("No active Roblox session found. Enable cookie access and launch Roblox once, then try again.");

            AuthenticatedUser? user = await App.Cookies.GetAuthenticated()
                ?? throw new InvalidOperationException("Could not verify the current account (invalid or expired session?)");

            Prop.RemoveAll(x => x.UserId == user.Id);

            Prop.Add(new StoredAccount
            {
                Name = name.Trim(),
                UserId = user.Id,
                ProtectedToken = Protect(cookie)
            });

            Save();

            return user.Username;
        }

        /// <summary>
        /// Writes the stored account's token into Roblox's cookie store. Takes effect on next client launch.
        /// </summary>
        public async Task<bool> SwitchTo(StoredAccount account)
        {
            string? token = Unprotect(account.ProtectedToken);

            if (token is null)
                throw new InvalidOperationException("Failed to decrypt this account's token");

            return await App.Cookies.WriteAuthCookie(token);
        }

        public void Remove(StoredAccount account)
        {
            Prop.Remove(account);
            Save();
        }
    }
}
