using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Schematron.Formatters;

/// <summary>
/// Provides an Xml output from validation.
/// </summary>
public class XmlFormatter : FormatterBase
{
    /// <summary>
    /// Namespace of generated output.
    /// </summary>
    public const string OutputNamespace = "http://sourceforge.net/projects/dotnetopensrc/schematron";

    /// <summary>
    /// Look at <see cref="IFormatter.Format(Test, XPathNavigator, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(Test source, XPathNavigator context, StringBuilder output)
    {
        var msg = source.Message;
        var writer = new XmlTextWriter(new StringWriter(output))
        {
            //Temporary disable namespace support.
            Namespaces = false
        };

        // Start element declaration.
        writer.WriteStartElement("message");

        if (!string.IsNullOrEmpty(source.Severity))
            writer.WriteAttributeString("severity", source.Severity);
        if (!string.IsNullOrEmpty(source.Role))
            writer.WriteAttributeString("role", source.Role);
        if (source.Flag.Count > 0)
            writer.WriteAttributeString("flag", string.Join(" ", source.Flag));

        msg = FormatMessage(source, context, msg).ToString();

        // Finally remove any non-name schematron tag in the message.
        var res = TagExpressions.AllSchematron.Replace(msg, string.Empty);

        //Accumulate namespaces found during traversal of node for its position.
        var ns = new Hashtable();

        // Write <text> element.
        writer.WriteElementString("text", res);
        // Write <path> element.
        writer.WriteElementString("path", FormattingUtils.GetFullNodePosition(context.Clone(), string.Empty, source, ns));
        // Write <summary> element.
        //writer.WriteElementString("summary", FormattingUtils.GetNodeSummary(context, ns, String.Empty));
        writer.WriteStartElement("summary");
        writer.WriteRaw(FormattingUtils.GetNodeSummary(context, ns, string.Empty));
        writer.WriteEndElement();

        // Write <position> element.
        if (context is IXmlLineInfo info)
        {
            writer.WriteStartElement("position");
            writer.WriteAttributeString("line", info.LineNumber.ToString());
            writer.WriteAttributeString("column", info.LinePosition.ToString());
            writer.WriteEndElement();
        }

        // Close <message> element.
        writer.WriteEndElement();
        writer.Flush();
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(Rule, XPathNavigator, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(Rule source, XPathNavigator context, StringBuilder output)
    {
        var res = "<rule context=\"" + source.Context + "\" ";
        if (source.Id != string.Empty) res += "id=\"" + source.Id + "\" ";
        res += ">";

        output.Insert(0, res);
        output.Append("</rule>");
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(Pattern, XPathNavigator, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(Pattern source, XPathNavigator context, StringBuilder output)
    {
        var elemName = source is Group ? "group" : "pattern";
        var res = "<" + elemName + " name=\"" + source.Name + "\" ";
        if (source.Id != string.Empty) res += "id=\"" + source.Id + "\" ";
        res += ">";

        output.Insert(0, res);
        output.Append("</" + elemName + ">");
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(Phase, XPathNavigator, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(Phase source, XPathNavigator context, StringBuilder output)
    {
        output.Insert(0, "<phase id=\"" + source.Id + "\">");
        output.Append("</phase>");
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(Schema, XPathNavigator, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(Schema source, XPathNavigator context, StringBuilder output)
    {
        var sb = new StringBuilder();
        var writer = new XmlTextWriter(new StringWriter(sb));
        writer.WriteStartElement("schema ");

        foreach (string prefix in source.NsManager)
        {
            if (!prefix.StartsWith("xml"))
                writer.WriteAttributeString("xmlns", prefix, null,
                    source.NsManager.LookupNamespace(source.NsManager.NameTable.Get(prefix)));
        }

        if (source.Title != string.Empty) writer.WriteAttributeString("title", source.Title);
        if (source.SchematronEdition != string.Empty) writer.WriteAttributeString("schematronEdition", source.SchematronEdition);

        writer.WriteRaw(output.ToString());
        writer.WriteEndElement();
        writer.Flush();
        output.Remove(0, output.Length);
        output.Append(sb.ToString());
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(ValidationEventArgs, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(ValidationEventArgs source, StringBuilder output)
    {
        var writer = new XmlTextWriter(new StringWriter(output));
        // Start element declaration.
        writer.WriteStartElement("message");

        // Write <text> element.
        writer.WriteElementString("text", FormattingUtils.XmlErrorPosition.Replace(source.Message, string.Empty));

        // Write <position> element.
        writer.WriteStartElement("position");
        writer.WriteAttributeString("line", source.Exception.LineNumber.ToString());
        writer.WriteAttributeString("column", source.Exception.LinePosition.ToString());
        writer.WriteEndElement();

        // Close <message> element.
        writer.WriteEndElement();
        writer.Flush();
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(XmlSchemaSet, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(XmlSchemaSet schemas, StringBuilder output)
    {
        var sb = new StringBuilder();
        var writer = new XmlTextWriter(new StringWriter(sb));

        foreach (XmlSchema sch in schemas.Schemas())
        {
            writer.WriteStartElement("xmlSchema");
            writer.WriteAttributeString("id", sch.Id);
            writer.WriteAttributeString("version", sch.Version);
            writer.WriteAttributeString("targetNamespace", sch.TargetNamespace);
            writer.WriteEndElement();
        }
        writer.Flush();
        output.Insert(0, sb.ToString());
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(SchemaCollection, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(SchemaCollection schemas, StringBuilder output)
    {
        // Enclose putput in an <schematron> element.
        output.Insert(0, "<schematron>");
        output.Append("</schematron>");
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(XmlReader, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(XmlReader reader, StringBuilder output)
    {
        // Enclose messages in an <xml> element.
        output.Insert(0, "<xml>");
        output.Append("</xml>");
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(StringBuilder output)
    {
        var sb = new StringBuilder();
        var writer = new XmlTextWriter(new StringWriter(sb));

        writer.WriteStartElement("output", OutputNamespace);
        writer.WriteRaw(output.ToString());
        writer.WriteEndElement();
        writer.Flush();

        // Clean output.
        output.Remove(0, output.Length);

        // Create indented output.
        writer = new XmlTextWriter(new StringWriter(output))
        {
            Formatting = Formatting.Indented
        };
        writer.WriteStartDocument();
        writer.WriteNode(new XmlTextReader(new StringReader(sb.ToString())), false);
        writer.WriteEndDocument();
        writer.Flush();
    }
}

