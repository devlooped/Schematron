using System.Collections;
using System.Xml;
using System.Xml.XPath;

namespace Schematron;

/// <summary>
/// Loads and parses Schematron schema documents from an XPathNavigator source.
/// Detects the Schematron namespace (ISO or legacy), compiles XPath expressions,
/// and populates a Schema object with phases, patterns, rules, assertions, reports,
/// and diagnostics. Supports abstract pattern instantiation with parameter substitution,
/// rule extension through abstract rules, and schema composition via extends references.
/// </summary>
public class SchemaLoader(Schema schema)
{
    XPathNavigator filenav = null!;
    Hashtable? abstracts;

    // Detected Schematron namespace and the namespace manager derived from the source document.
    string schNs = string.Empty;
    XmlNamespaceManager mgr = null!;

    // Instance-level XPath expressions compiled against the detected namespace.
    XPathExpression exprSchema = null!;
    XPathExpression exprEmbeddedSchema = null!;
    XPathExpression exprPhase = null!;
    XPathExpression exprPattern = null!;
    XPathExpression exprAbstractRule = null!;
    XPathExpression exprConcreteRule = null!;
    XPathExpression exprRuleExtends = null!;
    XPathExpression exprAssert = null!;
    XPathExpression exprReport = null!;
    XPathExpression exprLet = null!;
    XPathExpression exprDiagnostic = null!;
    XPathExpression exprParam = null!;
    XPathExpression exprLibrary = null!;
    XPathExpression exprRulesContainer = null!;
    XPathExpression exprGroup = null!;

    /// <summary />
    /// <param name="source"></param>
    public virtual void LoadSchema(XPathNavigator source)
    {
        schema.NsManager = new XmlNamespaceManager(source.NameTable);

        DetectAndBuildExpressions(source);

        var it = source.Select(exprSchema);
        if (it.Count > 1)
            throw new BadSchemaException("There can be at most one schema element per Schematron schema.");

        // Always work with the whole document to look for elements.
        // Embedded schematron will work as well as stand-alone schemas.
        filenav = source;

        if (it.Count == 1)
        {
            it.MoveNext();
            LoadSchemaElement(it.CurrentOrThrow());
        }
        else
        {
            // Check for <library> root element (ISO Schematron 2025)
            var libIt = source.Select(exprLibrary);
            if (libIt.Count == 1)
            {
                libIt.MoveNext();
                schema.IsLibrary = true;
                LoadSchemaElement(libIt.CurrentOrThrow());
            }
            else
            {
                // Load child elements from the appinfo element if it exists.
                LoadSchemaElements(source.Select(exprEmbeddedSchema));
            }
        }

        RetrieveAbstractRules();
        LoadPhases();
        LoadPatterns();
    }

    /// <summary>
    /// Detects the Schematron namespace used in <paramref name="source"/> and compiles all
    /// instance-level XPath expressions against that namespace.
    /// </summary>
    void DetectAndBuildExpressions(XPathNavigator source)
    {
        schNs = DetectSchematronNamespace(source);

        mgr = new XmlNamespaceManager(source.NameTable);
        mgr.AddNamespace("sch", schNs);
        mgr.AddNamespace("xsd", System.Xml.Schema.XmlSchema.Namespace);

        exprSchema = Compile("//sch:schema");
        exprEmbeddedSchema = Compile("xsd:schema/xsd:annotation/xsd:appinfo/*");
        exprPhase = Compile("descendant-or-self::sch:phase");
        exprPattern = Compile("//sch:pattern");
        exprAbstractRule = Compile("//sch:rule[@abstract=\"true\"]");
        exprConcreteRule = Compile("descendant-or-self::sch:rule[not(@abstract) or @abstract=\"false\"]");
        exprRuleExtends = Compile("descendant-or-self::sch:extends");
        exprAssert = Compile("descendant-or-self::sch:assert");
        exprReport = Compile("descendant-or-self::sch:report");
        exprLet = Compile("sch:let");
        exprDiagnostic = Compile("sch:diagnostics/sch:diagnostic");
        exprParam = Compile("sch:param");
        exprLibrary = Compile("//sch:library");
        exprRulesContainer = Compile("//sch:rules/sch:rule");
        exprGroup = Compile("//sch:group");
    }

