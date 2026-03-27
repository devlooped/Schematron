namespace Schematron;

/// <summary>
/// A Report element.
/// </summary>
/// <remarks>
/// As stated in the <link ref="schematron" />, this is the other
/// lowest element (aside from <see cref="Assert"/>) in a Schematron schema.
/// This element contains the expression
/// to execute in the context of its parent <see cref="Rule"/>.
/// <para>If the results of the execution of the expression are <c>true</c>, the
/// report succeeds and the correponding message will be displayed.</para>
/// <para>
/// Constructor is not public. To programatically create an instance of this
/// class use the <see cref="Rule.CreateAssert"/> factory method.
/// </para>
/// </remarks>
/// <author ref="kzu" />
/// <progress amount="100" />
public class Report : Test
{
    /// <summary>Initializes a new instance of the class with the parameters specified.</summary>
    /// <param name="test">The XPath expression to test.</param>
    /// <param name="message">The message to return.</param>
    internal protected Report(string test, string message) : base(test, message)
    {
    }
}

