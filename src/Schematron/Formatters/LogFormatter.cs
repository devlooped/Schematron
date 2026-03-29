using System.Collections;
using System.Text;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Schematron.Formatters;

/// <summary>
/// Provides a complete log of validation errors in text format.
/// </summary>
public class LogFormatter : FormatterBase
{
    /// <summary>
    /// Look at <see cref="IFormatter.Format(Test, XPathNavigator, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(Test source, XPathNavigator context, StringBuilder output)
    {
        var sb = FormatMessage(source, context, source.Message);
        // Finally remove any non-name schematron tag in the message.
        var res = TagExpressions.AllSchematron.Replace(sb.ToString(), string.Empty);
        sb = new StringBuilder();
        if (source is Assert)
        {
            sb.Append("\tAssert fails: ");
        }
        else
        {
            sb.Append("\tReport: ");
        }
        if (!string.IsNullOrEmpty(source.Severity))
            sb.Append("[" + source.Severity + "] ");
        sb.Append(res);

        //Accumulate namespaces found during traversal of node for its position.
        var ns = new Hashtable();

        sb.Append("\r\n\tAt: ").Append(FormattingUtils.GetFullNodePosition(context.Clone(), string.Empty, source, ns));
        sb.Append(FormattingUtils.GetNodeSummary(context, ns, "\r\n\t    "));

        res = FormattingUtils.GetPositionInFile(context, "\r\n\t    ");
        if (res != string.Empty) sb.Append(res);

        res = FormattingUtils.GetNamespaceSummary(context, ns, "\r\n\t    ");
        if (res != string.Empty) sb.Append(res);
        sb.Append("\r\n");
        output.Append(sb.ToString());
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(Pattern, XPathNavigator, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(Pattern source, XPathNavigator context, StringBuilder output)
    {
        output.Insert(0, "    From pattern \"" + source.Name + "\"\r\n");
        output.Append("\r\n");
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(Schema, XPathNavigator, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(Schema source, XPathNavigator context, StringBuilder output)
    {
        if (source.Title != string.Empty)
            output.Insert(0, source.Title + "\r\n");
        else
            output.Insert(0, "Results from Schematron validation\r\n");

        output.Append("\r\n");
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(ValidationEventArgs, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(ValidationEventArgs source, StringBuilder output)
    {
        output.Append("  Error: ");
        output.Append(FormattingUtils.XmlErrorPosition.Replace(source.Message, string.Empty));
        output.Append("\r\n  At: (Line: ").Append(source.Exception.LineNumber);
        output.Append(", Column: ").Append(source.Exception.LinePosition).Append(")\r\n");
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(XmlSchemaSet, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(XmlSchemaSet schemas, StringBuilder output)
    {
        output.Insert(0, "Results from XML Schema validation:\r\n");
        output.Append("\r\n");
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(SchemaCollection, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(SchemaCollection schemas, StringBuilder output)
    {
        output.Insert(0, "Results from Schematron validation:\r\n");
        output.Append("\r\n");
    }
}

