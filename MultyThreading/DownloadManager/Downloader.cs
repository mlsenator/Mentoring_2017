using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DownloadManager
{
    public class Downloader
    {
        public async Task DownloadPageAsync(Uri uri, CancellationToken cancellationToken)
        {
            using (HttpResponseMessage response = await new HttpClient().GetAsync(uri, cancellationToken))
            {
                var page = await response.Content.ReadAsStringAsync();
                var path = string.Format($"{uri.Host}.html");
                //simulating 
                await Task.Delay(3000, cancellationToken);
                File.WriteAllText(path, page, Encoding.Unicode);
            }
        }
    }
}