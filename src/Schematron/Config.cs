using System.Xml;
using System.Xml.XPath;
using Schematron.Formatters;

namespace Schematron;

/// <summary>
/// Provides global settings for Schematron validation.
/// </summary>
/// <remarks>
/// This class is public to allow inheritors of Schematron elements
/// to use these global settings.
/// </remarks>
/// <author ref="kzu" />
/// <progress amount="100" />
public class Config
{
    /// <summary>
    /// Initializes global settings.
    /// </summary>
    static Config()
    {
        // Default formatter outputs in text format a log with results.
        _formatter = new LogFormatter();

        //TODO: create and load the schematron full and embedded versions for validation.
        _embedded = new Schema();
        _embedded.Phases.Add(_embedded.CreatePhase(Phase.All));
        _full = new Schema();
        _full.Phases.Add(_full.CreatePhase(Phase.All));

        //TODO: should we move all the schema language elements to a resource file?
        _navigator = new XmlDocument().CreateNavigator();
        _navigator.NameTable.Add("active");
        _navigator.NameTable.Add("pattern");
        _navigator.NameTable.Add("assert");
        _navigator.NameTable.Add("test");
        _navigator.NameTable.Add("role");
        _navigator.NameTable.Add("id");
        _navigator.NameTable.Add("diagnostics");
        _navigator.NameTable.Add("icon");
        _navigator.NameTable.Add("subject");
        _navigator.NameTable.Add("diagnostic");
        _navigator.NameTable.Add("dir");
        _navigator.NameTable.Add("emph");
        _navigator.NameTable.Add("extends");
        _navigator.NameTable.Add("rule");
        _navigator.NameTable.Add("key");
        _navigator.NameTable.Add("name");
        _navigator.NameTable.Add("path");
        _navigator.NameTable.Add("ns");
        _navigator.NameTable.Add("uri");
        _navigator.NameTable.Add("prefix");
        _navigator.NameTable.Add("p");
        _navigator.NameTable.Add("class");
        _navigator.NameTable.Add("see");
        _navigator.NameTable.Add("phase");
        _navigator.NameTable.Add("fpi");
        _navigator.NameTable.Add("report");
        _navigator.NameTable.Add("context");
        _navigator.NameTable.Add("abstract");
        _navigator.NameTable.Add("schema");
        _navigator.NameTable.Add("schemaVersion");
        _navigator.NameTable.Add("defaultPhase");
        _navigator.NameTable.Add("version");
        _navigator.NameTable.Add("span");
        _navigator.NameTable.Add("title");
        _navigator.NameTable.Add("value-of");
        _navigator.NameTable.Add("select");
        _navigator.NameTable.Add(Schema.IsoNamespace);
        _navigator.NameTable.Add(Schema.LegacyNamespace);

        //Namespace manager initialization
        _nsmanager = new XmlNamespaceManager(_navigator.NameTable);
        _nsmanager.AddNamespace(String.Empty, Schema.Namespace);
        _nsmanager.AddNamespace("sch", Schema.Namespace);
        _nsmanager.AddNamespace("xsd", System.Xml.Schema.XmlSchema.Namespace);
    }

    Config()
    {
    }

    static IFormatter _formatter;

    /// <summary>
    /// The default object to use to format messages from validation.
    /// </summary>
    public static IFormatter DefaultFormatter
    {
        get { return _formatter; }
    }

    static XPathNavigator _navigator;

    /// <summary>
    /// A default empty navigator used to pre-compile XPath expressions.
    /// </summary>
    /// <remarks>
    /// Compiling <see cref="XPathExpression"/> doesn't involve any namespace,
    /// name table or other specific processing. It's only a parsing procedure that
    /// builds the abstract syntax tree for later evaluation. So we can safely
    /// use an empty <see cref="XPathNavigator"/> to compile them against.
    /// </remarks>
    /// <example>
    /// <code>expr = Config.DefaultNavigator.Compile("//sch:pattern");
    /// other code;
    /// </code>
    /// <para>
    ///		<seealso cref="CompiledExpressions"/>
    /// </para>
    /// </example>
    internal static XPathNavigator DefaultNavigator
    {
        // Returning a cloned navigator appeared to solve the threading issues
        // we had, because a single navigator was being used to compile all the
        // expressions in all potential threads.
        get { return _navigator.Clone(); }
    }

    static XmlNamespaceManager _nsmanager;

    /// <summary>
    /// Manager to use when executing expressions that validate or
    /// load Schematron and Embedded Schematron schemas.
    /// </summary>
    public static XmlNamespaceManager DefaultNsManager
    {
        get { return _nsmanager; }
    }

    static Schema _full;

    /// <summary>
    /// A cached schema in Schematron format to validate schematron schemas.
    /// </summary>
    /// <remarks>This is the version for standalone schemas.</remarks>
    public static Schema FullSchematron
    {
        get { return _full; }
    }

    static Schema _embedded;

    /// <summary>
    /// A cached schema in Schematron format to validate schematron schemas.
    /// </summary>
    /// <remarks>This is the version for embedded schemas.</remarks>
    public static Schema EmbeddedSchematron
    {
        get { return _embedded; }
    }

    static string _uid = String.Intern(Guid.NewGuid().ToString());

    /// <summary>
    /// A unique identifier to use for internal keys.
    /// </summary>
    public static string UniqueKey
    {
        get { return _uid; }
    }

    /// <summary>
    /// Force all static constructors in the library.
    /// </summary>
    public static void Setup()
    {
        System.Diagnostics.Trace.Write("Loading schematron statics...");
        System.Diagnostics.Trace.Write(CompiledExpressions.Schema.ReturnType);
        System.Diagnostics.Trace.Write(TagExpressions.Dir.RightToLeft);
        System.Diagnostics.Trace.WriteLine(FormattingUtils.XmlErrorPosition.RightToLeft);
    }
}

