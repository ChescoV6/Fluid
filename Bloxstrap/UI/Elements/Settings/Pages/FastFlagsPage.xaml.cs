using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Bloxstrap.UI.ViewModels.Settings;
using Wpf.Ui.Mvvm.Contracts;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for FastFlagsPage.xaml
    /// </summary>
    public partial class FastFlagsPage
    {
        private bool _initialLoad = false;

        private FastFlagsViewModel _viewModel = null!;

        public FastFlagsPage()
        {
            SetupViewModel();
            InitializeComponent();
            RefreshProfileList();
        }

        private void SetupViewModel()
        {
            _viewModel = new FastFlagsViewModel();

            _viewModel.OpenFlagEditorEvent += OpenFlagEditor;
            _viewModel.RequestPageReloadEvent += (_, _) => ReloadPage();

            DataContext = _viewModel;
        }

        private void ReloadPage()
        {
            SetupViewModel();
            RefreshProfileList();
        }

        private void RefreshProfileList()
        {
            var selected = ProfileComboBox.SelectedItem as string;

            ProfileComboBox.Items.Clear();
            RuleProfileComboBox.Items.Clear();

            foreach (string profile in FlagProfileManager.ListProfiles())
            {
                ProfileComboBox.Items.Add(profile);
                RuleProfileComboBox.Items.Add(profile);
            }

            if (selected is not null && ProfileComboBox.Items.Contains(selected))
                ProfileComboBox.SelectedItem = selected;
            else if (ProfileComboBox.Items.Count > 0)
                ProfileComboBox.SelectedIndex = 0;

            if (RuleProfileComboBox.Items.Count > 0)
                RuleProfileComboBox.SelectedIndex = 0;

            RefreshRuleList();
        }

        private void RefreshRuleList()
        {
            RuleListPanel.Children.Clear();

            if (GameRuleManager.Instance.Prop.Count == 0)
            {
                RuleListPanel.Children.Add(new TextBlock
                {
                    Text = "No rules set.",
                    FontSize = 12,
                    Foreground = FindResource("TextFillColorTertiaryBrush") as System.Windows.Media.Brush
                });
                return;
            }

            foreach (var rule in GameRuleManager.Instance.Prop.ToList())
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };

                row.Children.Add(new TextBlock
                {
                    Text = $"Place {rule.Key}  →  {rule.Value}",
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center
                });

                var removeButton = new Wpf.Ui.Controls.Button
                {
                    Content = "Remove",
                    Margin = new Thickness(10, 0, 0, 0),
                    MinWidth = 80
                };

                string placeId = rule.Key;
                removeButton.Click += (_, _) =>
                {
                    GameRuleManager.Instance.Remove(placeId);
                    RefreshRuleList();
                };

                row.Children.Add(removeButton);
                RuleListPanel.Children.Add(row);
            }
        }

        private void AddRule_Click(object sender, RoutedEventArgs e)
        {
            string? placeId = RulePlaceIdBox.Text.Trim();

            if (String.IsNullOrEmpty(placeId))
            {
                Frontend.ShowMessageBox("Enter a place ID first.", MessageBoxImage.Warning);
                return;
            }

            if (RuleProfileComboBox.SelectedItem is not string profile)
            {
                Frontend.ShowMessageBox("Save a profile first, then assign it.", MessageBoxImage.Warning);
                return;
            }

            try
            {
                GameRuleManager.Instance.Add(placeId, profile);
                RulePlaceIdBox.Text = "";
                RefreshRuleList();
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
            }
        }

        private void ApplyProfile_Click(object sender, RoutedEventArgs e)
        {
            if (ProfileComboBox.SelectedItem is not string profile)
            {
                Frontend.ShowMessageBox("Select a profile to apply first.", MessageBoxImage.Warning);
                return;
            }

            var confirm = Frontend.ShowMessageBox(
                $"Apply profile '{profile}'?\n\nThis replaces your entire current flag set.",
                MessageBoxImage.Warning,
                MessageBoxButton.YesNo);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                int count = FlagProfileManager.ApplyProfile(profile);
                Frontend.ShowMessageBox($"Applied '{profile}' ({count} flags).", MessageBoxImage.Information);
                ReloadPage();
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox($"Failed to apply profile: {ex.Message}", MessageBoxImage.Error);
            }
        }

        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            string? name = Dialogs.InputDialog.Show(
                "Save flag profile",
                "Enter a name for this profile:",
                $"Profile {DateTime.Now:yyyy-MM-dd HH:mm}");

            if (name is null)
                return;

            try
            {
                FlagProfileManager.SaveProfile(name);
                RefreshProfileList();
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox($"Failed to save profile: {ex.Message}", MessageBoxImage.Error);
            }
        }

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            if (ProfileComboBox.SelectedItem is not string profile)
                return;

            var confirm = Frontend.ShowMessageBox($"Delete profile '{profile}'?", MessageBoxImage.Warning, MessageBoxButton.YesNo);

            if (confirm != MessageBoxResult.Yes)
                return;

            FlagProfileManager.DeleteProfile(profile);
            RefreshProfileList();
        }

        private void Preset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button || button.Tag is not string preset)
                return;

            try
            {
                FlagProfileManager.ApplyBuiltInPreset(preset);
                Frontend.ShowMessageBox($"'{preset}' preset applied on top of your current flags.", MessageBoxImage.Information);
                ReloadPage();
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox($"Failed to apply preset: {ex.Message}", MessageBoxImage.Error);
            }
        }

        private void OpenFlagEditor(object? sender, EventArgs e)
        {
            if (Window.GetWindow(this) is INavigationWindow window)
            {
               window.Navigate(typeof(FastFlagEditorPage));
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // refresh datacontext on page load to synchronize with editor page
            
            if (!_initialLoad)
            {
                _initialLoad = true;
                return;
            }

            ReloadPage();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });

            e.Handled = true;
        }

        private void ValidateInt32(object sender, TextCompositionEventArgs e) => e.Handled = e.Text != "-" && !Int32.TryParse(e.Text, out int _);
        
        private void ValidateUInt32(object sender, TextCompositionEventArgs e) => e.Handled = !UInt32.TryParse(e.Text, out uint _);
    }
}
