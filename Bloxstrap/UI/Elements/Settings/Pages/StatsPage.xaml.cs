using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Local play statistics.
    /// </summary>
    public partial class StatsPage
    {
        public StatsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => Refresh();

        private void Refresh()
        {
            var (sessions, total, topGames) = PlayStatsManager.Instance.GetSummary();

            SessionCountText.Text = sessions.ToString("N0");
            TotalTimeText.Text = FormatDuration(total);

            TopGamesPanel.Children.Clear();

            if (topGames.Count == 0)
            {
                TopGamesPanel.Children.Add(new TextBlock
                {
                    Text = "No sessions recorded yet.",
                    FontSize = 12,
                    Foreground = FindResource("TextFillColorTertiaryBrush") as System.Windows.Media.Brush
                });
                return;
            }

            int rank = 1;

            foreach (var (name, time, visits) in topGames)
            {
                var card = new Wpf.Ui.Controls.Card { Padding = new Thickness(14, 10, 14, 10), Margin = new Thickness(0, 0, 0, 8) };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var rankText = new TextBlock { Text = $"#{rank}", FontSize = 16, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 14, 0) };
                grid.Children.Add(rankText);
                Grid.SetColumn(rankText, 0);

                var nameText = new TextBlock { Text = name, FontSize = 14, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
                grid.Children.Add(nameText);
                Grid.SetColumn(nameText, 1);

                var metaText = new TextBlock
                {
                    Text = $"{FormatDuration(time)}  ·  {visits} visit{(visits == 1 ? "" : "s")}",
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = FindResource("TextFillColorTertiaryBrush") as System.Windows.Media.Brush
                };
                grid.Children.Add(metaText);
                Grid.SetColumn(metaText, 2);

                card.Content = grid;
                TopGamesPanel.Children.Add(card);

                rank++;
            }
        }

        private static string FormatDuration(TimeSpan span)
        {
            if (span.TotalMinutes < 1)
                return "under a minute";

            if (span.TotalHours < 1)
                return $"{span.Minutes}m";

            return $"{(int)span.TotalHours}h {span.Minutes}m";
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Fluid-stats-{DateTime.Now:yyyy-MM-dd}",
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                PlayStatsManager.ExportCsv(dialog.FileName);
                Frontend.ShowMessageBox("Statistics exported successfully.", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox($"Export failed: {ex.Message}", MessageBoxImage.Error);
            }
        }
    }
}
