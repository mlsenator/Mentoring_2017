using CentralServer;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DocumentСaptureService
{
	public class DocumentCaptureService
	{
		const string ServerQueueName = @".\private$\CentralServerQueue";
		const string MonitorQueueName = @".\private$\MonitorQueue";
		const string ClientQueueName = @".\Private$\ClientQueue";

		private string inDirectory;
		private string tempDirectory;
		private FileSystemWatcher watcher;
		private Task processFilesTask;
		private Task sendSettingsTask;
		private Task controlTask;
		private CancellationTokenSource tokenSource;
		private AutoResetEvent newFileEvent;
		private ManualResetEvent stopWaitEvent;
		private Document document;
		private Section section;
		private PdfDocumentRenderer pdfRender;
		private int timeout;
		private string status;

		public DocumentCaptureService(string inDir, string tempDir)
		{
			inDirectory = inDir;
			tempDirectory = tempDir;

			if (!Directory.Exists(inDirectory))
				Directory.CreateDirectory(inDirectory);

			if (!Directory.Exists(tempDirectory))
				Directory.CreateDirectory(tempDirectory);

			if (!MessageQueue.Exists(ServerQueueName))
				MessageQueue.Create(ServerQueueName);

			if (!MessageQueue.Exists(MonitorQueueName))
				MessageQueue.Create(MonitorQueueName);

			if (!MessageQueue.Exists(ClientQueueName))
				MessageQueue.Create(ClientQueueName);

			watcher = new FileSystemWatcher(inDirectory);
			watcher.Created += Watcher_Created;

			timeout = 5000;
			status = "Waiting";
			stopWaitEvent = new ManualResetEvent(false);
			tokenSource = new CancellationTokenSource();
			newFileEvent = new AutoResetEvent(false);
			processFilesTask = new Task(() => ProcessFiles(tokenSource.Token));
			sendSettingsTask = new Task(() => SendSettings(tokenSource.Token));
			controlTask = new Task(() => ControlService(tokenSource.Token));
		}

		public void ProcessFiles(CancellationToken token)
		{
			var currentImageIndex = -1;
			var nextPageWaiting = false;
			CreateNewDocument();

			do
			{
				status = "Process";
				foreach (var file in Directory.EnumerateFiles(inDirectory).OrderBy(f => f))
				{
					var fileName = Path.GetFileName(file);

					if (IsValidFormat(fileName))
					{
						var imageIndex = GetIndex(fileName);
						if (imageIndex != currentImageIndex + 1 && currentImageIndex != -1 && nextPageWaiting)
						{
							SendDocument();
							CreateNewDocument();
							nextPageWaiting = false;
						}

						if (TryOpen(file, 3))
						{
							var outFile = Path.Combine(tempDirectory, fileName);
							if (File.Exists(outFile))
							{
								File.Delete(file);
							}
							else
							{
								File.Move(file, outFile);
							}

							AddImageToDocument(outFile);
							currentImageIndex = imageIndex;
							nextPageWaiting = true;
						}
					}
					else
					{
						if (TryOpen(file, 3))
						{
							File.Delete(file);
						}
					}
				}

				status = "Waiting";
				if (!newFileEvent.WaitOne(timeout) && nextPageWaiting)
				{
					SendDocument();
					CreateNewDocument();
					nextPageWaiting = false;
				}

				if (token.IsCancellationRequested)
				{
					if (nextPageWaiting)
					{
						SendDocument();
					}

					foreach (var file in Directory.EnumerateFiles(tempDirectory))
					{
						if (TryOpen(file, 3))
						{
							File.Delete(file);
						}
					}
				}
			}
			while (!token.IsCancellationRequested);
		}

		private void CreateNewDocument()
		{
			document = new Document();
			section = document.AddSection();
			pdfRender = new PdfDocumentRenderer();
			pdfRender.Document = document;
		}

		public void SendSettings(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				var settings = new Settings
				{
					Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
					Status = status,
					Timeout = timeout
				};

				using (var serverQueue = new MessageQueue(MonitorQueueName))
				{
					var message = new Message(settings);
					serverQueue.Send(message);
				}

				Thread.Sleep(10000);
			}
		}

		public void ControlService(CancellationToken token)
		{
			using (var clientQueue = new MessageQueue(ClientQueueName))
			{
				clientQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(int) });

				while (!token.IsCancellationRequested)
				{
					var asyncReceive = clientQueue.BeginPeek();

					if (WaitHandle.WaitAny(new WaitHandle[] { stopWaitEvent, asyncReceive.AsyncWaitHandle }) == 0)
					{
						break;
					}

					var message = clientQueue.EndPeek(asyncReceive);
					clientQueue.Receive();
					timeout = (int)message.Body;
				}
			}
		}

		private void SendDocument()
		{
			pdfRender.RenderDocument();
			var pageCount = pdfRender.PdfDocument.PageCount - 1;
			pdfRender.PdfDocument.Pages.RemoveAt(pageCount);
			var pdfDocument = pdfRender.PdfDocument;
			var buffer = new byte[1024];
			int bytesRead;

			using (var ms = new MemoryStream())
			{
				pdfDocument.Save(ms, false);
				ms.Position = 0;
				var position = 0;
				var size = (int)Math.Ceiling((double)(ms.Length) / 1024) - 1;

				while ((bytesRead = ms.Read(buffer, 0, buffer.Length)) > 0)
				{
					var pdfChunk = new PdfChunk
					{
						Pozition = position,
						Size = size,
						Buffer = buffer.ToList(),
						BufferSize = bytesRead
					};

					position++;

					using (var serverQueue = new MessageQueue(ServerQueueName, QueueAccessMode.Send))
					{
						var message = new Message(pdfChunk);
						serverQueue.Send(message);
					}
				}
			}
		}

		private void AddImageToDocument(string file)
		{
			var image = section.AddImage(file);

			image.Height = document.DefaultPageSetup.PageHeight;
			image.Width = document.DefaultPageSetup.PageWidth;
			image.ScaleHeight = 0.75;
			image.ScaleWidth = 0.75;

			section.AddPageBreak();
		}

		private bool IsValidFormat(string fileName)
		{
			return Regex.IsMatch(fileName, @"^img_[0-9]{3}.(jpg|png|jpeg)$");
		}

		private int GetIndex(string fileName)
		{
			var match = Regex.Match(fileName, @"[0-9]{3}");

			return match.Success ? int.Parse(match.Value) : -1;
		}

		private void Watcher_Created(object sender, FileSystemEventArgs e)
		{
			newFileEvent.Set();
		}

		public void Start()
		{
			processFilesTask.Start();
			sendSettingsTask.Start();
			controlTask.Start();
			watcher.EnableRaisingEvents = true;
		}

		public void Stop()
		{
			watcher.EnableRaisingEvents = false;
			tokenSource.Cancel();
			stopWaitEvent.Set();
			Task.WaitAll(processFilesTask, sendSettingsTask, controlTask);
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
