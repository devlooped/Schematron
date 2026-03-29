namespace Schematron;

/// <summary>
/// Represents a <c>&lt;diagnostic&gt;</c> element in a Schematron schema.
/// </summary>
/// <remarks>
/// A diagnostic provides supplementary human-readable information associated with a failed
/// <see cref="Assert"/> or successful <see cref="Report"/> via the <c>@diagnostics</c> IDREFS
/// attribute. The <c>&lt;diagnostic&gt;</c> is stored at the schema level and referenced by id.
/// </remarks>
/// <param name="Id">Gets or sets the unique ID of this diagnostic (value of the <c>@id</c> attribute).</param>
/// <param name="Message">Gets or sets the raw text content / message of this diagnostic element.</param>
public record Diagnostic(string Id, string Message);