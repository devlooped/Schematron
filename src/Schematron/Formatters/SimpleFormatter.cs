using System.Collections;
using System.Text;
using System.Xml.XPath;

namespace Schematron.Formatters;

/// <summary>
/// Provides a simplified log of errors.
/// </summary>
/// <remarks>
/// Similar output as <see cref="LogFormatter"/>, but doesn't provide
/// node position in file and namespace summary text.
/// </remarks>
public class SimpleFormatter : LogFormatter
{
    /// <summary />
    public SimpleFormatter()
    {
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(Test, XPathNavigator, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(Test source, XPathNavigator context, StringBuilder output)
    {
        StringBuilder sb = FormatMessage(source, context, source.Message);

        if (source is Assert)
            sb.Insert(0, "\tAssert fails: ");
        else
            sb.Insert(0, "\tReport: ");

        var ns = new Hashtable();
        sb.Append("\r\n\tAt: " + FormattingUtils.GetFullNodePosition(context.Clone(), String.Empty, source, ns));
        sb.Append("\r\n");

        output.Append(sb.ToString());
    }
}

