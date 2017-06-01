using PdfManager;
using ServiceContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CentralServer
{
	public class CentaralServer
	{
		private string _outDirectory;
        private string _settingsDirectory;
        private int _lastTimeout;

        private PdfHelper _pdfHelper;
        private FileSystemWatcher _watcher;

		private Task _processFilesTask;
		private Task _monitoringTask;
		private CancellationTokenSource _tokenSource;
		private ManualResetEvent _stopWaitEvent;	

		public CentaralServer(string outDir)
		{
			_outDirectory = outDir;
            _settingsDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\"));
            _lastTimeout = 0;
            _pdfHelper = new PdfHelper();

            CheckDirectory(_outDirectory);
            CheckMessageQueue(ServiceHelper.ServerQueueName);
            CheckMessageQueue(ServiceHelper.MonitorQueueName);
            CheckMessageQueue(ServiceHelper.ClientQueueName);

			_watcher = new FileSystemWatcher(_settingsDirectory);
			_watcher.Filter = "*.xml";
			_watcher.Changed += Watcher_Changed;

			_stopWaitEvent = new ManualResetEvent(false);
			_tokenSource = new CancellationTokenSource();
			_processFilesTask = new Task(() => ProcessFiles(_tokenSource.Token));
			_monitoringTask = new Task(() => MonitorService(_tokenSource.Token));
		}

		public void ProcessFiles(CancellationToken token)
		{
			using (var serverQueue = new MessageQueue(ServiceHelper.ServerQueueName))
			{
				serverQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(PdfChunk) });
				var chunks = new List<PdfChunk>();

				do
				{
					var enumerator = serverQueue.GetMessageEnumerator2();
					var count = 0;

					while (enumerator.MoveNext())
					{
						var body = enumerator.Current.Body;
						if (body is PdfChunk)
						{
							var chunk = (PdfChunk)body;
							chunks.Add(chunk);

							if (chunk.Position == chunk.Size)
							{
								_pdfHelper.SaveDocumentUsingChunks(_outDirectory, chunks);
								chunks.Clear();
							}
						}

						count++;
					}

					for (int i = 0; i < count; i++)
					{
						serverQueue.Receive();
					}

					Thread.Sleep(ServiceHelper.DefaultTimeOut);
				}
				while (!token.IsCancellationRequested);
			}
		}

		public void MonitorService(CancellationToken token)
		{
			using (var serverQueue = new MessageQueue(ServiceHelper.MonitorQueueName))
			{
				serverQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(Settings) });

				while (!token.IsCancellationRequested)
				{
					var asyncReceive = serverQueue.BeginPeek();

					if (WaitHandle.WaitAny(new WaitHandle[] { _stopWaitEvent, asyncReceive.AsyncWaitHandle }) == 0)
					{
						break;
					}

					var message = serverQueue.EndPeek(asyncReceive);
					serverQueue.Receive();
					var settings = (Settings)message.Body;
					_lastTimeout = settings.Timeout;
					WriteSettings(settings);
				}
			}
		}

		public void WriteSettings(Settings settings)
		{
			var fullPath = Path.Combine(_settingsDirectory, "setting.csv");

			using (StreamWriter sw = File.AppendText(fullPath))
			{
				var line = string.Format("{0},{1},{2}", settings.Date, settings.Status, settings.Timeout);
				sw.WriteLine(line);
			}
		}

		private void Watcher_Changed(object sender, FileSystemEventArgs e)
		{
			var fullPath = Path.Combine(_settingsDirectory, "timeout.xml");
			if (TryToOpen(fullPath, 3))
			{
				XDocument doc = XDocument.Load(fullPath);
				var timeout = int.Parse(doc.Root.Value);
				if (_lastTimeout != timeout)
				{
					using (var clientQueue = new MessageQueue(ServiceHelper.ClientQueueName))
					{
						clientQueue.Send(timeout);
                        _lastTimeout = timeout;
					}
				}
			}		
		}

		public void Start()
		{
			_processFilesTask.Start();
			_monitoringTask.Start();
			_watcher.EnableRaisingEvents = true;
		}

		public void Stop()
		{
			_watcher.EnableRaisingEvents = false;
			_tokenSource.Cancel();
			_stopWaitEvent.Set();
			Task.WaitAll(_processFilesTask, _monitoringTask);
		}

        private bool TryToOpen(string filePath, int tryCount)
        {
            for (int i = 0; i < tryCount; i++)
            {
                try
                {
                    var file = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                    file.Close();

                    return true;
                }
                catch (IOException)
                {
                    Thread.Sleep(5000);
                }
            }

            return false;
        }

        private void CheckDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void CheckMessageQueue(string name)
        {
            if (!MessageQueue.Exists(name))
            {
                MessageQueue.Create(name);
            }
        }
    }
}
