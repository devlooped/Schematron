using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Schematron.Formatters;

/// <summary />
public class FormattingUtils
{
    static readonly Regex normalize;
    static readonly Regex removeprefix;

    static FormattingUtils()
    {
        normalize = new Regex(@"\s+", RegexOptions.Compiled);
        removeprefix = new Regex(" .*", RegexOptions.Compiled);

        // Match the position suffix appended by XmlSchemaException.Message:
        // " An error occurred at {uri}({line}, {col})."
        XmlErrorPosition = new Regex(@" An error occurred at [^(]*\(\d+,\s*\d+\)\.", RegexOptions.Compiled);
    }

    FormattingUtils()
    {
    }

    static readonly XPathExpression precedingSiblingsExpr = XPathExpression.Compile("preceding-sibling::*");

    /// <summary>
    /// Returns the full path to the context node. Clone the navigator to avoid loosing positioning.
    /// </summary>
    public static string GetFullNodePosition(XPathNavigator context, string previous, Test source)
    {
        return GetFullNodePosition(context, previous, source, []);
    }

    /// <summary>
    /// Returns the full path to the context node. Clone the navigator to avoid loosing positioning.
    /// </summary>
    /// <remarks>
    /// Cloning is not performed inside this method because it is called recursively.
    /// Keeping positioning is only relevant to the calling procedure, not subsequent
    /// recursive calls. This way we avoid creating unnecessary objects.
    /// </remarks>
    public static string GetFullNodePosition(XPathNavigator context, string previous, Test source, Hashtable namespaces)
    {
        var curr = context.Name;
        var pref = string.Empty;

        if (context.NamespaceURI != string.Empty)
        {
            if (context.Prefix == string.Empty)
            {
                pref = source.GetContext()!.LookupPrefix(source.GetContext()!.NameTable.Get(context.NamespaceURI));
            }
            else
            {
                pref = context.Prefix;
            }

            if (!namespaces.ContainsKey(context.NamespaceURI))
            {
                namespaces.Add(context.NamespaceURI, pref ?? "");
            }
            else if (((string)namespaces[context.NamespaceURI]) != pref &&
                !namespaces.ContainsKey(context.NamespaceURI + ":" + pref))
            {
                namespaces.Add(context.NamespaceURI + " " + pref, pref);
            }
        }

        var sibs = 1;
        foreach (XPathNavigator prev in context.Select(precedingSiblingsExpr))
            if (prev.Name == curr) sibs++;

        if (context.MoveToParent())
        {
            var sb = new StringBuilder();
            sb.Append("/");
            if (pref != string.Empty) sb.Append(pref).Append(":");
            sb.Append(curr).Append("[").Append(sibs).Append("]").Append(previous);
            return GetFullNodePosition(context, sb.ToString(), source, namespaces);
        }
        else
        {
            return previous;
        }
    }

    /// <summary>
    /// Returns line positioning information if supported by the XPathNavigator implementation.
    /// </summary>
    public static string GetPositionInFile(XPathNavigator context, string spacing)
    {
        if (context is not IXmlLineInfo)
            return string.Empty;

        var sb = new StringBuilder();
        sb.Append(spacing);

        var info = (IXmlLineInfo)context;

        sb.Append("(Line: ").Append(info.LineNumber);
        sb.Append(", Column: ").Append(info.LinePosition).Append(")");

        return sb.ToString();
    }

    /// <summary>
    /// Returns abreviated node information, including attribute values.
    /// </summary>
    public static string GetNodeSummary(XPathNavigator context, string spacing)
    {
        return GetNodeSummary(context, [], spacing);
    }

    /// <summary>
    /// Returns abreviated node information, including attribute values.
    /// </summary>
    /// <remarks>
    /// The namespaces param is optionally filled in <see cref="GetFullNodePosition(XPathNavigator, string, Test, Hashtable)"/>.
    /// </remarks>
    public static string GetNodeSummary(XPathNavigator context, Hashtable namespaces, string spacing)
    {
        var ctx = context.Clone();
        var sb = new StringBuilder();

        sb.Append(spacing).Append("<");

        // Get the element name
        XmlQualifiedName name;
        if (ctx.NamespaceURI != string.Empty)
            name = new XmlQualifiedName(ctx.LocalName, namespaces[ctx.NamespaceURI].ToString());
        else
            name = new XmlQualifiedName(ctx.LocalName);

        sb.Append(name.ToString());
        if (ctx.MoveToFirstAttribute())
        {
            do
            {
                sb.Append(" ").Append(ctx.LocalName);
                sb.Append("=\"").Append(ctx.Value);
                sb.Append("\"");
            } while (ctx.MoveToNextAttribute());
        }
        sb.Append(">...</");
        sb.Append(name.ToString()).Append(">");
        return sb.ToString();
    }

    /// <summary>
    /// Outputs the xmlns declaration for each namespace found in the parameter.
    /// </summary>
    public static string GetNamespaceSummary(XPathNavigator context, Hashtable namespaces, string spacing)
    {
        if (namespaces.Count == 0) return string.Empty;

        var sb = new StringBuilder();
        var keys = namespaces.Keys;
        var pref = string.Empty;

        foreach (var key in keys)
        {
            sb.Append(spacing).Append("xmlns");
            pref = namespaces[key].ToString();

            if (pref != string.Empty)
                sb.Append(":").Append(namespaces[key]);

            sb.Append("=\"");

            if (pref != string.Empty)
                sb.Append(removeprefix.Replace(key.ToString(), string.Empty));
            else
                sb.Append(key);

            sb.Append("\" ");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Allows to match the string stating the node position from System.Xml error messages.
    /// </summary>
    /// <remarks>
    /// This regular expression is used to remove the node position from the validation error
    /// message, to maintain consistency with schematron messages.
    /// </remarks>
    public static Regex XmlErrorPosition;

    /// <summary>
    /// Returns a decoded string, with spaces trimmed.
    /// </summary>
    public static string NormalizeString(string input)
    {
        // Account for encoded strings, such as &lt; (<) and &gt (>).
        return System.Web.HttpUtility.HtmlDecode(
            normalize.Replace(input, " ").Trim());
    }
}

