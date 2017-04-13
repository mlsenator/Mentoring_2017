using System;
using AdvancedXML.Task2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvancedXMLTest.Task2
{
    [TestClass]
    public class RssTransformerTest
    {
        [TestMethod]
        public void XmlValidatorCorrectXmlTest()
        {
            var transformer = new XmlToRssTransformer();
            var result = transformer.TransformToRss(@"..\..\Task2\Content\books.xml", @"Task2\Content\BookToRSS.xslt");
            Console.WriteLine(result);
        }
    }
}
