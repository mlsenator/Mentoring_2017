using System;
using System.IO;
using AdvancedXML.Task3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvancedXMLTest.Task3
{
    [TestClass]
    public class RssTransformerTest
    {
        [TestMethod]
        public void XmlValidatorCorrectXmlTest()
        {
            var transformer = new XmlToHtmlTransformer();
            var htmlPath = @"D:\_temp\convertedXml.html";
            transformer.TransformToHtml(@"..\..\Task3\Content\books.xml", @"Task3\Content\BookToHTML.xslt", htmlPath);
            Assert.IsTrue(File.Exists(htmlPath));
        }
    }
}