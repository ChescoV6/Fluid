using System.IO;
using System.Text.Json;

namespace Bloxstrap
{
    /// <summary>
    /// Saved FastFlag profiles and built-in performance presets.
    /// </summary>
    public static class FlagProfileManager
    {
        private static string ProfilesDir => Paths.SavedFlagProfiles;

        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public static class BuiltInPresets
        {
            public const string MaxFps = "Max FPS";
            public const string Balanced = "Balanced";
            public const string Quality = "Quality";

            public static readonly IReadOnlyList<string> All = new[] { MaxFps, Balanced, Quality };
        }

        public static List<string> ListProfiles()
        {
            if (!Directory.Exists(ProfilesDir))
                return new List<string>();

            return Directory.GetFiles(ProfilesDir, "*.json")
                            .Select(Path.GetFileNameWithoutExtension)
                            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                            .ToList();
        }

        public static void SaveProfile(string name)
        {
            const string LOG_IDENT = "FlagProfileManager::SaveProfile";

            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Profile name cannot be empty");

            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            Directory.CreateDirectory(ProfilesDir);

            var payload = new Dictionary<string, object>
            {
                ["name"] = name,
                ["created"] = DateTime.UtcNow.ToString("o"),
                ["flags"] = App.FastFlags.Prop
            };

            string path = Path.Combine(ProfilesDir, $"{name}.json");
            File.WriteAllText(path, JsonSerializer.Serialize(payload, JsonOptions));

            App.Logger.WriteLine(LOG_IDENT, $"Saved profile '{name}' ({App.FastFlags.Prop.Count} flags)");
        }

        /// <summary>
        /// Restores a saved profile, fully replacing the current flag set.
        /// </summary>
        public static int ApplyProfile(string name)
        {
            const string LOG_IDENT = "FlagProfileManager::ApplyProfile";

            string path = Path.Combine(ProfilesDir, $"{name}.json");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Profile '{name}' does not exist");

            var payload = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(path));

            if (payload is null || !payload.TryGetValue("flags", out var flags) || flags is null)
                throw new InvalidDataException($"Profile '{name}' has invalid data");

            App.FastFlags.Prop.Clear();

            foreach (var pair in flags)
                App.FastFlags.Prop[pair.Key] = pair.Value;

            App.FastFlags.Save();

            App.Logger.WriteLine(LOG_IDENT, $"Applied profile '{name}' ({flags.Count} flags)");
            return flags.Count;
        }

        public static void DeleteProfile(string name)
        {
            string path = Path.Combine(ProfilesDir, $"{name}.json");

            if (File.Exists(path))
                File.Delete(path);
        }

        /// <summary>
        /// Applies a built-in performance preset on top of the current flags (non-destructive).
        /// </summary>
        public static void ApplyBuiltInPreset(string name)
        {
            const string LOG_IDENT = "FlagProfileManager::ApplyBuiltInPreset";

            switch (name)
            {
                case BuiltInPresets.MaxFps:
                    App.FastFlags.SetValue("DFIntTaskSchedulerTargetFps", 999);
                    App.FastFlags.SetValue("DFIntDebugFRMQualityLevelOverride", 1);
                    App.FastFlags.SetValue("FIntDebugForceMSAASamples", 1);
                    break;

                case BuiltInPresets.Balanced:
                    App.FastFlags.SetValue("DFIntTaskSchedulerTargetFps", 240);
                    App.FastFlags.SetValue("DFIntDebugFRMQualityLevelOverride", 6);
                    App.FastFlags.SetValue("FIntDebugForceMSAASamples", 1);
                    break;

                case BuiltInPresets.Quality:
                    App.FastFlags.SetValue("DFIntTaskSchedulerTargetFps", 240);
                    App.FastFlags.SetValue("DFIntDebugFRMQualityLevelOverride", 10);
                    App.FastFlags.SetValue("FIntDebugForceMSAASamples", 4);
                    break;

                default:
                    throw new ArgumentException($"Unknown preset '{name}'");
            }

            App.FastFlags.Save();
            App.Logger.WriteLine(LOG_IDENT, $"Applied built-in preset '{name}'");
        }
    }
}
