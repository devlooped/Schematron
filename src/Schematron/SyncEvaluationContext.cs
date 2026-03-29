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
    /// <summary>
    /// Starts the evaluation process.
    /// </summary>
    /// <remarks>
    /// When the process is finished, the results are placed 
    /// in the <see cref="EvaluationContextBase.Messages"/> property.
    /// </remarks>
    public override void Start()
    {
        Reset();

        // Is there something to evaluate at all?
        if (Schema is null || Schema.Patterns.Count == 0)
            return;

        // If no phase was received, try the default phase defined for the schema.
        // If no default phase is defined, all patterns will be tested.
        if (Phase == string.Empty)
            Phase = Schema.DefaultPhase is { Length: > 0 } phase ? phase : Schematron.Phase.All;

        if (Phase != Schematron.Phase.All && Schema.Phases[Phase] == null)
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
        var failed = false;
        Source.MoveToRoot();
        var sb = new StringBuilder();

        // 2025: @when condition — if false, skip this phase entirely
        if (!string.IsNullOrEmpty(phase.When))
        {
            try
            {
                var whenExpr = Source.Compile(phase.When);
                whenExpr.SetContext(SchematronXsltContext.ForLoading(Schema.NsManager));
                var whenResult = Source.Evaluate(whenExpr);
                var whenBool = whenResult switch
                {
                    bool b => b,
                    string s => !string.IsNullOrEmpty(s),
                    _ => true
                };
                if (!whenBool) return false;
            }
            catch { /* if @when evaluation fails, proceed */ }
        }

        foreach (var pt in phase.Patterns)
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
        var failed = false;
        Source.MoveToRoot();
        var sb = new StringBuilder();

        // Reset matched nodes, as across patters, nodes can be 
        // evaluated more than once.
        Matched.Clear();
        var isGroup = pattern is Group;

        foreach (var rule in pattern.Rules)
        {
            // For groups (ISO Schematron 2025), each rule evaluates independently.
            if (isGroup) Matched.Clear();
            if (Evaluate(rule, sb, BuildLets(pattern.Lets))) failed = true;
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
    /// <param name="patternLets">A dictionary of pattern-level variable bindings (Let expressions) that will be merged with schema-level and rule-level variables during expression evaluation. Can be null if no pattern-level variables are defined.</param>
    /// <returns>A boolean indicating if a new message was added.</returns>
    /// <exception cref="InvalidOperationException">
    /// The rule to evaluate is abstract (see <see cref="Rule.IsAbstract"/>).
    /// </exception>
    bool Evaluate(Rule rule, StringBuilder output, Dictionary<string, string>? patternLets = null)
    {
        if (rule.IsAbstract)
            throw new InvalidOperationException("The Rule is abstract, so it can't be evaluated.");

        var failed = false;
        var sb = new StringBuilder();
        Source.MoveToRoot();
        var nodes = Source.Select(rule.CompiledExpression);
        var evaluables = new ArrayList(nodes.Count);

        // The navigator doesn't contain line info
        while (nodes.MoveNext())
        {
            if (!Matched.IsMatched(nodes.Current))
            {
                // Add the navigator to the list to be evaluated and to 
                // the list of pattern-level nodes matched so far.
                var curr = nodes.Current.Clone();
                evaluables.Add(curr);
                Matched.AddMatched(curr);
            }
        }

        // 2025: @visit-each — for each matched context node, select secondary nodes to test
        if (!string.IsNullOrEmpty(rule.VisitEach))
        {
            try
            {
                var visitExpr = Source.Compile(rule.VisitEach);
                visitExpr.SetContext(Schema.NsManager);
                var expanded = new ArrayList();
                foreach (XPathNavigator contextNode in evaluables)
                {
                    var visitNodes = contextNode.Select(visitExpr);
                    while (visitNodes.MoveNext())
                        expanded.Add(visitNodes.Current.Clone());
                }
                evaluables = expanded;
            }
            catch { /* fall back to original evaluables on error */ }
        }

        foreach (Assert asr in rule.Asserts)
        {
            foreach (XPathNavigator node in evaluables)
            {
                if (EvaluateAssert(asr, node.Clone(), sb, patternLets, rule.Lets)) failed = true;
            }
        }

        foreach (Report rpt in rule.Reports)
        {
            foreach (XPathNavigator node in evaluables)
            {
                if (EvaluateReport(rpt, node.Clone(), sb, patternLets, rule.Lets)) failed = true;
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
    /// <param name="patternLets">A dictionary of pattern-level variable bindings (Let expressions) that will be merged with schema-level and rule-level variables during expression evaluation. Can be null if no pattern-level variables are defined.</param>
    /// <param name="ruleLets">A collection of rule-level variable bindings (Let expressions) that will be merged with schema-level and pattern-level variables during expression evaluation. Can be null if no rule-level variables are defined.</param>
    /// <returns>A boolean indicating if a new message was added.</returns>
    bool EvaluateAssert(Assert assert, XPathNavigator context, StringBuilder output,
        Dictionary<string, string>? patternLets = null, LetCollection? ruleLets = null)
    {
        var expr = PrepareExpression(assert.CompiledExpression, context, patternLets, ruleLets, out var xsltCtx);
        var eval = context.Evaluate(expr);
        var result = true;

        if (assert.ReturnType == XPathResultType.Boolean)
        {
            result = (bool)eval;
        }
        else if (assert.ReturnType == XPathResultType.NodeSet &&
            ((XPathNodeIterator)eval).Count == 0)
        {
            result = false;
        }

        if (!result)
        {
            SchematronXsltContext.Current = xsltCtx;
            try { Formatter.Format(assert, context, output); }
            finally { SchematronXsltContext.Current = null; }
        }
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
    /// <param name="patternLets">A dictionary of pattern-level variable bindings (Let expressions) that will be merged with schema-level and rule-level variables during expression evaluation. Can be null if no pattern-level variables are defined.</param>
    /// <param name="ruleLets">A collection of rule-level variable bindings (Let expressions) that will be merged with schema-level and pattern-level variables during expression evaluation. Can be null if no rule-level variables are defined.</param>
    /// <returns>A boolean indicating if a new message was added.</returns>
    bool EvaluateReport(Report report, XPathNavigator context, StringBuilder output,
        Dictionary<string, string>? patternLets = null, LetCollection? ruleLets = null)
    {
        var expr = PrepareExpression(report.CompiledExpression, context, patternLets, ruleLets, out var xsltCtx);
        var eval = context.Evaluate(expr);
        var result = false;

        if (report.ReturnType == XPathResultType.Boolean)
        {
            result = (bool)eval;
        }
        else if (report.ReturnType == XPathResultType.NodeSet &&
            ((XPathNodeIterator)eval).Count != 0)
        {
            result = true;
        }

        if (result)
        {
            SchematronXsltContext.Current = xsltCtx;
            try { Formatter.Format(report, context, output); }
            finally { SchematronXsltContext.Current = null; }
        }
        return result;
    }

    // -------------------------------------------------------------------------
    // Helper: build a merged variable dictionary (schema → pattern → rule scope).
    // -------------------------------------------------------------------------

    Dictionary<string, string> BuildLets(LetCollection? extraLets = null)
    {
        var d = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var let in Schema.Lets)
            if (let.Value is not null) d[let.Name] = let.Value;
        if (extraLets != null)
            foreach (var let in extraLets)
                if (let.Value is not null) d[let.Name] = let.Value;
        return d;
    }

    // Prepares an XPathExpression to be evaluated with variable support.
    // When no variables are in scope this is a no-op (returns the original expression).
    XPathExpression PrepareExpression(
        XPathExpression expr,
        XPathNavigator context,
        Dictionary<string, string>? patternLets,
        LetCollection? ruleLets,
        out SchematronXsltContext? xsltCtx)
    {
        var hasVars = (Schema.Lets.Count + (patternLets?.Count ?? 0) + (ruleLets?.Count ?? 0)) > 0;
        if (!hasVars) { xsltCtx = null; return expr; }

        var vars = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var let in Schema.Lets)
            if (let.Value is not null) vars[let.Name] = let.Value;
        if (patternLets != null)
            foreach (var kv in patternLets) vars[kv.Key] = kv.Value;
        if (ruleLets != null)
            foreach (var let in ruleLets)
                if (let.Value is not null) vars[let.Name] = let.Value;

        xsltCtx = new SchematronXsltContext(vars, context, Schema.NsManager);
        var clone = expr.Clone();
        clone.SetContext(xsltCtx);
        return clone;
    }
}

