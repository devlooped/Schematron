using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace Schematron.Benchmarks;

/// <summary>
/// Thin wrapper around the ISO XSLT 1.0 reference Schematron pipeline.
/// Pipeline: .sch → iso_dsdl_include → iso_abstract_expand → iso_svrl_for_xslt1 → compiled validator XSLT.
/// </summary>
/// <remarks>
/// The three pipeline stylesheets are loaded once (static) because they never change.
/// Call <see cref="Compile"/> per schema to obtain a <see cref="CompiledReferenceValidator"/>
/// whose <see cref="CompiledReferenceValidator.Validate"/> method measures only per-document cost.
/// </remarks>
static class ReferenceXsltRunner
{
    const string XsltDir = "./Content/xslt";

    static readonly XslCompiledTransform Include =
        LoadXslt(Path.Combine(XsltDir, "iso_dsdl_include.xsl"));
    static readonly XslCompiledTransform AbstractExpand =
        LoadXslt(Path.Combine(XsltDir, "iso_abstract_expand.xsl"));
    static readonly XslCompiledTransform Svrl =
        LoadXslt(Path.Combine(XsltDir, "iso_svrl_for_xslt1.xsl"));

    static XslCompiledTransform LoadXslt(string path)
    {
        var xslt = new XslCompiledTransform();
        xslt.Load(path, new XsltSettings(enableDocumentFunction: true, enableScript: false),
            new XmlUrlResolver());
        return xslt;
    }

    /// <summary>
    /// Runs the full three-stage pipeline to produce a compiled validator XSLT for
    /// <paramref name="schemaPath"/>.  Equivalent in cost to <see cref="Validator.AddSchema"/>.
    /// </summary>
    public static CompiledReferenceValidator Compile(string schemaPath)
    {
        string raw       = File.ReadAllText(schemaPath, Encoding.UTF8);
        string included  = ApplyTransform(Include, raw,
                               baseUri: Path.GetFullPath(schemaPath));
        string expanded  = ApplyTransform(AbstractExpand, included);
        string xsltText  = ApplyTransform(Svrl, expanded);

        var validator = new XslCompiledTransform();
        using var reader = XmlReader.Create(new StringReader(xsltText));
        validator.Load(reader,
            new XsltSettings(enableDocumentFunction: true, enableScript: false),
            new XmlUrlResolver());

        return new CompiledReferenceValidator(validator);
    }

    internal static string ApplyTransform(
        XslCompiledTransform xslt,
        string xmlInput,
        string? baseUri = null,
        XsltArgumentList? args = null)
    {
        var readerSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };
        using var inputReader = XmlReader.Create(new StringReader(xmlInput),
            readerSettings, baseUri);

        var sb = new StringBuilder();
        var writerSettings = xslt.OutputSettings?.Clone() ?? new XmlWriterSettings();
        writerSettings.OmitXmlDeclaration = false;
        writerSettings.Indent = false;
        using var writer = XmlWriter.Create(sb, writerSettings);
        xslt.Transform(inputReader, args, writer);
        return sb.ToString();
    }
}

/// <summary>
/// A compiled reference validator for a single Schematron schema.
/// Produced by <see cref="ReferenceXsltRunner.Compile"/>.
/// </summary>
sealed class CompiledReferenceValidator(XslCompiledTransform validatorXslt)
{
    const string SvrlNs = "http://purl.oclc.org/dsdl/svrl";

    /// <summary>
    /// Validates <paramref name="xmlContent"/> and returns
    /// <c>true</c> if any failed-assert or successful-report is present.
    /// </summary>
    public bool HasErrors(string xmlContent)
    {
        string svrlXml = ReferenceXsltRunner.ApplyTransform(validatorXslt, xmlContent);

        var doc = new XmlDocument();
        doc.LoadXml(svrlXml);
        var ns = new XmlNamespaceManager(doc.NameTable);
        ns.AddNamespace("svrl", SvrlNs);

        return doc.SelectSingleNode("//svrl:failed-assert | //svrl:successful-report", ns) is not null;
    }
}
