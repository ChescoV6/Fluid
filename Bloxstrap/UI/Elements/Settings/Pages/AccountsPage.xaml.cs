using System.Windows;
using System.Windows.Controls;

using Bloxstrap.UI.Elements.Dialogs;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Saved Roblox accounts and switching.
    /// </summary>
    public partial class AccountsPage
    {
        public AccountsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => RefreshAccounts();

        private void RefreshAccounts()
        {
            AccountListPanel.Children.Clear();

            var accounts = AccountManager.Instance.Prop.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();

            if (accounts.Count == 0)
            {
                AccountListPanel.Children.Add(new TextBlock
                {
                    Text = "No accounts saved yet.",
                    FontSize = 12,
                    Foreground = FindResource("TextFillColorTertiaryBrush") as System.Windows.Media.Brush,
                    Margin = new Thickness(0, 4, 0, 0)
                });
                return;
            }

            foreach (var account in accounts)
            {
                AccountListPanel.Children.Add(BuildAccountCard(account));
            }
        }

        private UIElement BuildAccountCard(StoredAccount account)
        {
            var card = new Wpf.Ui.Controls.CardAction
            {
                Margin = new Thickness(0, 8, 0, 0)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                FontSize = 14,
                Text = account.Name + (account.IsCurrent ? "  (active)" : "")
            });

            grid.Children.Add(stack);
            Grid.SetColumn(stack, 0);

            var switchButton = new Wpf.Ui.Controls.Button
            {
                Content = account.IsCurrent ? "Active" : "Switch",
                Appearance = account.IsCurrent ? Wpf.Ui.Common.ControlAppearance.Secondary : Wpf.Ui.Common.ControlAppearance.Primary,
                MinWidth = 90,
                Margin = new Thickness(0, 0, 8, 0),
                IsEnabled = !account.IsCurrent
            };
            switchButton.Click += async (_, _) => await SwitchAsync(account);
            grid.Children.Add(switchButton);
            Grid.SetColumn(switchButton, 1);

            var removeButton = new Wpf.Ui.Controls.Button
            {
                Content = "Remove",
                MinWidth = 90
            };
            removeButton.Click += (_, _) =>
            {
                var confirm = Frontend.ShowMessageBox($"Remove '{account.Name}' from saved accounts?", MessageBoxImage.Warning, MessageBoxButton.YesNo);
                if (confirm == MessageBoxResult.Yes)
                {
                    AccountManager.Instance.Remove(account);
                    RefreshAccounts();
                }
            };
            grid.Children.Add(removeButton);
            Grid.SetColumn(removeButton, 2);

            card.Content = grid;
            return card;
        }

        private async void AddCurrent_Click(object sender, RoutedEventArgs e)
        {
            string? name = InputDialog.Show("Add current account", "Enter a name for this account:");

            if (String.IsNullOrEmpty(name))
                return;

            try
            {
                string username = await AccountManager.Instance.SaveCurrent(name);
                Frontend.ShowMessageBox($"Saved '{username}' as '{name}'.", MessageBoxImage.Information);
                RefreshAccounts();
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
            }
        }

        private async Task SwitchAsync(StoredAccount account)
        {
            try
            {
                bool success = await AccountManager.Instance.SwitchTo(account);

                if (success)
                    Frontend.ShowMessageBox($"Switched to '{account.Name}'. Restart Roblox to use this account.", MessageBoxImage.Information);
                else
                    Frontend.ShowMessageBox("Failed to switch (cookie access must be enabled in Behaviour settings).", MessageBoxImage.Error);

                RefreshAccounts();
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
            }
        }
    }
}
