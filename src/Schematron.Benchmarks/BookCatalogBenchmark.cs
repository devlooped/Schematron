using System;
using System.IO;
using System.Text;
using System.Xml;
using BenchmarkDotNet.Attributes;

namespace Schematron.Benchmarks;

/// <summary>
/// Benchmarks Schematron.NET against the ISO XSLT 1.0 reference implementation
/// for book-catalog documents that exercise abstract rules via &lt;extends&gt;,
/// let variables, and complex XPath predicates across three patterns.
/// </summary>
[MemoryDiagnoser]
[Config(typeof(BenchmarkConfig))]
public class BookCatalogBenchmark
{
    const string SchemaPath = "Content/book-catalog.sch";

    // --- pre-compiled state --------------------------------------------------

    Validator _validator = null!;
    CompiledReferenceValidator _reference = null!;
    string _validXml   = null!;
    string _invalidXml = null!;

    // --- parameters ----------------------------------------------------------

    /// <summary>Number of books in the catalog (each with 5 chapters).</summary>
    [Params(10, 50)]
    public int BookCount { get; set; }

    // --- setup ---------------------------------------------------------------

    [GlobalSetup]
    public void Setup()
    {
        _validator = new Validator();
        _validator.AddSchema(XmlReader.Create(SchemaPath));

        _reference = ReferenceXsltRunner.Compile(SchemaPath);

        _validXml   = GenerateCatalogXml(BookCount, valid: true);
        _invalidXml = GenerateCatalogXml(BookCount, valid: false);
    }

    // --- benchmarks ----------------------------------------------------------

    /// <summary>Validate a well-formed book catalog – Schematron.NET.</summary>
    [Benchmark(Baseline = true, Description = "SchematronNET · valid")]
    public bool SchematronNet_ValidDoc() => ValidateSchematronNet(_validXml);

    /// <summary>Validate a well-formed book catalog – ISO XSLT reference.</summary>
    [Benchmark(Description = "Reference XSLT · valid")]
    public bool Reference_ValidDoc() => !_reference.HasErrors(_validXml);

    /// <summary>Validate a catalog with constraint violations – Schematron.NET.</summary>
    [Benchmark(Description = "SchematronNET · invalid")]
    public bool SchematronNet_InvalidDoc() => !ValidateSchematronNet(_invalidXml);

    /// <summary>Validate a catalog with constraint violations – ISO XSLT reference.</summary>
    [Benchmark(Description = "Reference XSLT · invalid")]
    public bool Reference_InvalidDoc() => _reference.HasErrors(_invalidXml);

    // --- helpers -------------------------------------------------------------

    bool ValidateSchematronNet(string xml)
    {
        var nav = new System.Xml.XPath.XPathDocument(
            new StringReader(xml)).CreateNavigator();
        try
        {
            _validator.ValidateSchematron(nav);
            return true;
        }
        catch (ValidationException)
        {
            return false;
        }
    }

    // -------------------------------------------------------------------------
    // XML generation
    // -------------------------------------------------------------------------

    static readonly string[] Genres = ["fiction", "non-fiction", "science", "history", "biography"];

    static string GenerateCatalogXml(int bookCount, bool valid)
    {
        var sb = new StringBuilder();
        using var w = XmlWriter.Create(sb, new XmlWriterSettings { Indent = false });

        w.WriteStartElement("catalog");

        for (int b = 1; b <= bookCount; b++)
        {
            w.WriteStartElement("book");
            w.WriteAttributeString("id",    $"BK-{b:D4}");
            w.WriteAttributeString("title", $"Title of Book {b}");
            // 13-char ISBN (valid) or 10-char (invalid for even books)
            w.WriteAttributeString("isbn",  valid || b % 3 != 0
                                                ? $"978{b:D10}"
                                                : $"0-{b:D8}");
            w.WriteAttributeString("year",  valid ? "2023" : (b % 5 == 0 ? "1800" : "2023"));
            w.WriteAttributeString("price", valid ? "29.99" : (b % 7 == 0 ? "-5" : "29.99"));
            w.WriteAttributeString("genre", Genres[b % Genres.Length]);

            // At least one author (valid) or zero authors for invalid every 4th book
            int authorCount = valid || b % 4 != 0 ? 2 : 0;
            for (int a = 1; a <= authorCount; a++)
            {
                w.WriteStartElement("author");
                w.WriteAttributeString("name", $"Author {a} of Book {b}");
                w.WriteEndElement();
            }

            // Chapters
            w.WriteStartElement("chapters");
            for (int ch = 1; ch <= 5; ch++)
            {
                w.WriteStartElement("chapter");
                w.WriteAttributeString("id",    $"CH-{b:D4}-{ch}");
                w.WriteAttributeString("title", $"Chapter {ch}");
                w.WriteAttributeString("pages", valid ? (ch * 10).ToString()
                                                      : (ch == 1 ? "0" : (ch * 10).ToString()));
                w.WriteEndElement(); // chapter
            }
            w.WriteEndElement(); // chapters

            w.WriteEndElement(); // book
        }

        w.WriteEndElement(); // catalog
        w.Flush();
        return sb.ToString();
    }
}
