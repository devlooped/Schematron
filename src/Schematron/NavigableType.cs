namespace Schematron;

/// <summary>
/// Return type to use as the validation result.
/// </summary>
public enum NavigableType
{
    /// <summary>
    /// Use an <see cref="XmlDocument"/> for validation and return type.
    /// </summary>
    XmlDocument,
    /// <summary>
    /// Use an <see cref="XPathDocument"/> for validation and return type.
    /// </summary>
    XPathDocument,
    /// <summary>
    /// Use the default type, equal to <see cref="XPathDocument"/>, for validation and return type.
    /// </summary>
    Default,
}

