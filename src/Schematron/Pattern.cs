namespace Schematron;

/// <summary>
/// A Pattern element, containing <see cref="Rule"/> elements.
/// </summary>
/// <remarks>
/// Constructor is not public. To programatically create an instance of this
/// class use the <see cref="Phase.CreatePattern(string)"/> factory method.
/// </remarks>
public class Pattern
{
    readonly RuleCollection rules = [];
    readonly LetCollection lets = [];

    /// <summary>Gets or sets the pattern's name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the pattern's Id.</summary>
    /// <remarks>
    /// This property is important because it is used by the
    /// <see cref="Phase"/> to activate certain patterns.
    /// </remarks>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets the rules contained in this pattern.</summary>
    public RuleCollection Rules => rules;

    /// <summary>Initializes the pattern with the name specified.</summary>
    /// <param name="name">The name of the new pattern.</param>
    internal protected Pattern(string name) => Name = name;

    /// <summary>Initializes the pattern with the name and id specified.</summary>
    /// <param name="name">The name of the new pattern.</param>
    /// <param name="id">The id of the new pattern.</param>
    internal protected Pattern(string name, string id)
    {
        Name = name;
        Id = id;
    }

    /// <summary>Gets the variable bindings declared in this pattern (<c>&lt;let&gt;</c> elements).</summary>
    public LetCollection Lets => lets;

    /// <summary>Creates a new rule instance.</summary>
    /// <remarks>
    /// Inheritors should override this method to create instances
    /// of their own rule implementations.
    /// </remarks>
    public virtual Rule CreateRule() => new();

    /// <summary>Creates a new rule instance with the context specified.</summary>
    /// <remarks>
    /// Inheritors should override this method to create instances
    /// of their own rule implementations.
    /// </remarks>
    /// <param name="context">
    /// The context for the new rule. <see cref="Rule.Context"/>
    /// </param>
    public virtual Rule CreateRule(string context) => new(context);
}

