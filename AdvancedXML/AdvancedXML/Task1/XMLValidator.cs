using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace AdvancedXML.Task1
{
    public class XmlValidator : IXmlValidator
    {
        public List<string> Errors { get; private set; }

        public bool Validate(string xmlPath, string schemaPath, string targetNamespace)
        {
            if (string.IsNullOrEmpty(xmlPath))
            {
                throw new ArgumentNullException(xmlPath);
            }

            if (string.IsNullOrEmpty(schemaPath))
            {
                throw new ArgumentNullException(schemaPath);
            }

            if (string.IsNullOrEmpty(targetNamespace))
            {
                throw new ArgumentNullException(targetNamespace);
            }

            var settings = new XmlReaderSettings();
            settings.Schemas.Add(targetNamespace, schemaPath);
            settings.ValidationEventHandler += ValidationEventHandler;
            settings.ValidationFlags = settings.ValidationFlags | XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationType = ValidationType.Schema;

            Errors = new List<string>();
            var reader = XmlReader.Create(xmlPath, settings);

            while (reader.Read()) ;

            return !Errors.Any();
        }

        public virtual void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            Errors.Add($"Error. Line: {e.Exception.LineNumber}; Position: {e.Exception.LinePosition}. Message: {e.Exception.Message}");
        }
    }
}