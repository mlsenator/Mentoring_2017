using System;
using System.Xml.Xsl;

namespace AdvancedXML.Task3
{
    public class XmlToHtmlTransformer : IXmlToHtmlTransformer
    {
        public void TransformToHtml(string xmlPath, string xsltPath, string htmlPath)
        {
            if (string.IsNullOrEmpty(xmlPath))
            {
                throw new ArgumentNullException(xmlPath);
            }

            if (string.IsNullOrEmpty(xsltPath))
            {
                throw new ArgumentNullException(xsltPath);
            }

            if (string.IsNullOrEmpty(htmlPath))
            {
                throw new ArgumentNullException(htmlPath);
            }

            var xsl = new XslCompiledTransform();
            xsl.Load(xsltPath, new XsltSettings(true, true), null);
            xsl.Transform(xmlPath, htmlPath);
        }
    }
}