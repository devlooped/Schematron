using System.Text.RegularExpressions;
using System.Xml.XPath;

namespace Schematron;

/// <summary>
/// Base class for testing units of Schematron, such as Assert or Report elements.
/// </summary>
public abstract class Test : EvaluableExpression
{
    /// <summary />
    /// <param name="test"></param>
    /// <param name="message"></param>
    public Test(string test, string message) : base(test)
    {
        if (ReturnType != XPathResultType.Boolean &&
            ReturnType != XPathResultType.NodeSet)
            throw new InvalidExpressionException("Test expression doesn't evaluate to a boolean or nodeset result: " + test);

        Message = Formatters.FormattingUtils.NormalizeString(message);

        // Save <name> and <value-of> tags in the message and their paths / selects in their compiled form.
        // TODO: see if we can work with the XML in the message, instead of using RE.
        // TODO: Check the correct usage of path and select attributes.
        NameValueOfExpressions = TagExpressions.NameValueOf.Matches(Message);
        var nc = NameValueOfExpressions.Count;
        NamePaths = new XPathExpression[nc];
        ValueOfSelects = new XPathExpression[nc];

        for (var i = 0; i < nc; i++)
        {
            var name_valueof = NameValueOfExpressions[i];
            var start = name_valueof.Value.IndexOf("path=");
            if (start > 0)
            {
                start += 6;
                var end = name_valueof.Value.LastIndexOf("xmlns") - 2;
                if (end < 0)
                    end = name_valueof.Value.LastIndexOf('"');
                var xpath = name_valueof.Value[start..end];
                NamePaths[i] = Config.DefaultNavigator.Compile(xpath);
                ValueOfSelects[i] = null;
            }
            else if ((start = name_valueof.Value.IndexOf("select=")) > 0)
            {
                start += 8;
                var end = name_valueof.Value.LastIndexOf("xmlns") - 2;
                if (end < 0)
                    end = name_valueof.Value.LastIndexOf('"');
                var xpath = name_valueof.Value[start..end];
                ValueOfSelects[i] = Config.DefaultNavigator.Compile(xpath);
                NamePaths[i] = null;
            }
            else
            {
                NamePaths[i] = null;
                ValueOfSelects[i] = null;
            }
        }
    }

    /// <summary />
    public string Message { get; set; }

    /// <summary>Gets or sets the optional identifier for this test (<c>@id</c> attribute).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the role of this test (<c>@role</c> attribute).</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the flag tokens for this test (<c>@flag</c> attribute).
    /// In ISO Schematron 2025 <c>@flag</c> is a whitespace-separated list of tokens.
    /// </summary>
    public IReadOnlyList<string> Flag { get; set; } = [];

    /// <summary>Gets or sets the severity of this test (<c>@severity</c> attribute).</summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>Gets or sets the diagnostic IDs referenced by this test (<c>@diagnostics</c> attribute).</summary>
    public IReadOnlyList<string> DiagnosticRefs { get; set; } = [];

    /// <summary />
    public MatchCollection NameValueOfExpressions { get; protected set; }

    /// <summary />
    public XPathExpression?[] NamePaths { get; protected set; } = [];

    /// <summary />
    public XPathExpression?[] ValueOfSelects { get; protected set; } = [];
}