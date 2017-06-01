using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfManager;
using ServiceContract;
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
        private string _inDirectory;
        private string _processedImagesDirectory;
        private string _unprocessedImagesDirectory;

        private PdfHelper _pdfHelper;
        private FileSystemWatcher _watcher;
		private Task _processFilesTask;
		private Task _sendSettingsTask;
		private Task _controlTask;
		private AutoResetEvent _newFileAdded;
        private ManualResetEvent _workStopped;

        private int _currentImageIndex = -1;
        private int _timeout;
		private Status _status;

        public DocumentCaptureService(string inDirectory)
		{
            _inDirectory = inDirectory;
            _processedImagesDirectory = Path.Combine(_inDirectory, "ProcessedImages");
            _unprocessedImagesDirectory = Path.Combine(_inDirectory, "UnprocessedImages");

            CheckDirectory(inDirectory);
            CheckDirectory(_processedImagesDirectory);
            CheckDirectory(_unprocessedImagesDirectory);

            CheckMessageQueue(ServiceHelper.ServerQueueName);
            CheckMessageQueue(ServiceHelper.MonitorQueueName);
            CheckMessageQueue(ServiceHelper.ClientQueueName);

			_watcher = new FileSystemWatcher(_inDirectory);
			_watcher.Created += Watcher_Created;
            _pdfHelper = new PdfHelper();

			_timeout = ServiceHelper.DefaultTimeOut;
			_status = Status.Waiting;

			_workStopped = new ManualResetEvent(false);
			_newFileAdded = new AutoResetEvent(false);

			_processFilesTask = new Task(() => ProcessFiles());
			_sendSettingsTask = new Task(() => SendSettings());
			_controlTask = new Task(() => ControlService());
		}

		public void ProcessFiles()
		{
            _pdfHelper.CreateNewDocument();

            do
			{
				_status = Status.Processing;

                foreach (var filePath in Directory.EnumerateFiles(_inDirectory).OrderBy(name => name))
                {
                    if (IsValidFormat(filePath) && TryToOpen(filePath, 3))
                    {
                        var newFilePath = Path.Combine(_processedImagesDirectory, Path.GetFileName(filePath));
                        AddImageToDocument(newFilePath);
                        MoveFile(filePath, newFilePath);
                    }
                    else
                    {
                        TrySendDocument();
                    }
                }

                if (!_newFileAdded.WaitOne(_timeout))
                {
                    TrySendDocument();
                    _status = Status.Waiting;
                }
            }
            while (WaitHandle.WaitAny(new WaitHandle[] { _workStopped, _newFileAdded }) != 0);
        }

		public void SendSettings()
		{
			while (!_workStopped.WaitOne(_timeout))
			{
				var settings = new Settings
				{
					Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
					Status = _status,
					Timeout = _timeout
				};

				using (var serverQueue = new MessageQueue(ServiceHelper.MonitorQueueName))
				{
					var message = new Message(settings);
					serverQueue.Send(message);
				}
			}
		}

		public void ControlService()
		{
			using (var clientQueue = new MessageQueue(ServiceHelper.ClientQueueName))
			{
				clientQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(int) });

				while (!_workStopped.WaitOne(_timeout))
				{
					var asyncReceive = clientQueue.BeginPeek();

					if (WaitHandle.WaitAny(new WaitHandle[] { _workStopped, asyncReceive.AsyncWaitHandle }) == 0)
					{
						break;
					}

					var message = clientQueue.EndPeek(asyncReceive);
					clientQueue.Receive();
					_timeout = (int)message.Body;
				}
			}
		}

		private void Watcher_Created(object sender, FileSystemEventArgs e)
		{
			_newFileAdded.Set();
		}

		public void Start()
		{
			_processFilesTask.Start();
			_sendSettingsTask.Start();
			_controlTask.Start();
			_watcher.EnableRaisingEvents = true;
		}

		public void Stop()
		{
			_watcher.EnableRaisingEvents = false;
            _workStopped.Set();
			Task.WaitAll(_processFilesTask, _sendSettingsTask, _controlTask);
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

        private static void MoveFile(string sourceFilePath, string newFilePath)
        {
            if (File.Exists(newFilePath))
            {
                File.Delete(newFilePath);
            }

            File.Move(sourceFilePath, newFilePath);
        }

        private bool IsValidFormat(string fileName)
        {
            var pattern = @"img_\d+[.](?:png|jpeg|jpg)";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return regex.IsMatch(fileName);
        }

        private void AddImageToDocument(string filePath)
        {
            var pattern = @"\d+";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var imageIndex = int.Parse(regex.Match(Path.GetFileName(filePath)).ToString());

            if (_currentImageIndex >= 0 && imageIndex != _currentImageIndex + 1)
            {
                TrySendDocument();
            }

            _pdfHelper.AddImage(filePath);
            _currentImageIndex = imageIndex;
        }

        public void TrySendDocument()
        {
            if (_pdfHelper.Images.Any())
            {
                try
                {
                    var pdfChunks = _pdfHelper.GetPdfChunks(ServiceHelper.ChunkSize);
                    using (var serverQueue = new MessageQueue(ServiceHelper.ServerQueueName, QueueAccessMode.Send))
                    {
                        foreach (var chunk in pdfChunks)
                        {
                            var message = new Message(chunk);
                            serverQueue.Send(message);
                        }
                    }
                }
                catch (Exception)
                {
                    _pdfHelper.Images.ForEach(img => MoveFile(img, Path.Combine(_unprocessedImagesDirectory, Path.GetFileName(img))));
                }
                finally
                {
                    _pdfHelper.CreateNewDocument();
                }
            }
        }
    }
}
