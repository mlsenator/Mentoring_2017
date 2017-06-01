using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace ImageToPdfConverter
{
    public class ImageToPdf
    {
        private string _inDirectory;
        private string _outDirectory;
        private string _processedImagesDirectory;
        private string _unprocessedImagesDirectory;

        private Thread _workingThread;
        private FileSystemWatcher _watcher;
        private ManualResetEvent _workStopped;
        private AutoResetEvent _newFileAdded;
        private PdfHelper _pdfHelper;

        private int _currentImageIndex = -1;

        public ImageToPdf(string inDirectory, string outDirectory)
        {
            _inDirectory = inDirectory;
            _outDirectory = outDirectory;
            _processedImagesDirectory = Path.Combine(_inDirectory, "ProcessedImages");
            _unprocessedImagesDirectory = Path.Combine(_outDirectory, "UnprocessedImages");

            CheckDirectory(_inDirectory);
            CheckDirectory(_outDirectory);
            CheckDirectory(_processedImagesDirectory);
            CheckDirectory(_unprocessedImagesDirectory);

            _pdfHelper = new PdfHelper();
            _workingThread = new Thread(WorkProcedure);
            _workStopped = new ManualResetEvent(false);
            _newFileAdded = new AutoResetEvent(false);

            _watcher = new FileSystemWatcher(_inDirectory);
            _watcher.Created += On_Created;
        }

        public void Start()
        {
            _workingThread.Start();
            _watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            _watcher.EnableRaisingEvents = false;
            _workStopped.Set();

            TrySaveDocument();

            _workingThread.Join();
        }

        private void WorkProcedure()
        {
            _pdfHelper.CreateNewDocument();

            do
            {
                foreach (var filePath in Directory.EnumerateFiles(_inDirectory))
                {
                    if (IsValidFormat(filePath) && TryToOpen(filePath, 3))
                    {
                        var newFilePath = Path.Combine(_processedImagesDirectory, Path.GetFileName(filePath));
                        AddImageToDocument(newFilePath);
                        MoveFile(filePath, newFilePath);

                        if (!_newFileAdded.WaitOne(5000))
                        {
                            TrySaveDocument();
                        }
                    }
                    else
                    {
                        TrySaveDocument();
                    }
                }
            }
            while (WaitHandle.WaitAny(new WaitHandle[] { _workStopped, _newFileAdded }) != 0);
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
            var pattern = @"image_\d+[.](?:png|jpeg|jpg)";
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
                TrySaveDocument();
            }

            _pdfHelper.AddImage(filePath);
            _currentImageIndex = imageIndex;
        }

        private void On_Created(object sender, FileSystemEventArgs e)
        {
            _newFileAdded.Set();
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

        public void TrySaveDocument()
        {
            if (_pdfHelper.Images.Any())
            {
                try
                {
                    _pdfHelper.SaveDocument(_outDirectory);
                }
                catch (Exception)
                {
                    _pdfHelper.Images.ForEach(img => MoveFile(img, Path.Combine(_unprocessedImagesDirectory, Path.GetFileName(img))));
                    _pdfHelper.CreateNewDocument();
                }
            }
        }
    }
}
