using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

using Bloxstrap.UI.ViewModels.Settings;
using Wpf.Ui.Controls;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Configuration backup, restore and legacy migration.
    /// </summary>
    public partial class BackupPage
    {
        public BackupViewModel ViewModel { get; } = new();

        public BackupPage()
        {
            DataContext = ViewModel;
            InitializeComponent();
            Loaded += (_, _) => RefreshMigrationCards();
        }

        private void RefreshMigrationCards()
        {
            MigrationPanel.Children.Clear();

            var installs = ConfigManager.DetectLegacyInstalls();

            if (installs.Count == 0)
            {
                MigrationPanel.Children.Add(new TextBlock
                {
                    Text = "No Fishstrap or Bloxstrap installations were detected.",
                    FontSize = 12,
                    Foreground = FindResource("TextFillColorTertiaryBrush") as System.Windows.Media.Brush,
                    Margin = new Thickness(0, 4, 0, 0)
                });
                return;
            }

            foreach (var (name, path) in installs)
            {
                var card = new CardAction
                {
                    Margin = new Thickness(0, 8, 0, 0)
                };

                var stack = new StackPanel();
                stack.Children.Add(new TextBlock { FontSize = 14, Text = $"Migrate from {name}" });
                stack.Children.Add(new TextBlock
                {
                    FontSize = 12,
                    Padding = new Thickness(0, 2, 16, 0),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = FindResource("TextFillColorTertiaryBrush") as System.Windows.Media.Brush,
                    Text = path
                });

                card.Content = stack;
                card.Click += async (_, _) => await MigrateAsync(name, path);

                MigrationPanel.Children.Add(card);
            }
        }

        private async Task MigrateAsync(string name, string path)
        {
            var confirm = Frontend.ShowMessageBox(
                $"Import settings, fast flags, mods and themes from {name}?\n\nExisting Fluid settings with the same name will be overwritten.",
                MessageBoxImage.Question,
                MessageBoxButton.YesNo);

            if (confirm != MessageBoxResult.Yes)
                return;

            string summary = await Task.Run(() => ConfigManager.MigrateFrom(path));

            Frontend.ShowMessageBox($"Migration from {name} finished. Imported: {summary}.\n\nRestart Fluid to see everything applied.", MessageBoxImage.Information);
            RefreshMigrationCards();
        }
    }
}
