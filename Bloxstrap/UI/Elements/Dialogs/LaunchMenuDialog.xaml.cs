using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Bloxstrap.UI.ViewModels.Dialogs;
using Bloxstrap.UI.ViewModels.Installer;

namespace Bloxstrap.UI.Elements.Dialogs
{
    /// <summary>
    /// Interaction logic for LaunchMenuDialog.xaml
    /// </summary>
    public partial class LaunchMenuDialog
    {
        public NextAction CloseAction = NextAction.Terminate;

        public LaunchMenuDialog()
        {
            var viewModel = new LaunchMenuViewModel();
            viewModel.CloseWindowRequest += (_, closeAction) =>
            {
                CloseAction = closeAction;
                Close();
            };

            DataContext = viewModel;

            InitializeComponent();

            RefreshFavorites();
        }

        private void RefreshFavorites()
        {
            FavoritesList.ItemsSource = FavoritesManager.Instance.Prop.ToList();
        }

        private void FavoritePlay_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not FavoriteGame game)
                return;

            CloseAction = NextAction.Terminate;
            FavoritesManager.Launch(game);
            Close();
        }

        private void FavoriteRemove_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not FavoriteGame game)
                return;

            FavoritesManager.Instance.Remove(game);
            RefreshFavorites();
        }
    }
}
