using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Schematron.Tests;

/// <summary>
/// Compatibility tests: verifies that Schematron.NET produces the same pass/fail result
/// as the ISO XSLT 1.0 reference implementation for a set of (schema, xml) pairs.
///
/// Comparison is limited to whether validation passes or fails (HasErrors). Message text
/// is intentionally not compared because the two implementations format messages differently.
///
/// ISO Schematron 2025 features that have no XSLT 1.0 counterpart (group, @visit-each,
/// @when on phase) are NOT covered here — they are tested in ValidatorTests.cs.
/// </summary>
public class CompatibilityTests
{
    /// <summary>
    /// Test cases: (label, schemaPath, xmlFile, expectErrors).
    /// xmlFile is relative to the test output directory.
    /// </summary>
    public static IEnumerable<object[]> Cases()
    {
        // ── basic-assert ──────────────────────────────────────────────────────
        yield return Case("BasicAssert/valid", "Content/compat/basic-assert.sch", "Content/compat/basic-assert-valid.xml", false);
        yield return Case("BasicAssert/invalid", "Content/compat/basic-assert.sch", "Content/compat/basic-assert-invalid.xml", true);

        // ── basic-report ─────────────────────────────────────────────────────
        yield return Case("BasicReport/no-trigger", "Content/compat/basic-report.sch", "Content/compat/basic-report-valid.xml", false);
        yield return Case("BasicReport/trigger", "Content/compat/basic-report.sch", "Content/compat/basic-report-trigger.xml", true);

        // ── first-match (rule node-first-match within a pattern) ──────────────
        yield return Case("FirstMatch/valid", "Content/compat/first-match.sch", "Content/compat/first-match-valid.xml", false);
        yield return Case("FirstMatch/invalid", "Content/compat/first-match.sch", "Content/compat/first-match-invalid.xml", true);

        // ── multi-pattern ────────────────────────────────────────────────────
        yield return Case("MultiPattern/valid", "Content/compat/multi-pattern.sch", "Content/compat/multi-pattern-valid.xml", false);
        yield return Case("MultiPattern/invalid", "Content/compat/multi-pattern.sch", "Content/compat/multi-pattern-invalid.xml", true);

        // ── let variables ────────────────────────────────────────────────────
        yield return Case("LetScopes/valid", "Content/compat/let-scopes.sch", "Content/compat/let-scopes-valid.xml", false);
        yield return Case("LetScopes/invalid", "Content/compat/let-scopes.sch", "Content/compat/let-scopes-invalid.xml", true);

        // ── abstract rules via <extends> ─────────────────────────────────────
        yield return Case("AbstractRule/valid", "Content/compat/abstract-rule.sch", "Content/compat/abstract-rule-valid.xml", false);
        yield return Case("AbstractRule/invalid", "Content/compat/abstract-rule.sch", "Content/compat/abstract-rule-invalid.xml", true);

        // ── abstract patterns (is-a) — reuse existing fixture ────────────────
        yield return Case("AbstractPattern/valid", "Content/abstract-pattern-schema.sch", "Content/compat/abstract-pattern-valid.xml", false);
        yield return Case("AbstractPattern/invalid", "Content/abstract-pattern-schema.sch", "Content/compat/abstract-pattern-invalid.xml", true);

        // ── phases ───────────────────────────────────────────────────────────
        yield return Case("Phases/basic-valid", "Content/compat/phases.sch", "Content/compat/phases-basic-valid.xml", false);
        yield return Case("Phases/basic-invalid", "Content/compat/phases.sch", "Content/compat/phases-basic-invalid.xml", true);

        // ── namespace prefixes ───────────────────────────────────────────────
        yield return Case("Namespaces/valid", "Content/compat/namespaces.sch", "Content/compat/namespaces-valid.xml", false);
        yield return Case("Namespaces/invalid", "Content/compat/namespaces.sch", "Content/compat/namespaces-invalid.xml", true);

        // ── value-of in message ──────────────────────────────────────────────
        yield return Case("ValueOf/valid", "Content/compat/value-of.sch", "Content/compat/value-of-valid.xml", false);
        yield return Case("ValueOf/invalid", "Content/compat/value-of.sch", "Content/compat/value-of-invalid.xml", true);
    }

    static object[] Case(string label, string schemaPath, string xmlPath, bool expectErrors)
        => [label, schemaPath, xmlPath, expectErrors];

    [Theory]
    [MemberData(nameof(Cases))]
    public void BothImplementationsAgreeOnPassFail(string label, string schemaPath, string xmlPath, bool expectErrors)
    {
        string schemaFullPath = ResolvePath(schemaPath);
        string xmlContent = File.ReadAllText(ResolvePath(xmlPath));

        // ── Schematron.NET ───────────────────────────────────────────────────
        bool netErrors = false;
        var validator = new Validator();
        validator.AddSchema(System.Xml.XmlReader.Create(schemaFullPath));
        try
        {
            validator.Validate(new StringReader(xmlContent));
        }
        catch (ValidationException)
        {
            netErrors = true;
        }

        // ── Reference XSLT implementation ────────────────────────────────────
        SvrlResult svrl = ReferenceSchematronRunner.Validate(schemaFullPath, xmlContent);

        // ── Assertions ───────────────────────────────────────────────────────
        Xunit.Assert.Equal(expectErrors, netErrors);
        Xunit.Assert.Equal(expectErrors, svrl.HasErrors);
        Xunit.Assert.Equal(netErrors, svrl.HasErrors);
        _ = label; // used only for test display name
    }

    // -------------------------------------------------------------------------
    // Abstract-pattern fixtures are inlined here (no separate file needed for
    // the reused schema) but we do need the XML files.
    // -------------------------------------------------------------------------

    static string ResolvePath(string relative)
    {
        // When tests run, the working directory is the test output directory.
        if (File.Exists(relative)) return relative;
        // Fallback: relative to Content/ for schemas shared with ValidatorTests.
        string alt = Path.Combine(".", relative);
        return alt;
    }
}
