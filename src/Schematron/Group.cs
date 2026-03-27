namespace Schematron;

/// <summary>
/// A <c>&lt;group&gt;</c> element (ISO Schematron 2025).
/// Similar to <see cref="Pattern"/>, but each rule within the group evaluates nodes
/// independently — a node matched by one rule is not excluded from subsequent rules.
/// </summary>
public class Group : Pattern
{
    internal protected Group(string name, string id) : base(name, id) { }
    internal protected Group(string name) : base(name) { }
}
