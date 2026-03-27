using System.Xml;
using System.Xml.XPath;

namespace Schematron;

/// <summary>
/// </summary>
/// <author ref="kzu" />
/// <progress amount="100">Lacks attributes defined in Schematron, but not in use currently.</progress>
public class Schema
{
    /// <summary>The ISO/IEC 19757-3 Schematron namespace (official, current standard).</summary>
    public const string IsoNamespace = "http://purl.oclc.org/dsdl/schematron";

    /// <summary>The legacy ASCC Schematron namespace. Supported for backward compatibility.</summary>
    public const string LegacyNamespace = "http://www.ascc.net/xml/schematron";

    /// <summary>The default Schematron namespace. Kept for backward compatibility; prefer <see cref="IsoNamespace"/>.</summary>
    public const string Namespace = LegacyNamespace;

    /// <summary>Returns <see langword="true"/> if <paramref name="uri"/> is a recognized Schematron namespace URI.</summary>
    public static bool IsSchematronNamespace(string? uri) =>
        uri == IsoNamespace || uri == LegacyNamespace;

    SchemaLoader _loader;
    string _title = String.Empty;
    string _schematronEdition = String.Empty;
    string _defaultphase = String.Empty;
    bool _isLibrary = false;
    PhaseCollection _phases = new PhaseCollection();
    PatternCollection _patterns = new PatternCollection();
    LetCollection _lets = new LetCollection();
    DiagnosticCollection _diagnostics = new DiagnosticCollection();
    ParamCollection _params = new ParamCollection();
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

    /// <summary>Gets or sets the Schematron edition declared by the schema's <c>@schematronEdition</c> attribute.</summary>
    /// <remarks>A value of <c>"2025"</c> indicates ISO Schematron 4th edition.</remarks>
    public string SchematronEdition
    {
        get { return _schematronEdition; }
        set { _schematronEdition = value; }
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

    /// <summary>Gets the variable bindings declared at the schema level (<c>&lt;let&gt;</c> elements).</summary>
    public LetCollection Lets => _lets;

    /// <summary>Gets the diagnostic elements declared in the schema (<c>&lt;diagnostics&gt;/&lt;diagnostic&gt;</c>).</summary>
    public DiagnosticCollection Diagnostics => _diagnostics;

    /// <summary>Gets the parameter declarations at the schema level (<c>&lt;param&gt;</c> elements).</summary>
    public ParamCollection Params => _params;

    /// <summary>Gets or sets a value indicating whether this schema was loaded from a <c>&lt;library&gt;</c> root element (ISO Schematron 2025).</summary>
    public bool IsLibrary
    {
        get { return _isLibrary; }
        set { _isLibrary = value; }
    }

    /// <summary />
    public XmlNamespaceManager NsManager
    {
        get { return _ns; }
        set { _ns = value; }
    }
    #endregion
}

