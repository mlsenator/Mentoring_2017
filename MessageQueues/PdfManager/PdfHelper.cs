using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;

namespace PdfManager
{
    public class PdfHelper
    {
        private Document _currentDocument;
        private Section _currentSection;

        public List<string> Images { get; private set; }

        public void AddImage(string filePath)
        {
            var image = _currentSection.AddImage(filePath);
            image.Height = _currentDocument.DefaultPageSetup.PageHeight;
            image.Width = _currentDocument.DefaultPageSetup.PageWidth;
            _currentSection.AddPageBreak();
            Images.Add(filePath);
        }

        public void SaveDocumentUsingChunks(string outDirectory, List<PdfChunk> chunks)
        {
            var path = Path.Combine(outDirectory, $"converted_{DateTime.Now:MM-dd-yy_H-mm-ss}.pdf");

            using (Stream destination = File.Create(Path.Combine(outDirectory, path)))
            {
                foreach (var chunk in chunks)
                {
                    destination.Write(chunk.Buffer.ToArray(), 0, chunk.BufferSize);
                }
            }
        }

        public void CreateNewDocument()
        {
            _currentDocument = new Document();
            _currentSection = _currentDocument.AddSection();
            Images = new List<string>();
        }

        public List<PdfChunk> GetPdfChunks(int chunkSize)
        {
            var render = new PdfDocumentRenderer();
            render.Document = _currentDocument;
            render.RenderDocument();

            var pageCount = render.PdfDocument.PageCount - 1;
            render.PdfDocument.Pages.RemoveAt(pageCount);
            var pdfDocument = render.PdfDocument;
            var buffer = new byte[1024];
            int bytesRead;
            var chunks = new List<PdfChunk>();

            using (var ms = new MemoryStream())
            {
                pdfDocument.Save(ms, false);
                ms.Position = 0;
                var position = 0;
                var size = (int)Math.Ceiling((double)(ms.Length) / chunkSize) - 1;

                while ((bytesRead = ms.Read(buffer, 0, buffer.Length)) > 0)
                {
                    var pdfChunk = new PdfChunk
                    {
                        Position = position,
                        Size = size,
                        Buffer = buffer.ToList(),
                        BufferSize = bytesRead
                    };

                    chunks.Add(pdfChunk);
                    position++;
                }
            }

            return chunks;
        }
    }
}
