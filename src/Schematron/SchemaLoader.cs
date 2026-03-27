using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Schematron;

/// <summary />
public class SchemaLoader
{
    Schema _schema;
    XPathNavigator _filenav = null!;
    Hashtable? _abstracts = null;

    // Detected Schematron namespace and the namespace manager derived from the source document.
    string _schNs = null!;
    XmlNamespaceManager _mgr = null!;

    // Instance-level XPath expressions compiled against the detected namespace.
    XPathExpression _exprSchema = null!;
    XPathExpression _exprEmbeddedSchema = null!;
    XPathExpression _exprPhase = null!;
    XPathExpression _exprPattern = null!;
    XPathExpression _exprAbstractRule = null!;
    XPathExpression _exprConcreteRule = null!;
    XPathExpression _exprRuleExtends = null!;
    XPathExpression _exprAssert = null!;
    XPathExpression _exprReport = null!;
    XPathExpression _exprLet = null!;
    XPathExpression _exprDiagnostic = null!;
    XPathExpression _exprParam = null!;
    XPathExpression _exprLibrary = null!;
    XPathExpression _exprRulesContainer = null!;
    XPathExpression _exprGroup = null!;

    /// <summary />
    public SchemaLoader(Schema schema)
    {
        _schema = schema;
    }

    /// <summary />
    /// <param name="source"></param>
    public virtual void LoadSchema(XPathNavigator source)
    {
        _schema.NsManager = new XmlNamespaceManager(source.NameTable);

        DetectAndBuildExpressions(source);

        XPathNodeIterator it = source.Select(_exprSchema);
        if (it.Count > 1)
            throw new BadSchemaException("There can be at most one schema element per Schematron schema.");

        // Always work with the whole document to look for elements.
        // Embedded schematron will work as well as stand-alone schemas.
        _filenav = source;

        if (it.Count == 1)
        {
            it.MoveNext();
            LoadSchemaElement(it.Current);
        }
        else
        {
            // Check for <library> root element (ISO Schematron 2025)
            XPathNodeIterator libIt = source.Select(_exprLibrary);
            if (libIt.Count == 1)
            {
                libIt.MoveNext();
                _schema.IsLibrary = true;
                LoadSchemaElement(libIt.Current);
            }
            else
            {
                // Load child elements from the appinfo element if it exists.
                LoadSchemaElements(source.Select(_exprEmbeddedSchema));
            }
        }

        #region Loading process start
        RetrieveAbstractRules();
        LoadPhases();
        LoadPatterns();
        #endregion
    }

