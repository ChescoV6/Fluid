using Bloxstrap.UI.ViewModels.Settings;

using System.Windows.Controls;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for AppearancePage.xaml
    /// </summary>
    public partial class AppearancePage
    {
        public AppearancePage()
        {
            DataContext = new AppearanceViewModel(this);
            InitializeComponent();
        }

        public void CustomThemeSelection(object sender, SelectionChangedEventArgs e)
        {
            AppearanceViewModel viewModel = (AppearanceViewModel)DataContext;

            viewModel.SelectedCustomTheme = (string)((ListBox)sender).SelectedItem;
            viewModel.SelectedCustomThemeName = viewModel.SelectedCustomTheme;

            viewModel.OnPropertyChanged(nameof(viewModel.SelectedCustomTheme));
            viewModel.OnPropertyChanged(nameof(viewModel.SelectedCustomThemeName));
        }

        private void OpenThemeGallery_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var dialog = new Dialogs.ThemeGalleryDialog
            {
                Owner = System.Windows.Window.GetWindow(this)
            };

            dialog.ShowDialog();

            // reload the theme list so newly installed themes show up
            DataContext = new AppearanceViewModel(this);
        }
    }
}