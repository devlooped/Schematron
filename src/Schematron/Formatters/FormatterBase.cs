using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Schematron.Formatters;

/// <summary>
/// Look at <see cref="IFormatter"/> documentation.
/// </summary>
public abstract class FormatterBase : IFormatter
{
    /// <summary />
    public FormatterBase()
    {
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(Test, XPathNavigator, StringBuilder)"/> documentation.
    /// </summary>
    public virtual void Format(Test source, XPathNavigator context, StringBuilder output)
    {
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(Rule, XPathNavigator, StringBuilder)"/> documentation.
    /// </summary>
    public virtual void Format(Rule source, XPathNavigator context, StringBuilder output)
    {
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(Pattern, XPathNavigator, StringBuilder)"/> documentation.
    /// </summary>
    public virtual void Format(Pattern source, XPathNavigator context, StringBuilder output)
    {
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(Phase, XPathNavigator, StringBuilder)"/> documentation.
    /// </summary>
    public virtual void Format(Phase source, XPathNavigator context, StringBuilder output)
    {
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(Schema, XPathNavigator, StringBuilder)"/> documentation.
    /// </summary>
    public virtual void Format(Schema source, XPathNavigator context, StringBuilder output)
    {
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(XmlReader, StringBuilder)"/> documentation.
    /// </summary>
    public virtual void Format(XmlReader reader, StringBuilder output)
    {
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(ValidationEventArgs, StringBuilder)"/> documentation.
    /// </summary>
    public virtual void Format(ValidationEventArgs source, StringBuilder output)
    {
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(XmlSchemaSet, StringBuilder)"/> documentation.
    /// </summary>
    public virtual void Format(XmlSchemaSet schemas, StringBuilder output)
    {
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(SchemaCollection, StringBuilder)"/> documentation.
    /// </summary>
    public virtual void Format(SchemaCollection schemas, StringBuilder output)
    {
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(StringBuilder)"/> documentation.
    /// </summary>
    public virtual void Format(StringBuilder output)
    {
    }

    /// <summary>Formats the message from a test, resolving <c>&lt;name&gt;</c> and <c>&lt;value-of&gt;</c> expressions against the given context.</summary>
    protected static StringBuilder FormatMessage(Test source, XPathNavigator context, string msg)
    {
        var sb = new StringBuilder();
        XPathExpression? nameExpr;
        XPathExpression? selectExpr;

        // As we move on, we have to append starting from the last point,
        // skipping the <name> and <value-of> expressions: Substring(offset, name.Index - offset).
        int offset = 0;

        for (int i = 0; i < source.NameValueOfExpressions.Count; i++)
        {
            System.Text.RegularExpressions.Match name = source.NameValueOfExpressions[i];
            nameExpr = source.NamePaths[i];
            selectExpr = source.ValueOfSelects[i];

            // Append the text without the expression.
            sb.Append(msg.Substring(offset, name.Index - offset));

            // Does the name element have a path attribute?
            if (nameExpr != null)
            {
                nameExpr.SetContext(source.GetContext()!);

                string? result = null;
                if (nameExpr.ReturnType == XPathResultType.NodeSet)
                {
                    // It the result of the expression is a nodeset, we only get the element
                    // name of the first node, which is compatible with XSLT implementation.
                    var nodes = (XPathNodeIterator)context.Evaluate(nameExpr);
                    if (nodes.MoveNext())
                        result = nodes.Current.Name;
                }
                else
                    result = context.Evaluate(nameExpr) as string;

                if (result != null)
                    sb.Append(result);
            }
            // Does the value-of element have a select attribute?
            else if (selectExpr != null)
            {
                selectExpr.SetContext(source.GetContext()!);

                string? result = null;
                if (selectExpr.ReturnType == XPathResultType.NodeSet)
                {
                    var nodes = (XPathNodeIterator)context.Evaluate(selectExpr);
                    result = String.Empty;
                    while (nodes.MoveNext())
                        result += nodes.Current.Value;
                }
                else
                    result = context.Evaluate(selectExpr) as string;

                if (result != null)
                    sb.Append(result);
            }
            // If there is no path or select expression, there is an empty <name> element.
            else
                sb.Append(context.Name);

            offset = name.Index + name.Length;
        }

        sb.Append(msg.Substring(offset));
        return sb;
    }

}

