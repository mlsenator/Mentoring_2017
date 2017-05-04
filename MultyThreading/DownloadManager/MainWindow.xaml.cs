using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DownloadManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnWindowLoaded;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var progressBarHelper = new ProgressBarHelper();
            var tasks = new List<Task>()
                            {
                                progressBarHelper.UpdateProgressBar(OperationProgress1, OperationStatus1),
                                progressBarHelper.UpdateProgressBar(OperationProgress2, OperationStatus2),
                                progressBarHelper.UpdateProgressBar(OperationProgress3, OperationStatus3),
                                progressBarHelper.UpdateProgressBar(OperationProgress4, OperationStatus4),
                                progressBarHelper.UpdateProgressBar(OperationProgress5, OperationStatus5),
                            };

            await Task.WhenAll(tasks);

            var downloadWindow = new DownloadWindow();
            downloadWindow.Show();
            this.Close();
        }
    }
}
