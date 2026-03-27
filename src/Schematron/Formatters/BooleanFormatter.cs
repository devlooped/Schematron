using System.Text;
using System.Xml.XPath;

namespace Schematron.Formatters;

/// <summary>
/// Provides a simple failure message, without any details of specific validation errors.
/// </summary>
public class BooleanFormatter : FormatterBase
{
    /// <summary />
    public BooleanFormatter()
    {
    }

    /// <summary>
    /// Look at <see cref="IFormatter.Format(Schema, XPathNavigator, StringBuilder)"/> documentation.
    /// </summary>
    public override void Format(Schema source, XPathNavigator context, StringBuilder output)
    {
        output.Append("Validation failed!");
    }
}

