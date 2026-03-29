using System.Xml;
using System.Xml.XPath;

namespace Schematron;

/// <summary>
/// Represents an ISO/IEC 19757-3 Schematron schema used for validating XML documents.
/// 
/// This class manages schema components including phases, patterns, variable bindings (lets),
/// diagnostic messages, and parameters. It supports loading schemas from multiple sources
/// (URI, streams, readers, and XPath navigators) and provides factory methods for creating
/// phases. The class maintains namespace management for XML processing and supports both
/// the current ISO standard namespace and legacy ASCC namespace for backward compatibility.
/// </summary>
public class Schema
{
    /// <summary>The ISO/IEC 19757-3 Schematron namespace (official, current standard).</summary>
    public const string IsoNamespace = "http://purl.oclc.org/dsdl/schematron";

    /// <summary>The legacy ASCC Schematron namespace. Supported for backward compatibility.</summary>
    public const string LegacyNamespace = "http://www.ascc.net/xml/schematron";

    /// <summary>The default Schematron namespace. Kept for backward compatibility; prefer <see cref="IsoNamespace"/>.</summary>
    public const string Namespace = LegacyNamespace;

    /// <summary>A shared empty schema instance used as a default sentinel.</summary>
    public static Schema Empty { get; } = new();

    /// <summary>Returns <see langword="true"/> if <paramref name="uri"/> is a recognized Schematron namespace URI.</summary>
    public static bool IsSchematronNamespace(string? uri) => uri == IsoNamespace || uri == LegacyNamespace;

    readonly LetCollection lets = [];
    readonly DiagnosticCollection diagnostics = [];
    readonly ParamCollection paramItems = [];

    /// <summary />
    public Schema() => Loader = CreateLoader();

    /// <summary />
    public Schema(string title) : this() => Title = title;

    /// <summary />
    internal protected virtual SchemaLoader CreateLoader() => new(this);

    /// <summary />
    public virtual Phase CreatePhase(string id) => new(id);

    /// <summary />
    public virtual Phase CreatePhase() => new();

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
    public void Load(TextReader reader) => Load(new XmlTextReader(reader));

    /// <summary>
    /// Loads the schema from the stream. Closing the stream is responsibility of the caller.
    /// </summary>
    public void Load(Stream input) => Load(new XmlTextReader(input));

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
    public void Load(XPathNavigator schema) => Loader.LoadSchema(schema);

    /// <summary />
    internal protected SchemaLoader Loader { get; set; } = null!;

    /// <summary />
    public string DefaultPhase { get; set; } = string.Empty;

    /// <summary />
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the Schematron edition declared by the schema's <c>@schematronEdition</c> attribute.</summary>
    /// <remarks>A value of <c>"2025"</c> indicates ISO Schematron 4th edition.</remarks>
    public string SchematronEdition { get; set; } = string.Empty;

    /// <summary />
    public PhaseCollection Phases { get; set; } = [];

    /// <summary />
    public PatternCollection Patterns { get; set; } = [];

    /// <summary>Gets the variable bindings declared at the schema level (<c>&lt;let&gt;</c> elements).</summary>
    public LetCollection Lets => lets;

    /// <summary>Gets the diagnostic elements declared in the schema (<c>&lt;diagnostics&gt;/&lt;diagnostic&gt;</c>).</summary>
    public DiagnosticCollection Diagnostics => diagnostics;

    /// <summary>Gets the parameter declarations at the schema level (<c>&lt;param&gt;</c> elements).</summary>
    public ParamCollection Params => paramItems;

    /// <summary>Gets or sets a value indicating whether this schema was loaded from a <c>&lt;library&gt;</c> root element (ISO Schematron 2025).</summary>
    public bool IsLibrary { get; set; }

    /// <summary />
    public XmlNamespaceManager NsManager { get; set; } = new(new NameTable());
}

