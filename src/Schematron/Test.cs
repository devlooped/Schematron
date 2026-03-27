using System.Text.RegularExpressions;
using System.Xml.XPath;

namespace Schematron;

/// <summary>
/// Base class for testing units of Schematron, such as Assert or Report elements.
/// </summary>
public abstract class Test : EvaluableExpression
{
    /// <summary />
    protected string _msg = string.Empty;

    /// <summary />
    protected MatchCollection _name_valueofs;

    /// <summary />
    protected XPathExpression?[] _paths;

    /// <summary />
    protected XPathExpression?[] _selects;

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
        // TODO: Check the correct usahe of path and select attributes.
        _name_valueofs = TagExpressions.NameValueOf.Matches(Message);
        int nc = _name_valueofs.Count;
        _paths = new XPathExpression[nc];
        _selects = new XPathExpression[nc];

        for (int i = 0; i < nc; i++)
        {
            // Locate the path attribute and compile it with the DefaultNavigator.
            Match name_valueof = _name_valueofs[i];
            int start = name_valueof.Value.IndexOf("path=");
            // Does it have a path attribute?
            if (start > 0)
            {
                // Skip the path=" string.
                start += 6;
                // If the namespace element is present, end the expression there.
                int end = name_valueof.Value.LastIndexOf("xmlns") - 2;
                if (end < 0)
                    end = name_valueof.Value.LastIndexOf('"');
                string xpath = name_valueof.Value.Substring(start, end - start);
                _paths[i] = Config.DefaultNavigator.Compile(xpath);
                _selects[i] = null;

            }
            else if ((start = name_valueof.Value.IndexOf("select=")) > 0)
            {
                // Skip the select=" string.
                start += 8;
                // If the namespace element is present, end the expression there.
                int end = name_valueof.Value.LastIndexOf("xmlns") - 2;
                if (end < 0)
                    end = name_valueof.Value.LastIndexOf('"');
                string xpath = name_valueof.Value.Substring(start, end - start);
                _selects[i] = Config.DefaultNavigator.Compile(xpath);
                _paths[i] = null;
            }
            else
            {
                _paths[i] = null;
                _selects[i] = null;
            }
        }
    }

    /// <summary />
    public string Message
    {
        get { return _msg; }
        set { _msg = value; }
    }

    /// <summary />
    public MatchCollection NameValueOfExpressions
    {
        get { return _name_valueofs; }
    }

    /// <summary />
    public XPathExpression?[] NamePaths
    {
        get { return _paths; }
    }

    /// <summary />
    public XPathExpression?[] ValueOfSelects
    {
        get { return _selects; }
    }
}

