namespace Schematron;

/// <summary>
/// A Phase element, containing the active <see cref="Pattern"/> elements.
/// </summary>
/// <remarks>
/// The <link ref="schematron" /> allows a certaing degree of workflow
/// through the use of phases. A document can have several states, and
/// therefore different sets of rules should be checked against it.
/// <para>
/// This element allows execution of a set of 'active' patterns.
/// </para>
/// <para>
/// Constructor is not public. To programatically create an instance of this
/// class use the <see cref="Schema.CreatePhase(string)"/> factory method.
/// </para>
/// </remarks>
/// <author ref="kzu" />
/// <progress amount="100" />
public class Phase
{
    string _id = String.Empty;
    PatternCollection _patterns = new PatternCollection();

    /// <summary>
    /// The identifier to check for All phases.
    /// </summary>
    /// <remarks>Causes all the patterns in a schema to be checked,
    /// irrespective of the phases where they are activated.</remarks>
    public const string All = "#ALL";

    /// <summary>Initializes a new instance of the class with the specified Id.</summary>
    /// <param name="id">The Id of the new phase.</param>
    internal protected Phase(string id)
    {
        Id = id;
    }

    /// <summary>Initializes a new instance of the class.</summary>
    internal protected Phase()
    {
    }

    #region Properties
    /// <summary>Gets or sets the phase identifier.</summary>
    public string Id
    {
        get { return _id; }
        set { _id = value; }
    }

    /// <summary>Gets the collection of child <see cref="Pattern"/> elements.</summary>
    public PatternCollection Patterns
    {
        get { return _patterns; }
    }
    #endregion

    #region Overridable Factory Methods
    /// <summary>Creates a new pattern instance.</summary>
    /// <remarks>
    /// Inheritors should override this method to create instances
    /// of their own pattern implementations.
    /// </remarks>
    /// <param name="name">The name of the new pattern.</param>
    /// <param name="id">The unique identifier of the new pattern.</param>
    public virtual Pattern CreatePattern(string name, string id)
    {
        return new Pattern(name, id);
    }

    /// <summary>Creates a new pattern instance.</summary>
    /// <remarks>
    /// This method calls the overloaded version passing a default
    /// <see cref="String.Empty"/> value for the pattern's id.
    /// Inheritors can override this method if they want to provide
    /// a different default value.
    /// </remarks>
    /// <param name="name">The name of the new pattern.</param>
    public virtual Pattern CreatePattern(string name)
    {
        return CreatePattern(name, String.Empty);
    }
    #endregion
}

