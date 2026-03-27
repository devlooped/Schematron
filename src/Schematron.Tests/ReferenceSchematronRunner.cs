using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Schematron.Tests;

/// <summary>
/// Runs the ISO XSLT 1.0 reference Schematron pipeline and returns an SVRL-based result.
/// Pipeline: schema → iso_dsdl_include → iso_abstract_expand → iso_svrl_for_xslt1 → validator.xslt → SVRL.
/// </summary>
static class ReferenceSchematronRunner
{
    const string XsltDir = "./Content/xslt";
    const string SvrlNs = "http://purl.oclc.org/dsdl/svrl";

    static readonly XslCompiledTransform _include = LoadXslt(Path.Combine(XsltDir, "iso_dsdl_include.xsl"));
    static readonly XslCompiledTransform _abstract = LoadXslt(Path.Combine(XsltDir, "iso_abstract_expand.xsl"));
    static readonly XslCompiledTransform _svrl = LoadXslt(Path.Combine(XsltDir, "iso_svrl_for_xslt1.xsl"));

    static XslCompiledTransform LoadXslt(string path)
    {
        var xslt = new XslCompiledTransform();
        var settings = new XsltSettings(enableDocumentFunction: true, enableScript: false);
        var resolver = new XmlUrlResolver();
        xslt.Load(path, settings, resolver);
        return xslt;
    }

    /// <summary>
    /// Validates <paramref name="xmlContent"/> against the Schematron schema at <paramref name="schemaPath"/>
    /// using the ISO XSLT 1.0 reference pipeline.
    /// </summary>
    public static SvrlResult Validate(string schemaPath, string xmlContent, string? phase = null)
    {
        // Step 1: resolve includes
        string included = ApplyTransform(_include, ReadFile(schemaPath), baseUri: Path.GetFullPath(schemaPath));
        // Step 2: expand abstract patterns
        string expanded = ApplyTransform(_abstract, included);
        // Step 3: generate validator XSLT from schema
        var svrlArgs = new XsltArgumentList();
        if (!string.IsNullOrEmpty(phase))
            svrlArgs.AddParam("phase", "", phase);
        string validatorXslt = ApplyTransform(_svrl, expanded, args: svrlArgs);
        // Step 4: apply validator XSLT to the XML instance
        var validator = new XslCompiledTransform();
        using (var validatorReader = XmlReader.Create(new StringReader(validatorXslt)))
            validator.Load(validatorReader, new XsltSettings(enableDocumentFunction: true, enableScript: false), new XmlUrlResolver());

        string svrl = ApplyTransform(validator, xmlContent);
        return SvrlResult.Parse(svrl);
    }

    static string ReadFile(string path)
    {
        using var sr = new StreamReader(path, Encoding.UTF8);
        return sr.ReadToEnd();
    }

    static string ApplyTransform(XslCompiledTransform xslt, string xmlInput, string? baseUri = null, XsltArgumentList? args = null)
    {
        var readerSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };
        using var inputReader = XmlReader.Create(new StringReader(xmlInput), readerSettings, baseUri);

        var sb = new StringBuilder();
        var writerSettings = xslt.OutputSettings?.Clone() ?? new XmlWriterSettings();
        writerSettings.OmitXmlDeclaration = false;
        writerSettings.Indent = false;
        using var writer = XmlWriter.Create(sb, writerSettings);
        xslt.Transform(inputReader, args, writer);
        return sb.ToString();
    }
}

/// <summary>Result of running the ISO XSLT reference pipeline on an XML instance.</summary>
sealed class SvrlResult
{
    const string SvrlNs = "http://purl.oclc.org/dsdl/svrl";

    public bool HasErrors { get; private init; }

    /// <summary>@test values of all svrl:failed-assert elements.</summary>
    public IReadOnlyList<string> FailedAsserts { get; private init; } = [];

    /// <summary>@test values of all svrl:successful-report elements.</summary>
    public IReadOnlyList<string> SuccessfulReports { get; private init; } = [];

    public static SvrlResult Parse(string svrlXml)
    {
        var doc = new XmlDocument();
        doc.LoadXml(svrlXml);

        var ns = new XmlNamespaceManager(doc.NameTable);
        ns.AddNamespace("svrl", SvrlNs);

        var failedAsserts = Collect(doc, ns, "//svrl:failed-assert/@test");
        var successReports = Collect(doc, ns, "//svrl:successful-report/@test");

        return new SvrlResult
        {
            HasErrors = failedAsserts.Count > 0 || successReports.Count > 0,
            FailedAsserts = failedAsserts,
            SuccessfulReports = successReports,
        };
    }

    static List<string> Collect(XmlDocument doc, XmlNamespaceManager ns, string xpath)
    {
        var result = new List<string>();
        var nodes = doc.SelectNodes(xpath, ns);
        if (nodes is not null)
            foreach (XmlNode n in nodes)
                result.Add(n.Value ?? n.InnerText);
        return result;
    }
}
