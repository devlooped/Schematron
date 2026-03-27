namespace Schematron;

/// <summary>
/// Represents a <c>&lt;let&gt;</c> variable binding in a Schematron schema.
/// </summary>
/// <remarks>
/// A <c>&lt;let&gt;</c> element declares a variable that can be referenced by XPath expressions
/// in sibling and descendant <see cref="Assert"/> and <see cref="Report"/> elements.
/// In ISO Schematron 2025 the optional <c>@as</c> attribute declares the variable's expected type.
/// </remarks>
public class Let
{
    /// <summary>Gets or sets the variable name (value of the <c>@name</c> attribute).</summary>
    public string Name { get; set; } = String.Empty;

    /// <summary>
    /// Gets or sets the variable value expression (value of the <c>@value</c> attribute).
    /// May be <see langword="null"/> when the value is supplied as element content.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the optional declared type for the variable (value of the <c>@as</c> attribute,
    /// introduced in ISO Schematron 2025).
    /// </summary>
    public string? As { get; set; }
}
