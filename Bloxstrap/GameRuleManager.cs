using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Bloxstrap
{
    /// <summary>
    /// Maps place IDs to FastFlag profiles, applied automatically when that game is launched.
    /// </summary>
    public class GameRuleManager : JsonManager<Dictionary<string, string>>
    {
        public override string ClassName => "GameRules";

        public static GameRuleManager Instance { get; } = new();

        public void Add(string placeId, string profileName)
        {
            if (!UInt64.TryParse(placeId, out _))
                throw new ArgumentException("Place ID must be a number");

            if (!File.Exists(Path.Combine(Paths.SavedFlagProfiles, $"{profileName}.json")))
                throw new ArgumentException($"Profile '{profileName}' does not exist");

            Prop[placeId] = profileName;
            Save();
        }

        public void Remove(string placeId)
        {
            if (Prop.Remove(placeId))
                Save();
        }

        /// <summary>
        /// Applies the profile mapped to this place ID, if a rule exists.
        /// Returns the profile name when applied, null otherwise.
        /// </summary>
        public string? ApplyRuleForPlace(long placeId)
        {
            const string LOG_IDENT = "GameRuleManager::ApplyRuleForPlace";

            string key = placeId.ToString();

            if (!Prop.TryGetValue(key, out string? profileName) || String.IsNullOrEmpty(profileName))
                return null;

            try
            {
                FlagProfileManager.ApplyProfile(profileName);
                App.Logger.WriteLine(LOG_IDENT, $"Applied profile '{profileName}' for place {placeId}");
                return profileName;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Failed to apply profile '{profileName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extracts a place ID from a roblox-player launch URI, if present.
        /// </summary>
        public static long? ParsePlaceId(string? launchArgs)
        {
            if (String.IsNullOrEmpty(launchArgs))
                return null;

            var match = Regex.Match(launchArgs, "placeId=(\\d+)");

            if (!match.Success || !long.TryParse(match.Groups[1].Value, out long placeId))
                return null;

            return placeId;
        }
    }
}
