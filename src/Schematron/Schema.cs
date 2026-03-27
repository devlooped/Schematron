using System.Xml;
using System.Xml.XPath;

namespace Schematron;

/// <summary>
/// </summary>
/// <author ref="kzu" />
/// <progress amount="100">Lacks attributes defined in Schematron, but not in use currently.</progress>
public class Schema
{
    /// <summary>
    /// The Schematron namespace.
    /// </summary>
    public const string Namespace = "http://www.ascc.net/xml/schematron";

    SchemaLoader _loader;
    string _title = String.Empty;

    string _defaultphase = String.Empty;
    PhaseCollection _phases = new PhaseCollection();
    PatternCollection _patterns = new PatternCollection();
    XmlNamespaceManager _ns = null!;

    /// <summary />
    public Schema()
    {
        _loader = CreateLoader();
    }

    /// <summary />
    public Schema(string title) : this()
    {
        _title = title;
    }

    #region Overridable Factory Methods
    /// <summary />
    internal protected virtual SchemaLoader CreateLoader()
    {
        return new SchemaLoader(this);
    }

    /// <summary />
    public virtual Phase CreatePhase(string id)
    {
        return new Phase(id);
    }

    /// <summary />
    public virtual Phase CreatePhase()
    {
        return new Phase();
    }
    #endregion

    #region Overloaded Load methods
    /// <summary>
    /// Loads the schema from the specified URI.
    /// </summary>
    public void Load(string uri)
    {
        using var fs = new FileStream(uri, FileMode.Open, FileAccess.Read, FileShare.Read);
        Load(new XmlTextReader(fs));
    }

    /// <summary>
    /// Loads the schema from the reader. Closing the reader is responsibility of the caller.
    /// </summary>
    public void Load(TextReader reader)
    {
        Load(new XmlTextReader(reader));
    }

    /// <summary>
    /// Loads the schema from the stream. Closing the stream is responsibility of the caller.
    /// </summary>
    public void Load(Stream input)
    {
        Load(new XmlTextReader(input));
    }

    /// <summary>
    /// Loads the schema from the reader. Closing the reader is responsibility of the caller.
    /// </summary>
    public void Load(XmlReader schema)
    {
        var doc = new XmlDocument(schema.NameTable);
        doc.Load(schema);
        Load(doc.CreateNavigator());
    }

    /// <summary />
    public void Load(XPathNavigator schema)
    {
        Loader.LoadSchema(schema);
    }
    #endregion

    #region Properties
    /// <summary />
    internal protected SchemaLoader Loader
    {
        get { return _loader; }
        set { _loader = value; }
    }

    /// <summary />
    public string DefaultPhase
    {
        get { return _defaultphase; }
        set { _defaultphase = value; }
    }

    /// <summary />
    public string Title
    {
        get { return _title; }
        set { _title = value; }
    }

    /// <summary />
    public PhaseCollection Phases
    {
        get { return _phases; }
        set { _phases = value; }
    }

    /// <summary />
    public PatternCollection Patterns
    {
        get { return _patterns; }
        set { _patterns = value; }
    }

    /// <summary />
    public XmlNamespaceManager NsManager
    {
        get { return _ns; }
        set { _ns = value; }
    }
    #endregion
}

