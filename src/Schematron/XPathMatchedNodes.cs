using System.Xml;
using System.Xml.XPath;

namespace Schematron;

/// <summary>
/// Strategy class for matching and keeping references to nodes in an <see cref="XPathDocument"/>.
/// </summary>
/// <authorref id="dcazzulino" />
/// <progress amount="100" />
/// <remarks>
/// When an <see cref="XPathNavigator"/> is created from an <see cref="XPathDocument"/>,
/// it implements the <see cref="IXmlLineInfo"/> interface, which is used to gain
/// access to the underlying node position.
/// </remarks>
class XPathMatchedNodes : IMatchedNodes
{
    // The dictionary maps each line number to a list of column positions.
    readonly Dictionary<int, List<int>> matched = [];

    /// <summary>See <see cref="IMatchedNodes.IsMatched"/>.</summary>
    public bool IsMatched(XPathNavigator node)
    {
        var info = (IXmlLineInfo)node;

        if (!matched.TryGetValue(info.LineNumber, out var pos))
            return false;

        return pos.Contains(info.LinePosition);
    }

    /// <summary>See <see cref="IMatchedNodes.AddMatched"/>.</summary>
    public void AddMatched(XPathNavigator node)
    {
        var info = (IXmlLineInfo)node;

        if (!matched.TryGetValue(info.LineNumber, out var pos))
        {
            pos = [];
            matched.Add(info.LineNumber, pos);
        }

        pos.Add(info.LinePosition);
    }

    /// <summary>See <see cref="IMatchedNodes.Clear"/>.</summary>
    public void Clear() => matched.Clear();
}

