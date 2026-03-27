namespace Schematron;

/// <summary>
/// Represents a <c>&lt;param&gt;</c> element used in abstract pattern instantiation.
/// </summary>
/// <remarks>
/// Abstract patterns declare rules using <c>$name</c> placeholders. Concrete patterns that
/// reference an abstract pattern via <c>@is-a</c> supply parameter values through
/// <c>&lt;param name="..." value="..."/&gt;</c> children. The loader replaces placeholder
/// tokens in the instantiated rules.
/// </remarks>
public class Param
{
    /// <summary>Gets or sets the parameter name (matches the <c>$name</c> placeholder in the abstract pattern).</summary>
    public string Name { get; set; } = String.Empty;

    /// <summary>Gets or sets the substitution value.</summary>
    public string Value { get; set; } = String.Empty;
}
