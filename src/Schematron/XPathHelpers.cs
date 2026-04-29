using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Schematron;

static class XPathHelpers
{
    public static XPathNavigator CreateRequiredNavigator(this IXPathNavigable source)
        => source.CreateNavigator()
        ?? throw new InvalidOperationException(
            $"Unable to create an {nameof(XPathNavigator)} from '{source.GetType().FullName}'.");

    public static XPathNavigator CurrentOrThrow(this XPathNodeIterator iterator)
        => iterator.Current
        ?? throw new InvalidOperationException("The XPath iterator is not positioned on a current node.");

    public static XmlNode GetRequiredNode(this IHasXmlNode owner)
        => owner.GetNode()
        ?? throw new InvalidOperationException("The XPath navigator does not expose an XML node at the current position.");

    public static XmlSchema ReadRequired(XmlReader reader, ValidationEventHandler? handler)
        => XmlSchema.Read(reader, handler)
        ?? throw new BadSchemaException("No XML Schema information found to read.");
}
