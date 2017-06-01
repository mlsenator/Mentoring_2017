using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;

namespace ImageToPdfConverter
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

        public void SaveDocument(string outDirectory)
        {
            if (_currentDocument != null)
            {
                var render = new PdfDocumentRenderer();
                render.Document = _currentDocument;
                render.RenderDocument();
                render.Save(Path.Combine(outDirectory, $"converted_{DateTime.Now:MM-dd-yy_H-mm-ss}.pdf"));
                _currentDocument = null;
                _currentSection = null;
            }
        }

        public void CreateNewDocument()
        {
            _currentDocument = new Document();
            _currentSection = _currentDocument.AddSection();
            Images = new List<string>();
        }
    }
}
