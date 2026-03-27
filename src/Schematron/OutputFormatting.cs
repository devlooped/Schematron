namespace Schematron;

/// <summary>
/// Represents the valid output formats.
/// </summary>
/// <remarks>
/// Items will be added to the list to reflect the
/// additional <see cref="Formatters.IFormatter"/> implementations we
/// will develop.
/// </remarks>
/// <author ref="kzu" />
/// <progress amount="100" />
public enum OutputFormatting
{
    /// <summary>Use the <see cref="Formatters.BooleanFormatter"/> class.</summary>
    Boolean,
    /// <summary>Use the <see cref="Formatters.LogFormatter"/> class.</summary>
    Log,
    /// <summary>Use the <see cref="Formatters.SimpleFormatter"/> class.</summary>
    Simple,
    /// <summary>Use the default formatter, which is the <see cref="Log"/>.</summary>
    Default,
    /// <summary>Use the <see cref="Formatters.XmlFormatter"/> class.</summary>
    XML
    /*
    /// <summary>Use the <see cref="Formatters.HtmlFormatter"/> class.</summary>
    HTML
    */
}

