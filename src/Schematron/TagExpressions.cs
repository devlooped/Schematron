using System.Text.RegularExpressions;

namespace Schematron;

/// <summary />
class TagExpressions
{
    /// <summary>
    /// The compiled regular expression to replace the special <c>name</c> and <c>value</c> tags inside a message.
    /// </summary>
    /// <remarks>
    /// Replaces each instance of <c>name</c> and <c>value</c>tags with the value in the current context element.
    /// </remarks>
    public static Regex NameValueOf;

    public static Regex Emph;
    public static Regex Dir;
    public static Regex Span;
    public static Regex Para;
    public static Regex Any;
    public static Regex AllSchematron;

    /// <summary />
    static TagExpressions()
    {
        // The element declarations can contain the namespace if expanded in a loaded document.
        NameValueOf = new Regex(@"<[^\s>]*\b(name|value-of)\b[^>]*/>", RegexOptions.Compiled);
        Emph = new Regex(@"<[^\s>]*\bemph\b[^>]*>", RegexOptions.Compiled);
        Dir = new Regex(@"<[^\s]*\bdir\b[^>]*>", RegexOptions.Compiled);
        Span = new Regex(@"<[^\s]*\bspan\b[^>]*>", RegexOptions.Compiled);
        Para = new Regex(@"<[^\s]*\bp\b[^>]*>", RegexOptions.Compiled);
        Any = new Regex(@"<[^\s]*[^>]*>", RegexOptions.Compiled);
        // Closing elements don't have an expanded xmlns so they will be matched too.
        // TODO: improve this to avoid removing non-schematron closing elements.
        var nsPattern = "(?:" + Regex.Escape(Schema.LegacyNamespace) + "|" + Regex.Escape(Schema.IsoNamespace) + ")";
        AllSchematron = new Regex(@"<.*\bxmlns\b[^\s]*" + nsPattern + "[^>]*>|</[^>]*>", RegexOptions.Compiled);
    }

    TagExpressions()
    {
    }
}

