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
		const string ServerQueueName = @".\private$\CentralServerQueue";
		const string MonitorQueueName = @".\private$\MonitorQueue";
		const string ClientQueueName = @".\Private$\ClientQueue";

		private string outDirectory;
		private FileSystemWatcher watcher;
		private Task processFilesTask;
		private Task monitoringTask;
		private CancellationTokenSource tokenSource;
		private ManualResetEvent stopWaitEvent;	
		private int lastTimeout;

		public CentaralServer(string outDir)
		{
			outDirectory = outDir;
			if (!Directory.Exists(outDirectory))
				Directory.CreateDirectory(outDirectory);

			if (!MessageQueue.Exists(ServerQueueName))
				MessageQueue.Create(ServerQueueName);

			if (!MessageQueue.Exists(MonitorQueueName))
				MessageQueue.Create(MonitorQueueName);

			if (!MessageQueue.Exists(ClientQueueName))
				MessageQueue.Create(ClientQueueName);

			string pathToXml = GetFullPath();
			watcher = new FileSystemWatcher(pathToXml);
			watcher.Filter = "*.xml";
			watcher.Changed += Watcher_Changed;

			stopWaitEvent = new ManualResetEvent(false);
			tokenSource = new CancellationTokenSource();
			processFilesTask = new Task(() => ProcessFiles(tokenSource.Token));
			monitoringTask = new Task(() => MonitorService(tokenSource.Token));
		}

		public void ProcessFiles(CancellationToken token)
		{
			using (var serverQueue = new MessageQueue(ServerQueueName))
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

							if (chunk.Pozition == chunk.Size)
							{
								SaveFile(chunks);
								chunks.Clear();
							}
						}

						count++;
					}

					for (int i = 0; i < count; i++)
					{
						serverQueue.Receive();
					}

					Thread.Sleep(1000);
				}
				while (!token.IsCancellationRequested);
			}
		}

		public void SaveFile(List<PdfChunk> chunks)
		{
			var documentIndex = Directory.GetFiles(outDirectory).Length + 1;
			var resultFile = Path.Combine(outDirectory, string.Format("result_{0}.pdf", documentIndex));
			using (Stream destination = File.Create(Path.Combine(outDirectory, resultFile)))
			{
				foreach (var chunk in chunks)
				{
					destination.Write(chunk.Buffer.ToArray(), 0, chunk.BufferSize);
				}
			}
		}

		public void MonitorService(CancellationToken token)
		{
			using (var serverQueue = new MessageQueue(MonitorQueueName))
			{
				serverQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(Settings) });

				while (!token.IsCancellationRequested)
				{
					var asyncReceive = serverQueue.BeginPeek();

					if (WaitHandle.WaitAny(new WaitHandle[] { stopWaitEvent, asyncReceive.AsyncWaitHandle }) == 0)
					{
						break;
					}

					var message = serverQueue.EndPeek(asyncReceive);
					serverQueue.Receive();
					var settings = (Settings)message.Body;
					lastTimeout = settings.Timeout;
					WriteSettings(settings);
				}
			}
		}

		public void WriteSettings(Settings settings)
		{
			string path = GetFullPath();
			var fullPath = Path.Combine(path, "setting.csv");

			using (StreamWriter sw = File.AppendText(fullPath))
			{
				var line = string.Format("{0},{1},{2}s", settings.Date, settings.Status, settings.Timeout);
				sw.WriteLine(line);
			}
		}

		private void Watcher_Changed(object sender, FileSystemEventArgs e)
		{
			string path = GetFullPath();
			var fullPath = Path.Combine(path, "timeout.xml");
			if (TryOpen(fullPath, 3))
			{
				XDocument doc = XDocument.Load(fullPath);
				var timeout = int.Parse(doc.Root.Value);
				if (lastTimeout != timeout)
				{
					using (var clientQueue = new MessageQueue(ClientQueueName))
					{
						clientQueue.Send(timeout);
					}
				}
			}		
		}

		private string GetFullPath()
		{
			var currentDir = AppDomain.CurrentDomain.BaseDirectory;
			return Path.GetFullPath(Path.Combine(currentDir, @"..\..\"));
		}

		public void Start()
		{
			processFilesTask.Start();
			monitoringTask.Start();
			watcher.EnableRaisingEvents = true;
		}

		public void Stop()
		{
			watcher.EnableRaisingEvents = false;
			tokenSource.Cancel();
			stopWaitEvent.Set();
			Task.WaitAll(processFilesTask, monitoringTask);
		}

		private bool TryOpen(string fileName, int tryCount)
		{
			for (int i = 0; i < tryCount; i++)
			{
				try
				{
					var file = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
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
	}
}
