using System;
using System.IO;
using System.Text;
using System.Xml;
using BenchmarkDotNet.Attributes;

namespace Schematron.Benchmarks;

/// <summary>
/// Benchmarks Schematron.NET against the ISO XSLT 1.0 reference implementation
/// for namespace-qualified purchase-order documents with three patterns,
/// let variables, and aggregate XPath expressions.
/// </summary>
[MemoryDiagnoser]
[Config(typeof(BenchmarkConfig))]
public class PurchaseOrderBenchmark
{
    const string SchemaPath = "Content/purchase-order.sch";

    // --- pre-compiled state (GlobalSetup) ------------------------------------

    Validator _validator = null!;
    CompiledReferenceValidator _reference = null!;
    string _validXml   = null!;
    string _invalidXml = null!;

    // --- parameters ----------------------------------------------------------

    /// <summary>Number of customers in the document (each with 3 orders, 5 items each).</summary>
    [Params(5, 25)]
    public int CustomerCount { get; set; }

    // --- setup ---------------------------------------------------------------

    [GlobalSetup]
    public void Setup()
    {
        _validator = new Validator();
        _validator.AddSchema(XmlReader.Create(SchemaPath));

        _reference = ReferenceXsltRunner.Compile(SchemaPath);

        _validXml   = GeneratePurchaseOrderXml(CustomerCount, valid: true);
        _invalidXml = GeneratePurchaseOrderXml(CustomerCount, valid: false);
    }

    // --- benchmarks ----------------------------------------------------------

    /// <summary>Validate a well-formed PO document (no violations) – Schematron.NET.</summary>
    [Benchmark(Baseline = true, Description = "SchematronNET · valid")]
    public bool SchematronNet_ValidDoc() => ValidateSchematronNet(_validXml);

    /// <summary>Validate a well-formed PO document (no violations) – ISO XSLT reference.</summary>
    [Benchmark(Description = "Reference XSLT · valid")]
    public bool Reference_ValidDoc() => !_reference.HasErrors(_validXml);

    /// <summary>Validate a PO with constraint violations – Schematron.NET.</summary>
    [Benchmark(Description = "SchematronNET · invalid")]
    public bool SchematronNet_InvalidDoc() => !ValidateSchematronNet(_invalidXml);

    /// <summary>Validate a PO with constraint violations – ISO XSLT reference.</summary>
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

    const string PurchaseOrderNs = "urn:example:po";

    static string GeneratePurchaseOrderXml(int customerCount, bool valid)
    {
        var sb = new StringBuilder();
        using var w = XmlWriter.Create(sb, new XmlWriterSettings { Indent = false });

        w.WriteStartElement("po", "orders", PurchaseOrderNs);

        for (int c = 1; c <= customerCount; c++)
        {
            w.WriteStartElement("po", "customer", PurchaseOrderNs);
            w.WriteAttributeString("id",    $"CUST-{c:D4}");
            w.WriteAttributeString("name",  $"Customer {c}");
            w.WriteAttributeString("sex",   c % 2 == 0 ? "Male" : "Female");
            w.WriteAttributeString("title", c % 2 == 0 ? "Mr"   : "Mrs");

            // 3 orders per customer: new, paid, cancelled
            string[] statuses = ["new", "paid", "cancelled"];
            for (int o = 0; o < 3; o++)
            {
                string status = statuses[o];
                w.WriteStartElement("po", "order", PurchaseOrderNs);
                w.WriteAttributeString("status", status);

                // 5 line items
                for (int i = 1; i <= 5; i++)
                {
                    w.WriteStartElement("po", "item", PurchaseOrderNs);
                    w.WriteAttributeString("sku",   $"{c:D3}-{o:D2}-{i:D2}");
                    w.WriteAttributeString("price", valid ? (i * 9.99m).ToString("F2") : "-1");
                    w.WriteAttributeString("qty",   "2");
                    w.WriteEndElement(); // item
                }

                if (status == "paid")
                {
                    w.WriteStartElement("po", "payment", PurchaseOrderNs);
                    w.WriteAttributeString("method", "card");
                    if (valid)
                    {
                        // cardRef is required for card payments in the schema
                        w.WriteStartElement("po", "cardRef", PurchaseOrderNs);
                        w.WriteString("XXXX-1234");
                        w.WriteEndElement();
                    }
                    // invalid: omit cardRef so the report fires
                    w.WriteEndElement(); // payment
                }

                w.WriteEndElement(); // order
            }

            w.WriteEndElement(); // customer
        }

        w.WriteEndElement(); // orders
        w.Flush();
        return sb.ToString();
    }
}
