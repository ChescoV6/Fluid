using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Bloxstrap.UI.Elements.Installer
{
    /// <summary>
    /// Branded splash shown briefly when the installer launches.
    /// </summary>
    public partial class FluidSplash : Window
    {
        public FluidSplash()
        {
            InitializeComponent();
            Opacity = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(240));
            BeginAnimation(OpacityProperty, fadeIn);

            var hold = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1150) };
            hold.Tick += (_, _) =>
            {
                hold.Stop();

                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(340));
                fadeOut.Completed += (_, _) => Close();
                BeginAnimation(OpacityProperty, fadeOut);
            };
            hold.Start();
        }
    }
}
