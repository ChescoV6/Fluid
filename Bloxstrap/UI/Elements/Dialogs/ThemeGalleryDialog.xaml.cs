using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Bloxstrap.UI.Elements.Base;

namespace Bloxstrap.UI.Elements.Dialogs
{
    public partial class ThemeGalleryDialog : WpfUiWindow
    {
        private List<string> _themes = new();

        public ThemeGalleryDialog()
        {
            InitializeComponent();
            Loaded += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            ThemesListBox.IsEnabled = false;

            try
            {
                _themes = await Task.Run(ThemeGallery.ListThemes);
                ThemesListBox.ItemsSource = _themes;

                if (_themes.Count > 0)
                    ThemesListBox.SelectedIndex = 0;
                else
                    ThemesListBox.ItemsSource = new[] { "(no themes found)" };
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox($"Could not load the theme gallery: {ex.Message}", MessageBoxImage.Error);
                Close();
                return;
            }

            ThemesListBox.IsEnabled = true;
        }

        private async void Install_Click(object sender, RoutedEventArgs e)
        {
            if (ThemesListBox.SelectedItem is not string theme)
                return;

            try
            {
                await ThemeGallery.InstallTheme(theme);

                Frontend.ShowMessageBox($"Installed '{theme}'. It now appears in your custom themes list.", MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox($"Install failed: {ex.Message}", MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
