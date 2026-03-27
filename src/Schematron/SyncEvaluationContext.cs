using System.Collections;
using System.Text;
using System.Xml.XPath;

namespace Schematron;

/// <summary>
/// Evaluates all the schema elements synchronously.
/// </summary>
/// <authorref id="dcazzulino" />
/// <progress amount="100" />
/// <remarks>
/// See <see cref="EvaluationContextBase"/> for a description of the purpose and 
/// of evaluation contexts, and where are they used.
/// </remarks>
class SyncEvaluationContext : EvaluationContextBase
{
    /// <summary>Creates the evaluation context</summary>
    public SyncEvaluationContext()
    {
    }

    /// <summary>
    /// Starts the evaluation process.
    /// </summary>
    /// <remarks>
    /// When the process is finished, the results are placed 
    /// in the <see cref="EvaluationContextBase.Messages"/> property.
    /// </remarks>
    public override void Start()
    {
        this.Reset();

        // Is there something to evaluate at all?
        if (Schema.Patterns.Count == 0) return;

        // If no phase was received, try the default phase defined for the schema.
        // If no default phase is defined, all patterns will be tested.
        if (Phase == string.Empty)
        {
            Phase = !string.IsNullOrEmpty(Schema.DefaultPhase)
                ? Schema.DefaultPhase
                : Schematron.Phase.All;
        }

        if (Schema.Phases[Phase] == null)
            throw new ArgumentException("The specified Phase isn't defined for the current schema.");

        if (Evaluate(Schema.Phases[Phase], Messages))
        {
            Formatter.Format(Schema, Source, Messages);
            HasErrors = true;
        }
    }

    /// <summary>
    /// Evaluates the selected <see cref="Phase"/>.
    /// </summary>
    /// <remarks>
    ///	As most of the other evaluation methods, it repositions the 
    ///	<see cref="EvaluationContextBase.Source"/> navigator on the root node.
    /// </remarks>
    /// <param name="phase">The <see cref="Phase"/> to evaluate.</param>
    /// <param name="output">Contains the builder to accumulate messages in.</param>
    /// <returns>A boolean indicating the presence of errors (true).</returns>
    bool Evaluate(Phase phase, StringBuilder output)
    {
        bool failed = false;
        Source.MoveToRoot();
        var sb = new StringBuilder();

        foreach (Pattern pt in phase.Patterns)
        {
            if (Evaluate(pt, sb)) failed = true;
        }

        if (failed)
        {
            Formatter.Format(phase, Source, sb);
            output.Append(sb.ToString());
        }

        return failed;
    }

    /// <summary>
    /// Evaluates the selected <see cref="Pattern"/>.
    /// </summary>
    /// <remarks>
    ///	As most of the other evaluation methods, it repositions the 
    ///	<see cref="EvaluationContextBase.Source"/> navigator on the root node.
    /// <para>
    /// Clears the <see cref="EvaluationContextBase.Matched"/> object before
    /// proceeding, as the restriction about node mathing (see <linkref id="schematron" />)
    /// applies only inside a single pattern.
    /// </para>
    /// </remarks>
    /// <param name="pattern">The <see cref="Pattern"/> to evaluate.</param>
    /// <param name="output">Contains the builder to accumulate messages in.</param>
    /// <returns>A boolean indicating if a new message was added.</returns>
    bool Evaluate(Pattern pattern, StringBuilder output)
    {
        bool failed = false;
        Source.MoveToRoot();
        var sb = new StringBuilder();

        // Reset matched nodes, as across patters, nodes can be 
        // evaluated more than once.
        Matched.Clear();

        foreach (Rule rule in pattern.Rules)
        {
            if (Evaluate(rule, sb)) failed = true;
        }

        if (failed)
        {
            Formatter.Format(pattern, Source, sb);
            output.Append(sb.ToString());
        }

        return failed;
    }

