using System.IO;
using System.Text.Json.Nodes;

namespace Bloxstrap
{
    /// <summary>
    /// Browse and install community bootstrapper themes from the Fishstrap examples repository.
    /// </summary>
    public static class ThemeGallery
    {
        private const string ApiBase = "https://api.github.com/repos/fishstrap/custom-bootstrapper-examples/contents";
        private const string RawBase = "https://raw.githubusercontent.com/fishstrap/custom-bootstrapper-examples/main";

        public static async Task<List<string>> ListThemes()
        {
            Uri url = new(ApiBase);

            var response = await Http.GetJson<List<GitHubContent>>(url)
                ?? throw new InvalidHTTPResponseException("Gallery request returned nothing");

            return response.Where(x => x.Type == "dir")
                           .Select(x => x.Name)
                           .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                           .ToList();
        }

        public static async Task InstallTheme(string name)
        {
            const string LOG_IDENT = "ThemeGallery::InstallTheme";

            string safeName = Sanitize(name);
            string dir = Path.Combine(Paths.CustomThemes, safeName);

            Directory.CreateDirectory(dir);

            foreach (string fileName in new[] { "Theme.xml", "theme.json" })
            {
                try
                {
                    Uri url = new($"{RawBase}/{Uri.EscapeDataString(name)}/{fileName}");
                    string content = await App.HttpClient.GetStringAsync(url);
                    File.WriteAllText(Path.Combine(dir, fileName), content);

                    App.Logger.WriteLine(LOG_IDENT, $"Installed gallery theme '{safeName}'");
                    return;
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"{fileName} not available for '{name}': {ex.Message}");
                }
            }

            throw new InvalidHTTPResponseException($"Could not find theme files for '{name}'");
        }

        private static string Sanitize(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            return name;
        }
    }

    public class GitHubContent
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("path")]
        public string Path { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "";
    }
}