    /// <summary>
    /// Inspects <paramref name="source"/> and returns the Schematron namespace URI in use.
    /// Checks the root element first, then descends into child elements (for embedded schemas).
    /// Defaults to <see cref="Schema.IsoNamespace"/> when no known namespace is found.
    /// </summary>
    static string DetectSchematronNamespace(XPathNavigator source)
    {
        var nav = source.Clone();
        nav.MoveToRoot();

        if (nav.MoveToFirstChild())
        {
            if (nav.NamespaceURI == Schema.IsoNamespace) return Schema.IsoNamespace;
            if (nav.NamespaceURI == Schema.LegacyNamespace) return Schema.LegacyNamespace;

            // Not directly a Schematron document (e.g. embedded inside XSD); scan descendants.
            var it = nav.SelectDescendants(XPathNodeType.Element, false);
            while (it.MoveNext())
            {
                var current = it.CurrentOrThrow();
                if (current.NamespaceURI == Schema.IsoNamespace) return Schema.IsoNamespace;
                if (current.NamespaceURI == Schema.LegacyNamespace) return Schema.LegacyNamespace;
            }
        }

        return Schema.IsoNamespace;
    }

    XPathExpression Compile(string xpath)
    {
        var expr = Config.DefaultNavigator.Compile(xpath);
        expr.SetContext(mgr);
        return expr;
    }

    void LoadSchemaElement(XPathNavigator context)
    {
        var phase = context.GetAttribute("defaultPhase", string.Empty);
        if (phase != string.Empty)
            schema.DefaultPhase = phase;

        var edition = context.GetAttribute("schematronEdition", string.Empty);
        if (edition != string.Empty)
            schema.SchematronEdition = edition;

        LoadSchemaElements(context.SelectChildren(XPathNodeType.Element));
        LoadLets(schema.Lets, context);
        LoadDiagnostics(context);
        LoadSchemaParams(context);
        LoadExtendsHref(context);
    }

    void LoadSchemaElements(XPathNodeIterator children)
    {
        while (children.MoveNext())
        {
            var current = children.CurrentOrThrow();
            if (current.NamespaceURI == schNs)
            {
                if (current.LocalName == "title")
                {
                    schema.Title = current.Value;
                }
                else if (current.LocalName == "ns")
                {
                    schema.NsManager.AddNamespace(
                        current.GetAttribute("prefix", string.Empty),
                        current.GetAttribute("uri", string.Empty));
                }
            }
        }
    }

    void RetrieveAbstractRules()
    {
        filenav.MoveToRoot();
        var it = filenav.Select(exprAbstractRule);

        // Also check for rules inside <rules> containers (implicitly abstract)
        filenav.MoveToRoot();
        var rulesContainerIt = filenav.Select(exprRulesContainer);

        if (it.Count == 0 && rulesContainerIt.Count == 0) return;

        abstracts = new(it.Count + rulesContainerIt.Count);

        // Dummy pattern to use for rule creation purposes. 
        // TODO: is there a better factory method implementation?
        var pt = schema.CreatePhase(string.Empty).CreatePattern(string.Empty);

        while (it.MoveNext())
        {
            var current = it.CurrentOrThrow();
            var rule = pt.CreateRule();
            rule.SetContext(schema.NsManager);
            rule.Id = current.GetAttribute("id", string.Empty);
            LoadAsserts(rule, current);
            LoadReports(rule, current);
            abstracts.Add(rule.Id, rule);
        }

        // Also collect rules inside <rules> containers (implicitly abstract, even without @abstract="true")
        while (rulesContainerIt.MoveNext())
        {
            var current = rulesContainerIt.CurrentOrThrow();
            var ruleId = current.GetAttribute("id", string.Empty);
            if (ruleId.Length == 0) continue;
            if (abstracts.ContainsKey(ruleId)) continue;

            var rule = pt.CreateRule();
            rule.SetContext(schema.NsManager);
            rule.Id = ruleId;
            LoadAsserts(rule, current);
            LoadReports(rule, current);
            abstracts.Add(rule.Id, rule);
        }
    }

    void LoadPhases()
    {
        filenav.MoveToRoot();
        var phases = filenav.Select(exprPhase);
        if (phases.Count == 0) return;

        while (phases.MoveNext())
        {
            var current = phases.CurrentOrThrow();
            var ph = schema.CreatePhase(current.GetAttribute("id", string.Empty));
            ph.From = current.GetAttribute("from", string.Empty);
            ph.When = current.GetAttribute("when", string.Empty);
            schema.Phases.Add(ph);
        }
    }

