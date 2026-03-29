using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Schematron;

/// <summary>
/// An <see cref="XsltContext"/> that resolves XPath variable references from Schematron
/// <c>&lt;let&gt;</c> declarations. Variables are evaluated lazily against a context node.
/// </summary>
sealed class SchematronXsltContext : XsltContext
{
    readonly Dictionary<string, string> variables;
    readonly XPathNavigator? contextNode;

    /// <summary>
    /// Thread-local ambient context for the current Schematron evaluation. Used by
    /// <see cref="Schematron.Formatters.FormatterBase"/> to resolve variables inside
    /// <c>&lt;value-of&gt;</c> message expressions without changing the formatter API.
    /// </summary>
    [ThreadStatic]
    internal static SchematronXsltContext? Current;

    /// <summary>
    /// Creates a load-time (stub) context for schema loading. Variables return empty strings.
    /// Useful for passing to <see cref="XPathExpression.SetContext(IXmlNamespaceResolver)"/> when the
    /// expression contains variable references but actual values are not yet available.
    /// </summary>
    public static SchematronXsltContext ForLoading(XmlNamespaceManager nsManager)
        => new([], null, nsManager);

    /// <summary>
    /// Initialises a new instance from accumulated <c>&lt;let&gt;</c> declarations,
    /// evaluated in the order schema → pattern → rule (inner declarations shadow outer ones).
    /// </summary>
    public SchematronXsltContext(
        Dictionary<string, string> variables,
        XPathNavigator? contextNode,
        XmlNamespaceManager nsManager)
        : base(new System.Xml.NameTable())
    {
        this.variables = variables;
        this.contextNode = contextNode;

        // Copy prefix→namespace mappings so the context can resolve namespace prefixes used
        // in variable-value XPath expressions.
        foreach (string prefix in nsManager)
        {
            if (string.IsNullOrEmpty(prefix)) continue;
            // "xml" and "xmlns" are reserved and cannot be re-added.
            if (prefix == "xml" || prefix == "xmlns") continue;
            var ns = nsManager.LookupNamespace(prefix) ?? string.Empty;
            if (ns.Length > 0)
            {
                try { AddNamespace(prefix, ns); }
                catch (ArgumentException) { /* reserved or invalid – skip */ }
            }
        }
    }

    /// <inheritdoc />
    public override bool Whitespace => false;

    /// <inheritdoc />
    public override bool PreserveWhitespace(XPathNavigator node) => false;

    /// <inheritdoc />
    public override int CompareDocument(string baseUri, string nextbaseUri) => 0;

    /// <inheritdoc />
    public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] argTypes)
        => throw new NotSupportedException($"External function '{name}' is not supported.");

    /// <inheritdoc />
    public override IXsltContextVariable ResolveVariable(string prefix, string name)
    {
        if (variables.TryGetValue(name, out var expr))
            return new LetVariable(name, expr, contextNode);

        // Return an empty-string variable for undefined references rather than throwing,
        // so that schemas with forward-declared variables do not crash during evaluation.
        return new LetVariable(name, "", contextNode);
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the raw string value for a variable by name, or <see langword="null"/> if not found.
    /// </summary>
    public string? GetVariableValue(string name)
        => variables.TryGetValue(name, out var val) ? val : null;

    // -------------------------------------------------------------------------

    sealed class LetVariable(string name, string expr, XPathNavigator? context) : IXsltContextVariable
    {
        public bool IsLocal => true;
        public bool IsParam => false;
        public XPathResultType VariableType => XPathResultType.Any;

        public object Evaluate(XsltContext xsltContext)
        {
            if (string.IsNullOrEmpty(expr) || context is null)
                return string.Empty;

            try
            {
                var compiled = context.Compile(expr);
                compiled.SetContext(xsltContext);
                return context.Evaluate(compiled);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