    /// <summary>
    /// Evaluates the selected <see cref="Rule"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="Rule.Asserts"/> and <see cref="Rule.Reports"/> are iterated
    /// and each <see cref="Assert"/> and <see cref="Report"/> is executed against
    /// the context selected by the <see cref="Rule.Context"/>.
    /// <para>
    /// Nodes matched are added to the <see cref="EvaluationContextBase.Matched"/> list of 
    /// nodes to skip in the next rule, using the <see cref="IMatchedNodes.AddMatched"/> method.
    /// This object is a strategy object which implements different algorithms for matching and 
    /// saving node references, as the actual <see cref="XPathNavigator"/> implementation provides
    /// different methods for accessing the underlying source. 
    /// <para>
    /// This makes the implementation both performant and compliant with
    /// the restriction about node matching (see <linkref id="schematron" />) in the spec.
    /// </para>
    /// <para>
    ///		<seealso cref="DomMatchedNodes"/>
    ///		<seealso cref="XPathMatchedNodes"/>
    ///		<seealso cref="GenericMatchedNodes"/>
    /// </para>
    ///	As most of the other evaluation methods, it repositions the 
    ///	<see cref="EvaluationContextBase.Source"/> navigator on the root node.
    /// </para>
    /// </remarks>
    /// <param name="rule">The <see cref="Rule"/> to evaluate.</param>
    /// <param name="output">Contains the builder to accumulate messages in.</param>
    /// <returns>A boolean indicating if a new message was added.</returns>
    /// <exception cref="InvalidOperationException">
    /// The rule to evaluate is abstract (see <see cref="Rule.IsAbstract"/>).
    /// </exception>
    bool Evaluate(Rule rule, StringBuilder output)
    {
        if (rule.IsAbstract)
            throw new InvalidOperationException("The Rule is abstract, so it can't be evaluated.");

        bool failed = false;
        var sb = new StringBuilder();
        Source.MoveToRoot();
        XPathNodeIterator nodes = Source.Select(rule.CompiledExpression);
        var evaluables = new ArrayList(nodes.Count);

        // The navigator doesn't contain line info
        while (nodes.MoveNext())
        {
            if (!Matched.IsMatched(nodes.Current))
            {
                // Add the navigator to the list to be evaluated and to 
                // the list of pattern-level nodes matched so far.
                XPathNavigator curr = nodes.Current.Clone();
                evaluables.Add(curr);
                Matched.AddMatched(curr);
            }
        }

        foreach (Assert asr in rule.Asserts)
        {
            foreach (XPathNavigator node in evaluables)
            {
                if (EvaluateAssert(asr, node.Clone(), sb)) failed = true;
            }
        }

        foreach (Report rpt in rule.Reports)
        {
            foreach (XPathNavigator node in evaluables)
            {
                if (EvaluateReport(rpt, node.Clone(), sb)) failed = true;
            }
        }

        if (failed)
        {
            Formatter.Format(rule, Source, sb);
            output.Append(sb.ToString());
        }

        return failed;
    }

    /// <summary>
    /// Performs the evaluation of the <see cref="Assert"/>.
    /// </summary>
    /// <remarks>
    /// This is where the actual assert expression is evaluated. If
    /// the <see cref="EvaluableExpression.Expression"/> returns false, 
    /// a formated message is generated from the <see cref="Test.Message"/> property.
    /// </remarks>
    /// <param name="assert">The <see cref="Assert"/> to evaluate.</param>
    /// <param name="context">The context node for the execution.</param>
    /// <param name="output">Contains the builder to accumulate messages in.</param>
    /// <returns>A boolean indicating if a new message was added.</returns>
    bool EvaluateAssert(Assert assert, XPathNavigator context, StringBuilder output)
    {
        object eval = context.Evaluate(assert.CompiledExpression);
        bool result = true;

        if (assert.ReturnType == XPathResultType.Boolean)
        {
            result = (bool)eval;
        }
        else if (assert.ReturnType == XPathResultType.NodeSet &&
            ((XPathNodeIterator)eval).Count == 0)
        {
            result = false;
        }

        if (!result) Formatter.Format(assert, context, output);
        return !result;
    }

    /// <summary>
    /// Performs the evaluation of the <see cref="Report"/>.
    /// </summary>
    /// <remarks>
    /// This is where the actual report expression is evaluated. If
    /// the <see cref="EvaluableExpression.Expression"/> returns true, 
    /// a formated message is generated from the <see cref="Test.Message"/> property.
    /// </remarks>
    /// <param name="report">The <see cref="Report"/> to evaluate.</param>
    /// <param name="context">The context node for the execution.</param>
    /// <param name="output">Contains the builder to accumulate messages in.</param>
    /// <returns>A boolean indicating if a new message was added.</returns>
    bool EvaluateReport(Report report, XPathNavigator context, StringBuilder output)
    {
        object eval = context.Evaluate(report.CompiledExpression);
        bool result = false;

        if (report.ReturnType == XPathResultType.Boolean)
        {
            result = (bool)eval;
        }
        else if (report.ReturnType == XPathResultType.NodeSet &&
            ((XPathNodeIterator)eval).Count != 0)
        {
            result = true;
        }

        if (result) Formatter.Format(report, context, output);
        return result;
    }
}

