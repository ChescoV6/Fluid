using System.Windows;

using Bloxstrap.UI.Elements.Base;

namespace Bloxstrap.UI.Elements.Dialogs
{
    public partial class InputDialog : WpfUiWindow
    {
        public string Value => ValueBox.Text.Trim();

        private bool _confirmed;

        public InputDialog(string title, string message, string initialValue = "")
        {
            InitializeComponent();

            Title = title;
            MessageText.Text = message;
            ValueBox.Text = initialValue;

            Loaded += (_, _) =>
            {
                ValueBox.Focus();
                ValueBox.SelectAll();
            };
        }

        /// <summary>
        /// Shows the dialog and returns the entered value, or null if cancelled.
        /// </summary>
        public static string? Show(string title, string message, string initialValue = "")
        {
            var dialog = new InputDialog(title, message, initialValue);

            dialog.ShowDialog();

            return dialog._confirmed ? dialog.Value : null;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(Value))
                return;

            _confirmed = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
