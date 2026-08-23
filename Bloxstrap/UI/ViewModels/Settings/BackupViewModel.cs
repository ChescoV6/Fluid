using System.IO;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using Bloxstrap.UI.Elements.Base;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class BackupViewModel : NotifyPropertyChangedViewModel
    {
        public ICommand ExportCommand => new RelayCommand(() => DoExport());
        public ICommand ImportCommand => new RelayCommand(() => DoImport());

        private static void DoExport()
        {
            var dialog = new SaveFileDialog
            {
                FileName = $"Fluid-config-{DateTime.Now:yyyy-MM-dd}",
                Filter = "Fluid config pack (*.json)|*.json"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                ConfigManager.ExportConfig(dialog.FileName);
                Frontend.ShowMessageBox("Configuration exported successfully.", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox($"Export failed: {ex.Message}", MessageBoxImage.Error);
            }
        }

        private static void DoImport()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Fluid config pack (*.json)|*.json|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                int restored = ConfigManager.ImportConfig(dialog.FileName);
                Frontend.ShowMessageBox($"Configuration imported ({restored} sections restored).\n\nRestart Fluid to see everything applied.", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox($"Import failed: {ex.Message}", MessageBoxImage.Error);
            }
        }
    }
}
