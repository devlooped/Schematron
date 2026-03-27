using System.Collections;
using System.Xml;
using System.Xml.XPath;

namespace Schematron;

/// <summary />
public class SchemaLoader
{
    Schema _schema;
    XPathNavigator _filenav = null!;
    Hashtable? _abstracts = null;

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
        //_schema.NsManager = new GotDotNet.Exslt.ExsltContext(source.NameTable);

        XPathNodeIterator it = source.Select(CompiledExpressions.Schema);
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
            // Load child elements from the appinfo element if it exists.
            LoadSchemaElements(source.Select(CompiledExpressions.EmbeddedSchema));
        }

        #region Loading process start
        RetrieveAbstractRules();
        LoadPhases();
        LoadPatterns();
        #endregion
    }

    void LoadSchemaElement(XPathNavigator context)
    {
        string phase = context.GetAttribute("defaultPhase", String.Empty);
        if (phase != String.Empty)
            _schema.DefaultPhase = phase;

        //TODO: add all schema attributes in the future.
        LoadSchemaElements(context.SelectChildren(XPathNodeType.Element));
    }

    void LoadSchemaElements(XPathNodeIterator children)
    {
        while (children.MoveNext())
        {
            if (children.Current.NamespaceURI == Schema.Namespace)
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
        XPathNodeIterator it = _filenav.Select(CompiledExpressions.AbstractRule);
        if (it.Count == 0) return;

        _abstracts = new Hashtable(it.Count);

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
    }

    void LoadPhases()
    {
        _filenav.MoveToRoot();
        XPathNodeIterator phases = _filenav.Select(CompiledExpressions.Phase);
        if (phases.Count == 0) return;

        while (phases.MoveNext())
        {
            Phase ph = _schema.CreatePhase(phases.Current.GetAttribute("id", String.Empty));
            _schema.Phases.Add(ph);
        }
    }

    void LoadPatterns()
    {
        _filenav.MoveToRoot();
        XPathNodeIterator patterns = _filenav.Select(CompiledExpressions.Pattern);

        if (patterns.Count == 0) return;

        // A special #ALL phase which contains all the patterns in the schema.
        Phase phase = _schema.CreatePhase(Phase.All);

        while (patterns.MoveNext())
        {
            Pattern pt = phase.CreatePattern(patterns.Current.GetAttribute("name", String.Empty),
                patterns.Current.GetAttribute("id", String.Empty));

            LoadRules(pt, patterns.Current);
            _schema.Patterns.Add(pt);
            phase.Patterns.Add(pt);

            if (pt.Id != String.Empty)
            {
                // Select the phases in which this pattern is active, and add it 
                // to its collection of patterns. 
                // This is the only dynamic expression in the process.
                // TODO: try to precompile this. Is it possible?
                XPathExpression expr = Config.DefaultNavigator.Compile(
                    "//sch:phase[sch:active/@pattern=\"" + pt.Id + "\"]/@id");
                expr.SetContext(Config.DefaultNsManager);
                XPathNodeIterator phases = _filenav.Select(expr);

                while (phases.MoveNext())
                {
                    _schema.Phases[phases.Current.Value].Patterns.Add(pt);
                }
            }
        }

        _schema.Phases.Add(phase);
    }

    void LoadRules(Pattern pattern, XPathNavigator context)
    {
        XPathNodeIterator rules = context.Select(CompiledExpressions.ConcreteRule);
        if (rules.Count == 0) return;

        while (rules.MoveNext())
        {
            Rule rule = pattern.CreateRule(rules.Current.GetAttribute("context", String.Empty));
            rule.Id = rules.Current.GetAttribute("id", String.Empty);
            rule.SetContext(_schema.NsManager);
            LoadExtends(rule, rules.Current);
            LoadAsserts(rule, rules.Current);
            LoadReports(rule, rules.Current);
            pattern.Rules.Add(rule);
        }
    }

    void LoadExtends(Rule rule, XPathNavigator context)
    {
        XPathNodeIterator extends = context.Select(CompiledExpressions.RuleExtends);
        if (extends.Count == 0) return;

        while (extends.MoveNext())
        {
            string ruleName = extends.Current.GetAttribute("rule", String.Empty);
            if (_abstracts != null && _abstracts.ContainsKey(ruleName))
                rule.Extend((Rule)_abstracts[ruleName]);
            else
                throw new BadSchemaException("The abstract rule with id=\"" + ruleName + "\" is used but not defined.");
        }
    }

    void LoadAsserts(Rule rule, XPathNavigator context)
    {
        XPathNodeIterator asserts = context.Select(CompiledExpressions.Assert);
        if (asserts.Count == 0) return;

        while (asserts.MoveNext())
        {
            if (asserts.Current is IHasXmlNode node)
            {
                Assert asr = rule.CreateAssert(asserts.Current.GetAttribute("test", String.Empty),
                    node.GetNode().InnerXml);
                asr.SetContext(_schema.NsManager);
                rule.Asserts.Add(asr);
            }
            else
            {
                Assert asr = rule.CreateAssert(asserts.Current.GetAttribute("test", String.Empty),
                    asserts.Current.Value);
                asr.SetContext(_schema.NsManager);
                rule.Asserts.Add(asr);
            }
        }
    }

    void LoadReports(Rule rule, XPathNavigator context)
    {
        XPathNodeIterator reports = context.Select(CompiledExpressions.Report);
        if (reports.Count == 0) return;

        while (reports.MoveNext())
        {
            if (reports.Current is IHasXmlNode node)
            {
                Report rpt = rule.CreateReport(reports.Current.GetAttribute("test", String.Empty),
                    node.GetNode().InnerXml);
                rpt.SetContext(_schema.NsManager);
                rule.Reports.Add(rpt);
            }
            else
            {
                Report rpt = rule.CreateReport(reports.Current.GetAttribute("test", String.Empty),
                    reports.Current.Value);
                rpt.SetContext(_schema.NsManager);
                rule.Reports.Add(rpt);
            }
        }
    }
}

