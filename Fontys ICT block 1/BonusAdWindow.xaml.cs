using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Fontys_ICT_block_1
{
    // A rewarded-ad style popup shown after claiming the balance bonus.
    // Can't be closed - by the Close button, Alt+F4, or a taskbar close -
    // until waitSeconds have passed.
    public partial class BonusAdWindow : Window
    {
        private int secondsRemaining;
        private readonly DispatcherTimer countdownTimer;

        public BonusAdWindow(Stream imageStream, int waitSeconds = 10)
        {
            InitializeComponent();

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            // OnLoad reads the whole stream into memory up front, so we
            // can dispose imageStream right after EndInit instead of
            // having to keep it open for the window's lifetime.
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = imageStream;
            bitmap.EndInit();
            bitmap.Freeze();
            imageStream.Dispose();

            BonusImage.Source = bitmap;

            secondsRemaining = waitSeconds;
            UpdateCloseButtonText();

            countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            secondsRemaining--;

            if (secondsRemaining <= 0)
            {
                countdownTimer.Stop();
                CloseButton.IsEnabled = true;
                CloseButton.Content = "Close";
            }
            else
            {
                UpdateCloseButtonText();
            }
        }

        private void UpdateCloseButtonText()
        {
            CloseButton.Content = $"Close ({secondsRemaining})";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Blocks Alt+F4 and a taskbar close attempt while the
            // countdown hasn't finished - there's no title-bar X button
            // to worry about since WindowStyle is "None".
            if (secondsRemaining > 0)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }
    }
}
