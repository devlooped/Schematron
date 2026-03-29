using System.Text.RegularExpressions;

namespace Schematron;

/// <summary />
partial class TagExpressions
{
#if NET8_0_OR_GREATER
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
    // Pattern derived from Regex.Escape(Schema.LegacyNamespace) and Regex.Escape(Schema.IsoNamespace).
    // If those constants ever change, update the hardcoded escaped values here to match.
    [GeneratedRegex(@"<.*\bxmlns\b[^\s]*(?:http://www\.ascc\.net/xml/schematron|http://purl\.oclc\.org/dsdl/schematron)[^>]*>|</[^>]*>")]
    public static partial Regex AllSchematron();
#else
    /// <summary>
    /// The compiled regular expression to replace the special <c>name</c> and <c>value</c> tags inside a message.
    /// </summary>
    /// <remarks>
    /// Replaces each instance of <c>name</c> and <c>value</c>tags with the value in the current context element.
    /// </remarks>
    // The element declarations can contain the namespace if expanded in a loaded document.
    static readonly Regex _nameValueOf = new Regex(@"<[^\s>]*\b(name|value-of)\b[^>]*/>", RegexOptions.Compiled);
    public static Regex NameValueOf() => _nameValueOf;

    static readonly Regex _emph = new Regex(@"<[^\s>]*\bemph\b[^>]*>", RegexOptions.Compiled);
    public static Regex Emph() => _emph;

    static readonly Regex _dir = new Regex(@"<[^\s]*\bdir\b[^>]*>", RegexOptions.Compiled);
    public static Regex Dir() => _dir;

    static readonly Regex _span = new Regex(@"<[^\s]*\bspan\b[^>]*>", RegexOptions.Compiled);
    public static Regex Span() => _span;

    static readonly Regex _para = new Regex(@"<[^\s]*\bp\b[^>]*>", RegexOptions.Compiled);
    public static Regex Para() => _para;

    static readonly Regex _any = new Regex(@"<[^\s]*[^>]*>", RegexOptions.Compiled);
    public static Regex Any() => _any;

    // Closing elements don't have an expanded xmlns so they will be matched too.
    // TODO: improve this to avoid removing non-schematron closing elements.
    static readonly Regex _allSchematron = new Regex(
        @"<.*\bxmlns\b[^\s]*(?:" + Regex.Escape(Schema.LegacyNamespace) + "|" + Regex.Escape(Schema.IsoNamespace) + @")[^>]*>|</[^>]*>",
        RegexOptions.Compiled);
    public static Regex AllSchematron() => _allSchematron;
#endif
}

