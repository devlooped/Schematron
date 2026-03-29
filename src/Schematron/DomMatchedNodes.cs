using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace Schematron;

/// <summary>
/// Strategy class for matching and keeping references to nodes in an xml document.
/// </summary>
/// <remarks>
/// When an <see cref="XPathNavigator"/> is created from an <see cref="XmlDocument"/>,
/// it implements the <see cref="IHasXmlNode"/> interface, which is used to gain
/// access to the underlying node.
/// </remarks>
/// <author ref="kzu" />
/// <progress amount="100" />
class DomMatchedNodes : IMatchedNodes
{
    /// <summary>
    /// We use a collection for saving the hash codes.
    /// </summary>
    readonly List<int> matched = [];

    /// <summary>See <see cref="IMatchedNodes.IsMatched"/>.</summary>
    public bool IsMatched(XPathNavigator node) => matched.Contains(((IHasXmlNode)node).GetNode().GetHashCode());

    /// <summary>See <see cref="IMatchedNodes.AddMatched"/>.</summary>
    public void AddMatched(XPathNavigator node) => matched.Add(((IHasXmlNode)node).GetNode().GetHashCode());

    /// <summary>See <see cref="IMatchedNodes.Clear"/>.</summary>
    public void Clear() => matched.Clear();
}

