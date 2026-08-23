using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bloxstrap
{
    /// <summary>
    /// Handles exporting, importing and migrating Fluid configuration.
    /// </summary>
    public static class ConfigManager
    {
        private const string PackType = "fluid-config-pack";
        private const int PackVersion = 1;

        private static readonly (string Name, string Location)[] PackedFiles =
        {
            ("settings", App.Settings.FileLocation),
            ("fastFlags", App.FastFlags.FileLocation),
            ("state", App.State.FileLocation),
            ("robloxState", App.RobloxState.FileLocation)
        };

        public static void ExportConfig(string filePath)
        {
            const string LOG_IDENT = "ConfigManager::ExportConfig";

            var pack = new JsonObject
            {
                ["type"] = PackType,
                ["packVersion"] = PackVersion,
                ["created"] = DateTime.UtcNow.ToString("o"),
                ["appVersion"] = App.Version
            };

            foreach (var (name, location) in PackedFiles)
            {
                if (!File.Exists(location))
                    continue;

                try
                {
                    pack[name] = JsonNode.Parse(File.ReadAllText(location));
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Failed to read {location}: {ex.Message}");
                }
            }

            File.WriteAllText(filePath, pack.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            App.Logger.WriteLine(LOG_IDENT, $"Exported configuration to {filePath}");
        }

        /// <summary>
        /// Imports a config pack written by ExportConfig. Returns the number of sections restored.
        /// </summary>
        public static int ImportConfig(string filePath)
        {
            const string LOG_IDENT = "ConfigManager::ImportConfig";

            var pack = JsonNode.Parse(File.ReadAllText(filePath))!.AsObject();

            if (pack["type"]?.GetValue<string>() != PackType)
                throw new InvalidDataException("Not a valid Fluid config pack");

            int restored = 0;

            foreach (var (name, location) in PackedFiles)
            {
                if (pack[name] is not JsonObject section)
                    continue;

                Directory.CreateDirectory(Path.GetDirectoryName(location)!);

                if (File.Exists(location))
                    File.Copy(location, location + ".bak", true);

                File.WriteAllText(location, section.ToJsonString());
                restored++;
            }

            ReloadManagers();

            App.Logger.WriteLine(LOG_IDENT, $"Imported configuration from {filePath} ({restored} sections)");
            return restored;
        }

        public static List<(string Name, string Path)> DetectLegacyInstalls()
        {
            var results = new List<(string, string)>();

            foreach (string name in new[] { "Fishstrap", "Bloxstrap" })
            {
                string dir = Path.Combine(Paths.LocalAppData, name);

                if (File.Exists(Path.Combine(dir, "settings.json")))
                    results.Add((name, dir));
            }

            return results;
        }

        /// <summary>
        /// Imports settings, fast flags and mods from an upstream Fishstrap/Bloxstrap install.
        /// Returns a short human-readable summary of what was taken.
        /// </summary>
        public static string MigrateFrom(string legacyDir)
        {
            const string LOG_IDENT = "ConfigManager::MigrateFrom";
            var summary = new List<string>();

            // 1. settings - overlay legacy values onto ours (same schema lineage)
            string legacySettingsPath = Path.Combine(legacyDir, "settings.json");

            if (File.Exists(legacySettingsPath))
            {
                try
                {
                    var legacy = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(legacySettingsPath));
                    var current = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(App.Settings.FileLocation));

                    if (legacy is not null && current is not null)
                    {
                        int copied = 0;

                        foreach (var pair in legacy)
                        {
                            current[pair.Key] = pair.Value;
                            copied++;
                        }

                        // never migrate the install location of the old app
                        current.Remove("InstallLocation");

                        File.WriteAllText(App.Settings.FileLocation, JsonSerializer.Serialize(current, new JsonSerializerOptions { WriteIndented = true }));
                        summary.Add($"{copied} settings");
                        App.Logger.WriteLine(LOG_IDENT, $"Migrated {copied} settings from {legacyDir}");
                    }
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            }

            // 2. fast flags - merge legacy flags over ours
            string legacyFlagsPath = Path.Combine(legacyDir, "Modifications", "ClientSettings", "ClientAppSettings.json");

            if (File.Exists(legacyFlagsPath))
            {
                try
                {
                    var legacyFlags = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(legacyFlagsPath));

                    if (legacyFlags is not null)
                    {
                        foreach (var pair in legacyFlags)
                            App.FastFlags.SetValue(pair.Key, pair.Value);

                        App.FastFlags.Save();
                        summary.Add($"{legacyFlags.Count} fast flags");
                        App.Logger.WriteLine(LOG_IDENT, $"Migrated {legacyFlags.Count} fast flags from {legacyDir}");
                    }
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            }

            // 3. mods and custom themes - copy anything we do not already have
            foreach (string folder in new[] { "Modifications", "CustomThemes" })
            {
                string legacyFolder = Path.Combine(legacyDir, folder);
                if (!Directory.Exists(legacyFolder))
                    continue;

                int filesCopied = 0;

                foreach (string source in Directory.EnumerateFiles(legacyFolder, "*", SearchOption.AllDirectories))
                {
                    // client settings were handled above
                    if (source.Contains("ClientAppSettings.json"))
                        continue;

                    string relative = Path.GetRelativePath(legacyFolder, source);
                    string target = Path.Combine(Paths.Base, folder, relative);

                    if (File.Exists(target))
                        continue;

                    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                    File.Copy(source, target, false);
                    filesCopied++;
                }

                if (filesCopied > 0)
                {
                    summary.Add($"{filesCopied} files in {folder.ToLower()}");
                    App.Logger.WriteLine(LOG_IDENT, $"Migrated {filesCopied} files into {folder}");
                }
            }

            ReloadManagers();
            return summary.Count > 0 ? String.Join(", ", summary) : "nothing found to migrate";
        }

        private static void ReloadManagers()
        {
            App.Settings.Load();
            App.FastFlags.Load();
            App.State.Load();
            App.RobloxState.Load();
        }
    }
}
