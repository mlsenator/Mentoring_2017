using System.Collections.Generic;

namespace AdvancedXML.Task1
{
    public interface IXmlValidator
    {
        List<string> Errors { get; }
        bool Validate(string xmlPath, string schemaPath, string targetNamespace);
    }
}