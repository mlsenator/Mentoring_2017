namespace AdvancedXML.Task3
{
    public interface IXmlToHtmlTransformer
    {
        void TransformToHtml(string xmlPath, string xsltPath, string htmlPath);
    }
}