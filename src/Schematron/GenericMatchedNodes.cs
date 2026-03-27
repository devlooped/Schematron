using System.Collections;
using System.Xml.XPath;

namespace Schematron;

/// <summary>
/// Strategy class for matching and keeping references to nodes in
/// an unknown implementation of <see cref="XPathNavigator"/>.
/// </summary>
/// <remarks>
/// This implementation uses the standard <see cref="XPathNavigator.IsSamePosition"/>
/// to know if a navigator has already been matched. This is not optimum because
/// a complete traversal of nodes matched so far has to be performed, but it will
/// work with all implementations of <see cref="XPathNavigator"/>.
/// </remarks>
/// <author ref="kzu" />
/// <progress amount="100" />
class GenericMatchedNodes : IMatchedNodes
{
    /// <summary>
    /// Uses a simple arraylist to keep the navigators.
    /// </summary>
    ArrayList _matched = new ArrayList();

    /// <summary>Initializes an instance of the class.</summary>
    public GenericMatchedNodes()
    {
    }

    /// <summary>See <see cref="IMatchedNodes.IsMatched"/>.</summary>
    public bool IsMatched(XPathNavigator node)
    {
        foreach (XPathNavigator nav in _matched)
        {
            if (node.IsSamePosition(nav)) return true;
        }

        return false;
    }

    /// <summary>See <see cref="IMatchedNodes.AddMatched"/>.</summary>
    public void AddMatched(XPathNavigator node)
    {
        _matched.Add(node.Clone());
    }

    /// <summary>See <see cref="IMatchedNodes.Clear"/>.</summary>
    public void Clear()
    {
        _matched.Clear();
    }
}

