namespace Schematron;

/// <summary>
/// A <c>&lt;group&gt;</c> element (ISO Schematron 2025).
/// Similar to <see cref="Pattern"/>, but each rule within the group evaluates nodes
/// independently — a node matched by one rule is not excluded from subsequent rules.
/// </summary>
public class Group : Pattern
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Group"/> class with a name and identifier.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <param name="id">The unique identifier of the group.</param>
    internal protected Group(string name, string id) : base(name, id) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Group"/> class with a name.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    internal protected Group(string name) : base(name) { }
}
