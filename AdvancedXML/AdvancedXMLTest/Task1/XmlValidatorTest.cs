using System;
using AdvancedXML.Task1;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvancedXMLTest.Task1
{
    [TestClass]
    public class XmlValidatorTest
    {
        [TestMethod]
        public void XmlValidatorCorrectXmlTest()
        {
            var validator = new XmlValidator();
            var validationResult = validator.Validate(@"..\..\Task1\Content\books.xml", @"Task1\Content\BooksScheme.xsd", "http://library.by/catalog");
            Assert.IsTrue(validationResult);
        }

        [TestMethod]
        public void XmlValidatorIncorrectXmlTest()
        {
            var validator = new XmlValidator();
            var validationResult = validator.Validate(@"..\..\Task1\Content\booksWithErrors.xml", @"Task1\Content\BooksScheme.xsd", "http://library.by/catalog");
            Assert.IsFalse(validationResult);

            foreach (var error in validator.Errors)
            {
                Console.WriteLine(error);
            }
        }
    }
}
