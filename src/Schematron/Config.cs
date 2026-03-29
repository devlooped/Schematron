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
public class Config
{
    static readonly IFormatter formatter;
    static readonly XPathNavigator navigator;
    static readonly XmlNamespaceManager nsmanager;
    static readonly Schema full;
    static readonly Schema embedded;
    static readonly string uid = string.Intern(Guid.NewGuid().ToString());

    /// <summary>
    /// Initializes global settings.
    /// </summary>
    static Config()
    {
        // Default formatter outputs in text format a log with results.
        formatter = new LogFormatter();

        //TODO: create and load the schematron full and embedded versions for validation.
        embedded = new Schema();
        embedded.Phases.Add(embedded.CreatePhase(Phase.All));
        full = new Schema();
        full.Phases.Add(full.CreatePhase(Phase.All));

        //TODO: should we move all the schema language elements to a resource file?
        navigator = new XmlDocument().CreateNavigator();
        navigator.NameTable.Add("active");
        navigator.NameTable.Add("pattern");
        navigator.NameTable.Add("assert");
        navigator.NameTable.Add("test");
        navigator.NameTable.Add("role");
        navigator.NameTable.Add("id");
        navigator.NameTable.Add("diagnostics");
        navigator.NameTable.Add("icon");
        navigator.NameTable.Add("subject");
        navigator.NameTable.Add("diagnostic");
        navigator.NameTable.Add("dir");
        navigator.NameTable.Add("emph");
        navigator.NameTable.Add("extends");
        navigator.NameTable.Add("rule");
        navigator.NameTable.Add("key");
        navigator.NameTable.Add("name");
        navigator.NameTable.Add("path");
        navigator.NameTable.Add("ns");
        navigator.NameTable.Add("uri");
        navigator.NameTable.Add("prefix");
        navigator.NameTable.Add("p");
        navigator.NameTable.Add("class");
        navigator.NameTable.Add("see");
        navigator.NameTable.Add("phase");
        navigator.NameTable.Add("fpi");
        navigator.NameTable.Add("report");
        navigator.NameTable.Add("context");
        navigator.NameTable.Add("abstract");
        navigator.NameTable.Add("schema");
        navigator.NameTable.Add("schemaVersion");
        navigator.NameTable.Add("defaultPhase");
        navigator.NameTable.Add("version");
        navigator.NameTable.Add("span");
        navigator.NameTable.Add("title");
        navigator.NameTable.Add("value-of");
        navigator.NameTable.Add("select");
        navigator.NameTable.Add(Schema.IsoNamespace);
        navigator.NameTable.Add(Schema.LegacyNamespace);

        //Namespace manager initialization
        nsmanager = new XmlNamespaceManager(navigator.NameTable);
        nsmanager.AddNamespace(string.Empty, Schema.Namespace);
        nsmanager.AddNamespace("sch", Schema.Namespace);
        nsmanager.AddNamespace("xsd", System.Xml.Schema.XmlSchema.Namespace);
    }

    Config() { }

    /// <summary>
    /// The default object to use to format messages from validation.
    /// </summary>
    public static IFormatter DefaultFormatter => formatter;

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
    /// <devdoc>
    /// Returning a cloned navigator appeared to solve the threading issues
    /// we had, because a single navigator was being used to compile all the
    /// expressions in all potential threads.
    /// </devdoc>
    internal static XPathNavigator DefaultNavigator => navigator.Clone();

    /// <summary>
    /// Manager to use when executing expressions that validate or
    /// load Schematron and Embedded Schematron schemas.
    /// </summary>
    public static XmlNamespaceManager DefaultNsManager => nsmanager;

    /// <summary>
    /// A cached schema in Schematron format to validate schematron schemas.
    /// </summary>
    /// <remarks>This is the version for standalone schemas.</remarks>
    public static Schema FullSchematron => full;

    /// <summary>
    /// A cached schema in Schematron format to validate schematron schemas.
    /// </summary>
    /// <remarks>This is the version for embedded schemas.</remarks>
    public static Schema EmbeddedSchematron => embedded;

    /// <summary>
    /// A unique identifier to use for internal keys.
    /// </summary>
    public static string UniqueKey => uid;

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