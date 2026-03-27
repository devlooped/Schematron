namespace Schematron;

/// <summary>
/// A Pattern element, containing <see cref="Rule"/> elements.
/// </summary>
/// <remarks>
/// Constructor is not public. To programatically create an instance of this
/// class use the <see cref="Phase.CreatePattern(string)"/> factory method.
/// </remarks>
/// <author ref="kzu" />
/// <progress amount="100" />
public class Pattern
{
    string _name = String.Empty;
    string _id = String.Empty;
    RuleCollection _rules = new RuleCollection();

    #region Properties
    /// <summary>Gets or sets the pattern's name.</summary>
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    /// <summary>Gets or sets the pattern's Id.</summary>
    /// <remarks>
    /// This property is important because it is used by the
    /// <see cref="Phase"/> to activate certain patterns.
    /// </remarks>
    public string Id
    {
        get { return _id; }
        set { _id = value; }
    }

    /// <summary>Gets the rules contained in this pattern.</summary>
    public RuleCollection Rules
    {
        get { return _rules; }
    }
    #endregion

    /// <summary>Initializes the pattern with the name specified.</summary>
    /// <param name="name">The name of the new pattern.</param>
    internal protected Pattern(string name)
    {
        _name = name;
    }

    /// <summary>Initializes the pattern with the name and id specified.</summary>
    /// <param name="name">The name of the new pattern.</param>
    /// <param name="id">The id of the new pattern.</param>
    internal protected Pattern(string name, string id)
    {
        _name = name;
        _id = id;
    }

    #region Overridable Factory Methods
    /// <summary>Creates a new rule instance.</summary>
    /// <remarks>
    /// Inheritors should override this method to create instances
    /// of their own rule implementations.
    /// </remarks>
    public virtual Rule CreateRule()
    {
        return new Rule();
    }

    /// <summary>Creates a new rule instance with the context specified.</summary>
    /// <remarks>
    /// Inheritors should override this method to create instances
    /// of their own rule implementations.
    /// </remarks>
    /// <param name="context">
    /// The context for the new rule. <see cref="Rule.Context"/>
    /// </param>
    public virtual Rule CreateRule(string context)
    {
        return new Rule(context);
    }
    #endregion
}

