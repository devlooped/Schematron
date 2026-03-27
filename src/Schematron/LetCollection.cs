using System.Collections.ObjectModel;

namespace Schematron;

/// <summary>
/// A keyed collection of <see cref="Let"/> variable bindings.
/// </summary>
public class LetCollection : KeyedCollection<string, Let>
{
    /// <inheritdoc />
    protected override string GetKeyForItem(Let item) => item.Name;
}
