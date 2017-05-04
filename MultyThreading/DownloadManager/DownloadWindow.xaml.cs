using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DownloadManager
{
    /// <summary>
    /// Interaction logic for DownloadWindow.xaml
    /// </summary>
    public partial class DownloadWindow : Window
    {
        private readonly Downloader _downloadManager;
        private CancellationTokenSource _cancellationTokenSource;

        public DownloadWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _downloadManager = new Downloader();
        }

        private async void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            DownloadProgress.Visibility = Visibility.Visible;
            StatusLabel.Visibility = Visibility.Hidden;
            DownloadProgress.IsIndeterminate = true;
            CancelBtn.Visibility = Visibility.Visible;
            DownloadBtn.Visibility = Visibility.Hidden;

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                var uri = new Uri(WebAddress.Text);

                await _downloadManager.DownloadPageAsync(uri, _cancellationTokenSource.Token);

                StatusLabel.Content = "Done";
            }
            catch (Exception)
            {
                ;
            }
            finally
            {
                DownloadProgress.IsIndeterminate = false;
                DownloadProgress.Visibility = Visibility.Hidden;
                StatusLabel.Visibility = Visibility.Visible;
                DownloadBtn.Visibility = Visibility.Visible;
                CancelBtn.Visibility = Visibility.Hidden;
            }            
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusLabel.Visibility = Visibility.Visible;
            StatusLabel.Content = "Canceled";
            DownloadProgress.Visibility = Visibility.Hidden;
            DownloadProgress.IsIndeterminate = false;
            DownloadBtn.Visibility = Visibility.Visible;
            CancelBtn.Visibility = Visibility.Hidden;

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }
    }
}
