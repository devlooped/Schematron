using System.Collections.ObjectModel;

namespace Schematron;

/// <summary>A keyed collection of <see cref="Param"/> elements, indexed by <see cref="Param.Name"/>.</summary>
public class ParamCollection : KeyedCollection<string, Param>
{
    /// <inheritdoc />
    protected override string GetKeyForItem(Param item) => item.Name;
}
