using System.Xml;
using System.Xml.XPath;

namespace Schematron;

/// <summary>
/// Base class for elements that can be evaluated by an XPath expression.
/// </summary>
/// <remarks>
/// This class performs the expression compilation, and provides
/// access to the context through two methods.
/// </remarks>
/// <author ref="kzu" />
/// <progress amount="100" />
public abstract class EvaluableExpression
{
    string xpath = null!;
    XPathExpression expr = null!;
    XmlNamespaceManager? ns;

    /// <summary>
    /// Cache the return type to avoid cloning the expression.
    /// </summary>
    XPathResultType ret;

    /// <summary>Initializes a new instance of the element with the expression specified.</summary>
    /// <param name="xpathExpression">The expression to evaluate.</param>
    internal protected EvaluableExpression(string xpathExpression) => InitializeExpression(xpathExpression);

    /// <summary>Initializes a new instance of the element.</summary>
    internal protected EvaluableExpression() { }

    /// <summary>Reinitializes the element with a new expression,
    /// after the class has already been constructed</summary>
    /// <param name="xpathExpression">The expression to evaluate.</param>
    protected void InitializeExpression(string xpathExpression)
    {
        xpath = xpathExpression;
        expr = Config.DefaultNavigator.Compile(xpathExpression);
        ret = expr.ReturnType;

        if (ns != null)
            expr.SetContext(ns);
    }

    /// <summary>Contains the compiled version of the expression.</summary>
    /// <remarks>
    /// A clone of the expression is always returned, because the compiled
    /// expression is not thread-safe for evaluation.
    /// </remarks>
    public XPathExpression CompiledExpression => expr != null ? expr.Clone() : null!;

    /// <summary>Contains the string version of the expression.</summary>
    public string Expression => xpath;

    /// <summary>Contains the string version of the expression.</summary>
    public XPathResultType ReturnType => ret;

    /// <summary>Returns the manager in use to resolve expression namespaces.</summary>
    public XmlNamespaceManager? GetContext() => ns;

    /// <summary>Sets the manager to use to resolve expression namespaces.</summary>
    public void SetContext(XmlNamespaceManager nsManager)
    {
        if (expr != null)
        {
            // When the expression contains variable references ($name), .NET requires an
            // XsltContext (not just XmlNamespaceManager). Use a load-time stub that satisfies
            // the requirement; actual variable values are injected at evaluation time.
            try
            {
                expr.SetContext(nsManager);
            }
            catch (System.Xml.XPath.XPathException)
            {
                // Expression contains variables – use a load-time XsltContext stub.
                expr.SetContext(SchematronXsltContext.ForLoading(nsManager));
            }
        }
        ns = nsManager;
    }
}

