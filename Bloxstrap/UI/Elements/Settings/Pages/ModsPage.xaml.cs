using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for ModsPage.xaml
    /// </summary>
    public partial class ModsPage
    {
        public ModsPage()
        {
            InitializeComponent();
            Loaded += (_, _) => RefreshModList();
        }

        private void RefreshModList()
        {
            ModManifestPanel.Children.Clear();

            if (!Directory.Exists(Paths.Modifications))
                return;

            var dirs = Directory.GetDirectories(Paths.Modifications)
                                .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
                                .ToList();

            if (dirs.Count == 0)
            {
                ModManifestPanel.Children.Add(new TextBlock
                {
                    Text = "No mods installed. Place files into the Modifications folder to use them.",
                    FontSize = 12,
                    Foreground = FindResource("TextFillColorTertiaryBrush") as System.Windows.Media.Brush
                });
                return;
            }

            foreach (string dir in dirs)
            {
                string manifestPath = Path.Combine(dir, "manifest.json");
                string name = Path.GetFileName(dir);
                string? description = null;
                string? version = null;

                if (File.Exists(manifestPath))
                {
                    try
                    {
                        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();

                        if (manifest["name"] is not null) name = manifest["name"]!.GetValue<string>();
                        description = manifest["description"]?.GetValue<string>();
                        version = manifest["version"]?.GetValue<string>();
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteLine("ModsPage::RefreshModList", $"Invalid manifest.json in '{dir}': {ex.Message}");
                    }
                }

                var card = new Wpf.Ui.Controls.Card { Padding = new Thickness(14, 10, 14, 10), Margin = new Thickness(0, 0, 0, 8) };

                var stack = new StackPanel();
                var titleRow = new StackPanel { Orientation = Orientation.Horizontal };

                titleRow.Children.Add(new TextBlock
                {
                    FontSize = 14,
                    FontWeight = FontWeights.Medium,
                    Text = name
                });

                if (!String.IsNullOrEmpty(version))
                {
                    titleRow.Children.Add(new TextBlock
                    {
                        FontSize = 12,
                        Margin = new Thickness(8, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = FindResource("TextFillColorTertiaryBrush") as System.Windows.Media.Brush,
                        Text = $"v{version.TrimStart('v')}"
                    });
                }

                stack.Children.Add(titleRow);

                if (!String.IsNullOrEmpty(description))
                {
                    stack.Children.Add(new TextBlock
                    {
                        FontSize = 12,
                        Margin = new Thickness(0, 2, 16, 0),
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = FindResource("TextFillColorTertiaryBrush") as System.Windows.Media.Brush,
                        Text = description
                    });
                }

                card.Content = stack;
                ModManifestPanel.Children.Add(card);
            }
        }
    }
}
