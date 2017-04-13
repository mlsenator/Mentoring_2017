namespace AdvancedXML.Task2
{
    public interface IXmlToRssTransformer
    {
        string TransformToRss(string xmlPath, string xsltPath);
    }
}