    void LoadPatterns()
    {
        filenav.MoveToRoot();
        var patterns = filenav.Select(exprPattern);
        filenav.MoveToRoot();
        var groups = filenav.Select(exprGroup);

        if (patterns.Count == 0 && groups.Count == 0) return;

        // A special #ALL phase which contains all the patterns in the schema.
        var phase = schema.CreatePhase(Phase.All);

        while (patterns.MoveNext())
        {
            var current = patterns.CurrentOrThrow();
            // Skip abstract patterns — they are templates; only instantiated via @is-a.
            var isAbstract = current.GetAttribute("abstract", string.Empty) == "true";
            if (isAbstract) continue;

            var pt = phase.CreatePattern(current.GetAttribute("name", string.Empty),
                current.GetAttribute("id", string.Empty));

            LoadLets(pt.Lets, current);

            var isA = current.GetAttribute("is-a", string.Empty);
            if (isA.Length > 0)
            {
                // Instantiate abstract pattern: collect param values, load rules from template.
                var paramValues = LoadParams(current);
                LoadRulesFromAbstractPattern(pt, isA, paramValues);
            }
            else
            {
                LoadRules(pt, current);
            }

            schema.Patterns.Add(pt);
            phase.Patterns.Add(pt);

            if (pt.Id != string.Empty)
            {
                // Select the phases in which this pattern is active, and add it 
                // to its collection of patterns. 
                // TODO: try to precompile this. Is it possible?
                var expr = Config.DefaultNavigator.Compile(
                    "//sch:phase[sch:active/@pattern=\"" + pt.Id + "\"]/@id");
                expr.SetContext(mgr);
                var phases = filenav.Select(expr);

                while (phases.MoveNext())
                {
                    schema.Phases[phases.CurrentOrThrow().Value].Patterns.Add(pt);
                }
            }
        }

        // Load <group> elements (ISO Schematron 2025)
        while (groups.MoveNext())
        {
            var current = groups.CurrentOrThrow();
            var grp = new Group(
                current.GetAttribute("name", string.Empty),
                current.GetAttribute("id", string.Empty));

            LoadLets(grp.Lets, current);
            LoadRules(grp, current);
            schema.Patterns.Add(grp);
            phase.Patterns.Add(grp);

            if (grp.Id != string.Empty)
            {
                var expr = Config.DefaultNavigator.Compile(
                    "//sch:phase[sch:active/@pattern=\"" + grp.Id + "\"]/@id");
                expr.SetContext(mgr);
                var phases = filenav.Select(expr);
                while (phases.MoveNext())
                    schema.Phases[phases.CurrentOrThrow().Value].Patterns.Add(grp);
            }
        }

        schema.Phases.Add(phase);
    }

    static Dictionary<string, string> LoadParams(XPathNavigator context)
    {
        var d = new Dictionary<string, string>(StringComparer.Ordinal);
        var it = context.SelectChildren(XPathNodeType.Element);
        while (it.MoveNext())
        {
            var current = it.CurrentOrThrow();
            if (current.LocalName == "param")
            {
                var name = current.GetAttribute("name", string.Empty);
                var value = current.GetAttribute("value", string.Empty);
                if (name.Length > 0)
                    d[name] = value;
            }
        }
        return d;
    }

    void LoadRulesFromAbstractPattern(Pattern target, string abstractId, Dictionary<string, string> paramValues)
    {
        // Find the abstract pattern node in the document.
        var expr = Config.DefaultNavigator.Compile(
            "//sch:pattern[@abstract=\"true\" and @id=\"" + abstractId + "\"]");
        expr.SetContext(mgr);
        filenav.MoveToRoot();
        var it = filenav.Select(expr);
        if (!it.MoveNext()) return;

        var rules = it.CurrentOrThrow().Select(exprConcreteRule);
        while (rules.MoveNext())
        {
            var current = rules.CurrentOrThrow();
            var ruleContext = SubstituteParams(
                current.GetAttribute("context", string.Empty), paramValues);

            var rule = target.CreateRule(ruleContext);
            rule.Id = current.GetAttribute("id", string.Empty);
            rule.SetContext(schema.NsManager);
            LoadLets(rule.Lets, current);

            var ruleFlag = current.GetAttribute("flag", string.Empty);
            rule.Flag = string.IsNullOrWhiteSpace(ruleFlag)
                ? []
                : ruleFlag.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

            rule.VisitEach = current.GetAttribute("visit-each", string.Empty);

            // Load asserts/reports with parameter substitution applied to test expressions.
            LoadAssertsWithSubstitution(rule, current, paramValues);
            LoadReportsWithSubstitution(rule, current, paramValues);
            target.Rules.Add(rule);
        }
    }