    /// <summary>
    /// Detects the Schematron namespace used in <paramref name="source"/> and compiles all
    /// instance-level XPath expressions against that namespace.
    /// </summary>
    void DetectAndBuildExpressions(XPathNavigator source)
    {
        _schNs = DetectSchematronNamespace(source);

        _mgr = new XmlNamespaceManager(source.NameTable);
        _mgr.AddNamespace("sch", _schNs);
        _mgr.AddNamespace("xsd", System.Xml.Schema.XmlSchema.Namespace);

        _exprSchema = Compile("//sch:schema");
        _exprEmbeddedSchema = Compile("xsd:schema/xsd:annotation/xsd:appinfo/*");
        _exprPhase = Compile("descendant-or-self::sch:phase");
        _exprPattern = Compile("//sch:pattern");
        _exprAbstractRule = Compile("//sch:rule[@abstract=\"true\"]");
        _exprConcreteRule = Compile("descendant-or-self::sch:rule[not(@abstract) or @abstract=\"false\"]");
        _exprRuleExtends = Compile("descendant-or-self::sch:extends");
        _exprAssert = Compile("descendant-or-self::sch:assert");
        _exprReport = Compile("descendant-or-self::sch:report");
        _exprLet = Compile("sch:let");
        _exprDiagnostic = Compile("sch:diagnostics/sch:diagnostic");
        _exprParam = Compile("sch:param");
        _exprLibrary = Compile("//sch:library");
        _exprRulesContainer = Compile("//sch:rules/sch:rule");
        _exprGroup = Compile("//sch:group");
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
                if (it.Current.NamespaceURI == Schema.IsoNamespace) return Schema.IsoNamespace;
                if (it.Current.NamespaceURI == Schema.LegacyNamespace) return Schema.LegacyNamespace;
            }
        }

        return Schema.IsoNamespace;
    }

    XPathExpression Compile(string xpath)
    {
        var expr = Config.DefaultNavigator.Compile(xpath);
        expr.SetContext(_mgr);
        return expr;
    }

    void LoadSchemaElement(XPathNavigator context)
    {
        string phase = context.GetAttribute("defaultPhase", String.Empty);
        if (phase != String.Empty)
            _schema.DefaultPhase = phase;

        string edition = context.GetAttribute("schematronEdition", String.Empty);
        if (edition != String.Empty)
            _schema.SchematronEdition = edition;

        LoadSchemaElements(context.SelectChildren(XPathNodeType.Element));
        LoadLets(_schema.Lets, context);
        LoadDiagnostics(context);
        LoadSchemaParams(context);
        LoadExtendsHref(context);
    }

    void LoadSchemaElements(XPathNodeIterator children)
    {
        while (children.MoveNext())
        {
            if (children.Current.NamespaceURI == _schNs)
            {
                if (children.Current.LocalName == "title")
                {
                    _schema.Title = children.Current.Value;
                }
                else if (children.Current.LocalName == "ns")
                {
                    _schema.NsManager.AddNamespace(
                        children.Current.GetAttribute("prefix", String.Empty),
                        children.Current.GetAttribute("uri", String.Empty));
                }
            }
        }
    }

    void RetrieveAbstractRules()
    {
        _filenav.MoveToRoot();
        XPathNodeIterator it = _filenav.Select(_exprAbstractRule);

        // Also check for rules inside <rules> containers (implicitly abstract)
        _filenav.MoveToRoot();
        XPathNodeIterator rulesContainerIt = _filenav.Select(_exprRulesContainer);

        if (it.Count == 0 && rulesContainerIt.Count == 0) return;

        _abstracts = new Hashtable(it.Count + rulesContainerIt.Count);

        // Dummy pattern to use for rule creation purposes. 
        // TODO: is there a better factory method implementation?
        Pattern pt = _schema.CreatePhase(String.Empty).CreatePattern(String.Empty);

        while (it.MoveNext())
        {
            Rule rule = pt.CreateRule();
            rule.SetContext(_schema.NsManager);
            rule.Id = it.Current.GetAttribute("id", String.Empty);
            LoadAsserts(rule, it.Current);
            LoadReports(rule, it.Current);
            _abstracts.Add(rule.Id, rule);
        }

        // Also collect rules inside <rules> containers (implicitly abstract, even without @abstract="true")
        while (rulesContainerIt.MoveNext())
        {
            string ruleId = rulesContainerIt.Current.GetAttribute("id", String.Empty);
            if (ruleId.Length == 0) continue;
            if (_abstracts.ContainsKey(ruleId)) continue;

            Rule rule = pt.CreateRule();
            rule.SetContext(_schema.NsManager);
            rule.Id = ruleId;
            LoadAsserts(rule, rulesContainerIt.Current);
            LoadReports(rule, rulesContainerIt.Current);
            _abstracts.Add(rule.Id, rule);
        }
    }

    void LoadPhases()
    {
        _filenav.MoveToRoot();
        XPathNodeIterator phases = _filenav.Select(_exprPhase);
        if (phases.Count == 0) return;

        while (phases.MoveNext())
        {
            Phase ph = _schema.CreatePhase(phases.Current.GetAttribute("id", String.Empty));
            ph.From = phases.Current.GetAttribute("from", String.Empty);
            ph.When = phases.Current.GetAttribute("when", String.Empty);
            _schema.Phases.Add(ph);
        }
    }

    void LoadPatterns()
    {
        _filenav.MoveToRoot();
        XPathNodeIterator patterns = _filenav.Select(_exprPattern);
        _filenav.MoveToRoot();
        XPathNodeIterator groups = _filenav.Select(_exprGroup);

        if (patterns.Count == 0 && groups.Count == 0) return;

        // A special #ALL phase which contains all the patterns in the schema.
        Phase phase = _schema.CreatePhase(Phase.All);

        while (patterns.MoveNext())
        {
            // Skip abstract patterns — they are templates; only instantiated via @is-a.
            bool isAbstract = patterns.Current.GetAttribute("abstract", String.Empty) == "true";
            if (isAbstract) continue;

            Pattern pt = phase.CreatePattern(patterns.Current.GetAttribute("name", String.Empty),
                patterns.Current.GetAttribute("id", String.Empty));

            LoadLets(pt.Lets, patterns.Current);

            string isA = patterns.Current.GetAttribute("is-a", String.Empty);
            if (isA.Length > 0)
            {
                // Instantiate abstract pattern: collect param values, load rules from template.
                var paramValues = LoadParams(patterns.Current);
                LoadRulesFromAbstractPattern(pt, isA, paramValues);
            }
            else
            {
                LoadRules(pt, patterns.Current);
            }

            _schema.Patterns.Add(pt);
            phase.Patterns.Add(pt);

            if (pt.Id != String.Empty)
            {
                // Select the phases in which this pattern is active, and add it 
                // to its collection of patterns. 
                // TODO: try to precompile this. Is it possible?
                XPathExpression expr = Config.DefaultNavigator.Compile(
                    "//sch:phase[sch:active/@pattern=\"" + pt.Id + "\"]/@id");
                expr.SetContext(_mgr);
                XPathNodeIterator phases = _filenav.Select(expr);

                while (phases.MoveNext())
                {
                    _schema.Phases[phases.Current.Value].Patterns.Add(pt);
                }
            }
        }

        // Load <group> elements (ISO Schematron 2025)
        while (groups.MoveNext())
        {
            var grp = new Group(
                groups.Current.GetAttribute("name", String.Empty),
                groups.Current.GetAttribute("id", String.Empty));

            LoadLets(grp.Lets, groups.Current);
            LoadRules(grp, groups.Current);
            _schema.Patterns.Add(grp);
            phase.Patterns.Add(grp);

            if (grp.Id != String.Empty)
            {
                XPathExpression expr = Config.DefaultNavigator.Compile(
                    "//sch:phase[sch:active/@pattern=\"" + grp.Id + "\"]/@id");
                expr.SetContext(_mgr);
                XPathNodeIterator phases = _filenav.Select(expr);
                while (phases.MoveNext())
                    _schema.Phases[phases.Current.Value].Patterns.Add(grp);
            }
        }

        _schema.Phases.Add(phase);
    }

    static Dictionary<string, string> LoadParams(XPathNavigator context)
    {
        var d = new Dictionary<string, string>(StringComparer.Ordinal);
        XPathNodeIterator it = context.SelectChildren(XPathNodeType.Element);
        while (it.MoveNext())
        {
            if (it.Current.LocalName == "param")
            {
                string name = it.Current.GetAttribute("name", String.Empty);
                string value = it.Current.GetAttribute("value", String.Empty);
                if (name.Length > 0)
                    d[name] = value;
            }
        }
        return d;
    }

    void LoadRulesFromAbstractPattern(Pattern target, string abstractId, Dictionary<string, string> paramValues)
    {
        // Find the abstract pattern node in the document.
        XPathExpression expr = Config.DefaultNavigator.Compile(
            "//sch:pattern[@abstract=\"true\" and @id=\"" + abstractId + "\"]");
        expr.SetContext(_mgr);
        _filenav.MoveToRoot();
        XPathNodeIterator it = _filenav.Select(expr);
        if (!it.MoveNext()) return;

        XPathNodeIterator rules = it.Current.Select(_exprConcreteRule);
        while (rules.MoveNext())
        {
            string ruleContext = SubstituteParams(
                rules.Current.GetAttribute("context", String.Empty), paramValues);

            Rule rule = target.CreateRule(ruleContext);
            rule.Id = rules.Current.GetAttribute("id", String.Empty);
            rule.SetContext(_schema.NsManager);
            LoadLets(rule.Lets, rules.Current);

            string ruleFlag = rules.Current.GetAttribute("flag", String.Empty);
            rule.Flag = string.IsNullOrWhiteSpace(ruleFlag)
                ? Array.Empty<string>()
                : ruleFlag.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

            rule.VisitEach = rules.Current.GetAttribute("visit-each", String.Empty);

            // Load asserts/reports with parameter substitution applied to test expressions.
            LoadAssertsWithSubstitution(rule, rules.Current, paramValues);
            LoadReportsWithSubstitution(rule, rules.Current, paramValues);
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
        XPathNodeIterator rules = context.Select(_exprConcreteRule);
        if (rules.Count == 0) return;

        while (rules.MoveNext())
        {
            Rule rule = pattern.CreateRule(rules.Current.GetAttribute("context", String.Empty));
            rule.Id = rules.Current.GetAttribute("id", String.Empty);
            rule.SetContext(_schema.NsManager);
            LoadLets(rule.Lets, rules.Current);
            LoadExtends(rule, rules.Current);
            LoadAsserts(rule, rules.Current);
            LoadReports(rule, rules.Current);

            string ruleFlag = rules.Current.GetAttribute("flag", String.Empty);
            rule.Flag = string.IsNullOrWhiteSpace(ruleFlag)
                ? Array.Empty<string>()
                : ruleFlag.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

            rule.VisitEach = rules.Current.GetAttribute("visit-each", String.Empty);

            pattern.Rules.Add(rule);
        }
    }

    void LoadLets(LetCollection lets, XPathNavigator context)
    {
        XPathNodeIterator it = context.Select(_exprLet);
        while (it.MoveNext())
        {
            var let = new Let
            {
                Name = it.Current.GetAttribute("name", String.Empty),
                Value = it.Current.GetAttribute("value", String.Empty),
                As = it.Current.GetAttribute("as", String.Empty) is { Length: > 0 } a ? a : null,
            };
            if (let.Value?.Length == 0) let.Value = null;
            if (!lets.Contains(let.Name))
                lets.Add(let);
        }
    }

    void LoadExtends(Rule rule, XPathNavigator context)
    {
        XPathNodeIterator extends = context.Select(_exprRuleExtends);
        if (extends.Count == 0) return;

        while (extends.MoveNext())
        {
            string ruleName = extends.Current.GetAttribute("rule", String.Empty);
            if (_abstracts != null && _abstracts.ContainsKey(ruleName))
                rule.Extend((Rule)_abstracts[ruleName]!);
            else
                throw new BadSchemaException("The abstract rule with id=\"" + ruleName + "\" is used but not defined.");
        }
    }

    void LoadAsserts(Rule rule, XPathNavigator context)
    {
        XPathNodeIterator asserts = context.Select(_exprAssert);
        if (asserts.Count == 0) return;

        while (asserts.MoveNext())
        {
            string testExpr = asserts.Current.GetAttribute("test", String.Empty);
            string message = asserts.Current is IHasXmlNode node
                ? node.GetNode().InnerXml
                : asserts.Current.Value;

            Assert asr = rule.CreateAssert(testExpr, message);
            asr.SetContext(_schema.NsManager);
            ReadTestAttributes(asr, asserts.Current);
            rule.Asserts.Add(asr);
        }
    }

    void LoadReports(Rule rule, XPathNavigator context)
    {
        XPathNodeIterator reports = context.Select(_exprReport);
        if (reports.Count == 0) return;

        while (reports.MoveNext())
        {
            string testExpr = reports.Current.GetAttribute("test", String.Empty);
            string message = reports.Current is IHasXmlNode node
                ? node.GetNode().InnerXml
                : reports.Current.Value;

            Report rpt = rule.CreateReport(testExpr, message);
            rpt.SetContext(_schema.NsManager);
            ReadTestAttributes(rpt, reports.Current);
            rule.Reports.Add(rpt);
        }
    }

    void LoadDiagnostics(XPathNavigator context)
    {
        XPathNodeIterator it = context.Select(_exprDiagnostic);
        while (it.MoveNext())
        {
            string id = it.Current.GetAttribute("id", String.Empty);
            if (id.Length == 0) continue;
            string msg = it.Current is IHasXmlNode node
                ? node.GetNode().InnerXml
                : it.Current.Value;
            if (!_schema.Diagnostics.Contains(id))
                _schema.Diagnostics.Add(new Diagnostic { Id = id, Message = msg });
        }
    }

    void LoadAssertsWithSubstitution(Rule rule, XPathNavigator context, Dictionary<string, string> paramValues)
    {
        XPathNodeIterator asserts = context.Select(_exprAssert);
        if (asserts.Count == 0) return;

        while (asserts.MoveNext())
        {
            string testExpr = SubstituteParams(
                asserts.Current.GetAttribute("test", String.Empty), paramValues);
            string message = asserts.Current is IHasXmlNode node
                ? node.GetNode().InnerXml
                : asserts.Current.Value;
            message = SubstituteParams(message, paramValues);

            Assert asr = rule.CreateAssert(testExpr, message);
            asr.SetContext(_schema.NsManager);
            ReadTestAttributes(asr, asserts.Current);
            rule.Asserts.Add(asr);
        }
    }

    void LoadReportsWithSubstitution(Rule rule, XPathNavigator context, Dictionary<string, string> paramValues)
    {
        XPathNodeIterator reports = context.Select(_exprReport);
        if (reports.Count == 0) return;

        while (reports.MoveNext())
        {
            string testExpr = SubstituteParams(
                reports.Current.GetAttribute("test", String.Empty), paramValues);
            string message = reports.Current is IHasXmlNode node
                ? node.GetNode().InnerXml
                : reports.Current.Value;
            message = SubstituteParams(message, paramValues);

            Report rpt = rule.CreateReport(testExpr, message);
            rpt.SetContext(_schema.NsManager);
            ReadTestAttributes(rpt, reports.Current);
            rule.Reports.Add(rpt);
        }
    }

    static void ReadTestAttributes(Test test, XPathNavigator nav)
    {
        test.Id = nav.GetAttribute("id", String.Empty);
        test.Role = nav.GetAttribute("role", String.Empty);
        test.Severity = nav.GetAttribute("severity", String.Empty);

        string flag = nav.GetAttribute("flag", String.Empty);
        test.Flag = string.IsNullOrWhiteSpace(flag)
            ? Array.Empty<string>()
            : flag.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        string diagnostics = nav.GetAttribute("diagnostics", String.Empty);
        test.DiagnosticRefs = string.IsNullOrWhiteSpace(diagnostics)
            ? Array.Empty<string>()
            : diagnostics.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
    }

    void LoadSchemaParams(XPathNavigator context)
    {
        XPathNodeIterator it = context.SelectChildren(XPathNodeType.Element);
        while (it.MoveNext())
        {
            if (it.Current.LocalName == "param" && Schema.IsSchematronNamespace(it.Current.NamespaceURI))
            {
                string name = it.Current.GetAttribute("name", String.Empty);
                string value = it.Current.GetAttribute("value", String.Empty);
                if (name.Length > 0 && !_schema.Params.Contains(name))
                {
                    _schema.Params.Add(new Param { Name = name, Value = value });
                    // Also expose as a schema-level let so they're available as variables
                    if (!_schema.Lets.Contains(name))
                        _schema.Lets.Add(new Let { Name = name, Value = value });
                }
            }
        }
    }

    void LoadExtendsHref(XPathNavigator context)
    {
        XPathNodeIterator children = context.SelectChildren(XPathNodeType.Element);
        while (children.MoveNext())
        {
            if (children.Current.LocalName != "extends") continue;
            if (!Schema.IsSchematronNamespace(children.Current.NamespaceURI)) continue;
            string href = children.Current.GetAttribute("href", String.Empty);
            if (string.IsNullOrEmpty(href)) continue;

            // Resolve the href relative to the schema's base URI
            string resolvedPath;
            string baseUri = context.BaseURI;
            if (!string.IsNullOrEmpty(baseUri))
            {
                try
                {
                    Uri resolved = new Uri(new Uri(baseUri), href);
                    resolvedPath = resolved.LocalPath;
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
                foreach (Diagnostic d in extSchema.Diagnostics)
                    if (!_schema.Diagnostics.Contains(d.Id))
                        _schema.Diagnostics.Add(d);

                // Merge schema-level lets
                foreach (Let let in extSchema.Lets)
                    if (!_schema.Lets.Contains(let.Name))
                        _schema.Lets.Add(let);
            }
            catch { /* skip extends that can't be loaded */ }
        }
    }
}

