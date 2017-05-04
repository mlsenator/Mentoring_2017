using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DownloadManager
{
    public class ProgressBarHelper
    {
        private Random _random = new Random();

        public async Task UpdateProgressBar(ProgressBar progressBar, Label statusLabel)
        {
            var time = _random.Next(1000,5000);
            var iterationNumber = 100;
            var step = time / iterationNumber;
            for (int i = 0; i < iterationNumber; i++)
            {
                await Task.Delay(step);
                progressBar.Value += 100 / iterationNumber;
            }

            progressBar.Value = 100;
            statusLabel.Content = "Done";
        }
    }
}