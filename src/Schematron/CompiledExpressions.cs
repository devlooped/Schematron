using System.Xml.XPath;

namespace Schematron;

/// <summary>
/// This class keeps static precompiled expressions used
/// in the Schematron schema loading and validation processes.
/// </summary>
/// <remarks>
/// All expressions are compiled against the <see cref="Config.DefaultNavigator"/>
/// object. All the <see cref="XPathExpression"/> objects are initialized with
/// the <see cref="Config.DefaultNsManager"/> for schematron and XML Schema
/// namespaces resolution.
/// </remarks>
/// <author ref="kzu" />
/// <progress amount="100" />
class CompiledExpressions
{
    public static XPathExpression Schema;
    public static XPathExpression EmbeddedSchema;
    public static XPathExpression Phase;
    public static XPathExpression Pattern;
    public static XPathExpression AbstractRule;
    public static XPathExpression ConcreteRule;
    public static XPathExpression RuleExtends;
    public static XPathExpression Assert;
    public static XPathExpression Report;

    //Not implemented yet.
    //public static XPathExpression Diagnostics;

    static CompiledExpressions()
    {
        var mgr = Config.DefaultNsManager;

        Schema = Config.DefaultNavigator.Compile("//sch:schema");
        Schema.SetContext(mgr);

        EmbeddedSchema = Config.DefaultNavigator.Compile("xsd:schema/xsd:annotation/xsd:appinfo/*");
        EmbeddedSchema.SetContext(mgr);

        // We use descendant-or-self for the XPath because the phase will always be
        // contained in a parent element, which is the context for the execution,
        // usually the sch:schema element or an xsd:appinfo.
        Phase = Config.DefaultNavigator.Compile("descendant-or-self::sch:phase");
        Phase.SetContext(mgr);

        Pattern = Config.DefaultNavigator.Compile("//sch:pattern");
        Pattern.SetContext(mgr);

        // Abstract rules can be anywhere on the schema too.
        AbstractRule = Config.DefaultNavigator.Compile("//sch:rule[@abstract=\"true\"]");
        AbstractRule.SetContext(mgr);

        // We use descendant-or-self because we want to search for rules under the current pattern context.
        ConcreteRule = Config.DefaultNavigator.Compile("descendant-or-self::sch:rule[not(@abstract) or @abstract=\"false\"]");
        ConcreteRule.SetContext(mgr);

        // We use descendant-or-self because sch:extends is always a child of its context, sch:rule.
        RuleExtends = Config.DefaultNavigator.Compile("descendant-or-self::sch:extends");
        RuleExtends.SetContext(mgr);

        // We use descendant-or-self because sch:assert is always a child of its context, sch:rule.
        Assert = Config.DefaultNavigator.Compile("descendant-or-self::sch:assert");
        Assert.SetContext(mgr);

        // We use descendant-or-self because sch:report is always a child of its context, sch:rule.
        Report = Config.DefaultNavigator.Compile("descendant-or-self::sch:report");
        Report.SetContext(mgr);

        // We use descendant-or-self because sch:diagnostics can only be used inside a sch:schema element.
        // Currently not in implemented.
        // Diagnostics = Config.DefaultNavigator.Compile("descendant-or-self::sch:diagnostics");
        // Diagnostics.SetContext(mgr);
    }

    CompiledExpressions() { }
}