    static string SubstituteParams(string text, Dictionary<string, string> paramValues)
    {
        if (paramValues.Count == 0) return text;
        foreach (var kv in paramValues)
            text = text.Replace("$" + kv.Key, kv.Value);
        return text;
    }

    void LoadRules(Pattern pattern, XPathNavigator context)
    {
        var rules = context.Select(exprConcreteRule);
        if (rules.Count == 0) return;

        while (rules.MoveNext())
        {
            var current = rules.CurrentOrThrow();
            var rule = pattern.CreateRule(current.GetAttribute("context", string.Empty));
            rule.Id = current.GetAttribute("id", string.Empty);
            rule.SetContext(schema.NsManager);
            LoadLets(rule.Lets, current);
            LoadExtends(rule, current);
            LoadAsserts(rule, current);
            LoadReports(rule, current);

            var ruleFlag = current.GetAttribute("flag", string.Empty);
            rule.Flag = string.IsNullOrWhiteSpace(ruleFlag)
                ? []
                : ruleFlag.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

            rule.VisitEach = current.GetAttribute("visit-each", string.Empty);

            pattern.Rules.Add(rule);
        }
    }

    void LoadLets(LetCollection lets, XPathNavigator context)
    {
        var it = context.Select(exprLet);
        while (it.MoveNext())
        {
            var current = it.CurrentOrThrow();
            var let = new Let(
                current.GetAttribute("name", string.Empty),
                current.GetAttribute("value", string.Empty) is { Length: > 0 } value ? value : null,
                current.GetAttribute("as", string.Empty) is { Length: > 0 } a ? a : null);

            if (!lets.Contains(let.Name))
                lets.Add(let);
        }
    }

    void LoadExtends(Rule rule, XPathNavigator context)
    {
        var extends = context.Select(exprRuleExtends);
        if (extends.Count == 0) return;

        while (extends.MoveNext())
        {
            var ruleName = extends.CurrentOrThrow().GetAttribute("rule", string.Empty);
            if (abstracts?.ContainsKey(ruleName) == true && abstracts[ruleName] is Rule abstractRule)
                rule.Extend(abstractRule);
            else
                throw new BadSchemaException("The abstract rule with id=\"" + ruleName + "\" is used but not defined.");
        }
    }

    void LoadAsserts(Rule rule, XPathNavigator context)
    {
        var asserts = context.Select(exprAssert);
        if (asserts.Count == 0) return;

        while (asserts.MoveNext())
        {
            var current = asserts.CurrentOrThrow();
            var testExpr = current.GetAttribute("test", string.Empty);
            var message = current is IHasXmlNode node
                ? node.GetRequiredNode().InnerXml
                : current.Value;

            var asr = rule.CreateAssert(testExpr, message);
            asr.SetContext(schema.NsManager);
            ReadTestAttributes(asr, current);
            rule.Asserts.Add(asr);
        }
    }

    void LoadReports(Rule rule, XPathNavigator context)
    {
        var reports = context.Select(exprReport);
        if (reports.Count == 0) return;

        while (reports.MoveNext())
        {
            var current = reports.CurrentOrThrow();
            var testExpr = current.GetAttribute("test", string.Empty);
            var message = current is IHasXmlNode node
                ? node.GetRequiredNode().InnerXml
                : current.Value;

            var rpt = rule.CreateReport(testExpr, message);
            rpt.SetContext(schema.NsManager);
            ReadTestAttributes(rpt, current);
            rule.Reports.Add(rpt);
        }
    }

    void LoadDiagnostics(XPathNavigator context)
    {
        var it = context.Select(exprDiagnostic);
        while (it.MoveNext())
        {
            var current = it.CurrentOrThrow();
            if (current.GetAttribute("id", string.Empty) is not { Length: > 0 } id)
                continue;

            var msg = current is IHasXmlNode node
                ? node.GetRequiredNode().InnerXml
                : current.Value;
            if (!schema.Diagnostics.Contains(id))
                schema.Diagnostics.Add(new Diagnostic(id, msg));
        }
    }

