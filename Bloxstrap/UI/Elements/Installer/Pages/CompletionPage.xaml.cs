using System.Windows;
using Bloxstrap.UI.ViewModels.Installer;

namespace Bloxstrap.UI.Elements.Installer.Pages
{
    /// <summary>
    /// Interaction logic for CompletionPage.xaml
    /// </summary>
    public partial class CompletionPage
    {
        private readonly CompletionViewModel _viewModel = new();
        public CompletionPage()
        {
            _viewModel.CloseWindowRequest += (_, closeAction) =>
            {
                if (Window.GetWindow(this) is MainWindow window)
                {
                    window.CloseAction = closeAction;
                    window.Close();
                }
            };

            DataContext = _viewModel;
            InitializeComponent();
        }

        private void UiPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow window)
            {
                window.SetNextButtonText(Strings.Common_Navigation_Next);
                window.SetButtonEnabled("back", false);
            }
        }

        private void Preset_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button || button.Tag is not string preset)
                return;

            try
            {
                FlagProfileManager.ApplyBuiltInPreset(preset);
                Frontend.ShowMessageBox($"'{preset}' preset applied. It will be used the next time Roblox starts.", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox($"Failed to apply preset: {ex.Message}", MessageBoxImage.Error);
            }
        }
    }
}
