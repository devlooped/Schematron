using System.Xml;
using System.Xml.XPath;

namespace Schematron;

/// <summary>
/// Defines the common interface used by the different node-matching strategies.
/// </summary>
/// <remarks>
/// As different <see cref="XPathNavigator"/> can exist, and even be developed
/// in the future, we have to take into account that the data store can change.
/// So in order to be efficient at keeping nodes matched so far, to satisfy the
/// <link ref="schematron" />, we provide a common interface and an
/// implementation optimized for specific stores.
/// <para>
/// Each navigator implementation typically provides an interface to let
/// applications get access to the underlying store, such as the <see cref="IHasXmlNode"/>
/// or <see cref="IXmlLineInfo"/> interfaces, implemented in navigators create by
/// <see cref="XmlDocument"/> or <see cref="XPathDocument"/> classes.
/// </para>
/// </remarks>
/// <author ref="kzu" />
/// <progress amount="100" />
public interface IMatchedNodes
{
    /// <summary>Checks if an specific node has already been matched.</summary>
    /// <param name="node">The node to check.</param>
    bool IsMatched(XPathNavigator node);

    /// <summary>Adds a node to the list of nodes matched so far.</summary>
    /// <param name="node">The node to add.</param>
    void AddMatched(XPathNavigator node);

    /// <summary>Clears the list of matched nodes.</summary>
    void Clear();
}