    void LoadAssertsWithSubstitution(Rule rule, XPathNavigator context, Dictionary<string, string> paramValues)
    {
        var asserts = context.Select(exprAssert);
        if (asserts.Count == 0) return;

        while (asserts.MoveNext())
        {
            var current = asserts.CurrentOrThrow();
            var testExpr = SubstituteParams(
                current.GetAttribute("test", string.Empty), paramValues);
            var message = current is IHasXmlNode node
                ? node.GetRequiredNode().InnerXml
                : current.Value;
            message = SubstituteParams(message, paramValues);

            var asr = rule.CreateAssert(testExpr, message);
            asr.SetContext(schema.NsManager);
            ReadTestAttributes(asr, current);
            rule.Asserts.Add(asr);
        }
    }

    void LoadReportsWithSubstitution(Rule rule, XPathNavigator context, Dictionary<string, string> paramValues)
    {
        if (context.Select(exprReport) is not { Count: > 0 } reports)
            return;

        while (reports.MoveNext())
        {
            var current = reports.CurrentOrThrow();
            var testExpr = SubstituteParams(
                current.GetAttribute("test", string.Empty), paramValues);
            var message = current is IHasXmlNode node
                ? node.GetRequiredNode().InnerXml
                : current.Value;
            message = SubstituteParams(message, paramValues);

            var rpt = rule.CreateReport(testExpr, message);
            rpt.SetContext(schema.NsManager);
            ReadTestAttributes(rpt, current);
            rule.Reports.Add(rpt);
        }
    }

    static void ReadTestAttributes(Test test, XPathNavigator nav)
    {
        test.Id = nav.GetAttribute("id", string.Empty);
        test.Role = nav.GetAttribute("role", string.Empty);
        test.Severity = nav.GetAttribute("severity", string.Empty);

        var flag = nav.GetAttribute("flag", string.Empty);
        test.Flag = string.IsNullOrWhiteSpace(flag)
            ? []
            : flag.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        var diagnostics = nav.GetAttribute("diagnostics", string.Empty);
        test.DiagnosticRefs = string.IsNullOrWhiteSpace(diagnostics)
            ? []
            : diagnostics.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
    }

    void LoadSchemaParams(XPathNavigator context)
    {
        var it = context.SelectChildren(XPathNodeType.Element);
        while (it.MoveNext())
        {
            var current = it.CurrentOrThrow();
            if (current.LocalName == "param" && Schema.IsSchematronNamespace(current.NamespaceURI))
            {
                var name = current.GetAttribute("name", string.Empty);
                var value = current.GetAttribute("value", string.Empty);

                if (name.Length > 0 && !schema.Params.Contains(name))
                {
                    schema.Params.Add(new Param { Name = name, Value = value });
                    // Also expose as a schema-level let so they're available as variables
                    if (!schema.Lets.Contains(name))
                        schema.Lets.Add(new Let(name, value is { Length: > 0 } ? value : null));
                }
            }
        }
    }

    void LoadExtendsHref(XPathNavigator context)
    {
        var children = context.SelectChildren(XPathNodeType.Element);
        while (children.MoveNext())
        {
            var current = children.CurrentOrThrow();
            if (current.LocalName != "extends") continue;
            if (!Schema.IsSchematronNamespace(current.NamespaceURI)) continue;
            var href = current.GetAttribute("href", string.Empty);
            if (string.IsNullOrEmpty(href)) continue;

            // Resolve the href relative to the schema's base URI
            string resolvedPath;
            var baseUri = context.BaseURI;
            if (!string.IsNullOrEmpty(baseUri))
            {
                try
                {
                    resolvedPath = new Uri(new Uri(baseUri), href).LocalPath;
                }
                catch
                {
                    resolvedPath = href;
                }
            }
            else
            {
                resolvedPath = href;
            }

            if (!File.Exists(resolvedPath)) continue;

            try
            {
                var extSchema = new Schema();
                extSchema.Load(resolvedPath);

                // Merge diagnostics
                foreach (var d in extSchema.Diagnostics)
                    if (!schema.Diagnostics.Contains(d.Id))
                        schema.Diagnostics.Add(d);

                // Merge schema-level lets
                foreach (var let in extSchema.Lets)
                    if (!schema.Lets.Contains(let.Name))
                        schema.Lets.Add(let);
            }
            catch { /* skip extends that can't be loaded */ }
        }
    }
}

