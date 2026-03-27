using System.Collections.ObjectModel;

namespace Schematron;

/// <summary>A keyed collection of <see cref="Diagnostic"/> elements, indexed by <see cref="Diagnostic.Id"/>.</summary>
public class DiagnosticCollection : KeyedCollection<string, Diagnostic>
{
    /// <inheritdoc />
    protected override string GetKeyForItem(Diagnostic item) => item.Id;
}
