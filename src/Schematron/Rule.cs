namespace Schematron;

/// <summary>
/// A Rule element.
/// </summary>
/// <remarks>
/// Rules enclose the <see cref="Assert"/> and <see cref="Report"/>
/// elements, providing the context for their evaluation.
/// According to the <link ref="schematron" />, nodes can be evaluated
/// by only one rule inside a <see cref="Pattern"/>, so typical schema
/// design includes placing the most exceptional rules first, down to
/// more generally applicable ones.
/// <para>
/// Constructor is not public. To programatically create an instance of this
/// class use the <see cref="Pattern.CreateRule(string)"/> factory method.
/// </para>
/// </remarks>
/// <author ref="dcazzulino">
/// Should we add support for the key element? <link ref="schematron" />
/// </author>
/// <progress amount="100" />
public class Rule : EvaluableExpression
{
    // TODO: add support to <key></key> child elements?
    readonly TestCollection asserts = [];
    readonly TestCollection reports = [];
    readonly LetCollection lets = [];

    /// <summary>
    /// Creates an abstract rule, without context.
    /// </summary>
    internal protected Rule() { }

    /// <summary>Initializes a new instance of the class, with the received context.</summary>
    /// <param name="context">The rule's context to evaluate.</param>
    /// <remarks>If passed a null or an <see cref="string.Empty"/>, this is implicitly an abstract rule.</remarks>
    internal protected Rule(string context) => InitContext(context);

    /// <summary>Initializes the context for the rule.</summary>
    /// <param name="context">The rule's context to evaluate.</param>
    /// <remarks>
    /// If passed a null or an <see cref="string.Empty"/>, this is implicitly an abstract rule.
    /// </remarks>
    /// <author ref="dcazzulino">
    /// Rules are evaluated through all the document (//), unless they
    /// explicitly want to start from the root (/). This is consistent
    /// with XSLT template match behavior. So we have to split the expression
    /// per union (|) to add the root expression in these cases.
    /// </author>
    void InitContext(string context)
    {
        if (string.IsNullOrEmpty(context))
        {
            IsAbstract = true;
            return;
        }


        // Rules are evaluated through all the document (//), unless they
        // explicitly want to start from the root (/).
        // We have to split per union (|) to add the root expression.
        var parts = context.Split('|');

        for (var i = 0; i < parts.Length; i++)
        {
            parts[i] = parts[i].Trim();

            if (!parts[i].StartsWith("/"))
                parts[i] = "//" + parts[i];
        }

        InitializeExpression(string.Join(" | ", parts));
        IsAbstract = false;
    }

    /// <summary>Creates a new assert instance.</summary>
    /// <remarks>
    /// Inheritors should override this method to create instances
    /// of their own assert implementations.
    /// </remarks>
    /// <param name="test">
    /// The XPath expression to test for the assert. See
    /// <see cref="EvaluableExpression.Expression"/>.
    /// </param>
    /// <param name="message">
    /// The message to display if the assert fails. See
    /// <see cref="Test.Message"/>.
    /// </param>
    public virtual Assert CreateAssert(string test, string message) => new Assert(test, message);

    /// <summary>Creates a new report instance.</summary>
    /// <remarks>
    /// Inheritors should override this method to create instances
    /// of their own report implementations.
    /// </remarks>
    /// <param name="test">
    /// The XPath expression to test for the report. See
    /// <see cref="EvaluableExpression.Expression"/>.
    /// </param>
    /// <param name="message">
    /// The message to display if the report succeeds. See
    /// <see cref="Test.Message"/>.
    /// </param>
    public virtual Report CreateReport(string test, string message) => new Report(test, message);

    /// <summary />
    public string Id { get; set; } = string.Empty;

    /// <summary />
    public string Context => base.Expression;

    /// <summary />
    public bool IsAbstract { get; private set; } = true;

    /// <summary>Gets the variable bindings declared in this rule (<c>&lt;let&gt;</c> elements).</summary>
    public LetCollection Lets => lets;

    /// <summary>Gets or sets the flag values declared on this rule (<c>@flag</c> attribute).</summary>
    public IReadOnlyList<string> Flag { get; set; } = [];

    /// <summary>Gets or sets the secondary visit path (<c>@visit-each</c> attribute, ISO Schematron 2025).
    /// For each node matched by <see cref="Context"/>, the <c>@visit-each</c> expression is evaluated
    /// and each resulting node is tested against the rule's asserts/reports.</summary>
    public string VisitEach { get; set; } = string.Empty;

    /// <summary />
    public TestCollection Asserts => asserts;

    /// <summary />
    public TestCollection Reports => reports;

    /// <summary />
    public void AddAssert(string test, string message) => asserts.Add(CreateAssert(test, message));

    /// <summary />
    public void AddReport(string test, string message) => reports.Add(CreateReport(test, message));

    /// <summary />
    /// <param name="parent"></param>
    /// <exception cref="InvalidOperationException">Only abstract rules can be used as a base for extensions.</exception>
    public void Extend(Rule parent)
    {
        if (!parent.IsAbstract)
            throw new ArgumentException("The rule to extend must be abstract.", "parent");

        foreach (Assert asr in parent.asserts)
            asserts.Add(asr);

        foreach (Report rpt in parent.reports)
            reports.Add(rpt);
    }
}

