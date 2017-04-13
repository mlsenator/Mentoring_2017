using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace AdvancedXML.Task2
{
    public class XmlToRssTransformer : IXmlToRssTransformer
    {
        public string TransformToRss(string xmlPath, string xsltPath)
        {
            if (string.IsNullOrEmpty(xmlPath))
            {
                throw new ArgumentNullException(xmlPath);
            }

            if (string.IsNullOrEmpty(xsltPath))
            {
                throw new ArgumentNullException(xsltPath);
            }

            var sb = new StringBuilder();
            var transformer = new XslCompiledTransform();
            transformer.Load(xsltPath, new XsltSettings(true, true), null);

            using (var xmlWriter = XmlWriter.Create(sb, transformer.OutputSettings))
            {
                transformer.Transform(xmlPath, xmlWriter);
            }

            return sb.ToString();
        }
    }
}
