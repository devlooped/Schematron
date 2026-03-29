using System.Text.RegularExpressions;

namespace Schematron;

/// <summary />
partial class TagExpressions
{
    /// <summary>
    /// The compiled regular expression to replace the special <c>name</c> and <c>value</c> tags inside a message.
    /// </summary>
    /// <remarks>
    /// Replaces each instance of <c>name</c> and <c>value</c>tags with the value in the current context element.
    /// </remarks>
    // The element declarations can contain the namespace if expanded in a loaded document.
    [GeneratedRegex(@"<[^\s>]*\b(name|value-of)\b[^>]*/>")]
    public static partial Regex NameValueOf();

    [GeneratedRegex(@"<[^\s>]*\bemph\b[^>]*>")]
    public static partial Regex Emph();

    [GeneratedRegex(@"<[^\s]*\bdir\b[^>]*>")]
    public static partial Regex Dir();

    [GeneratedRegex(@"<[^\s]*\bspan\b[^>]*>")]
    public static partial Regex Span();

    [GeneratedRegex(@"<[^\s]*\bp\b[^>]*>")]
    public static partial Regex Para();

    [GeneratedRegex(@"<[^\s]*[^>]*>")]
    public static partial Regex Any();

    // Closing elements don't have an expanded xmlns so they will be matched too.
    // TODO: improve this to avoid removing non-schematron closing elements.
    // Pattern derived from Schema.LegacyNamespace and Schema.IsoNamespace (with Regex.Escape applied).
    [GeneratedRegex(@"<.*\bxmlns\b[^\s]*(?:http://www\.ascc\.net/xml/schematron|http://purl\.oclc\.org/dsdl/schematron)[^>]*>|</[^>]*>")]
    public static partial Regex AllSchematron();
}

