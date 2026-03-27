namespace Schematron;

/// <summary>
/// Represents an assert element of the Schematron schema.
/// </summary>
/// <remarks>
/// As stated in the <link ref="schematron" />, this is
/// the lowest element in a Schematron schema. This element contains the expression
/// to execute in the context of its parent <see cref="Rule"/>.
/// <para>If the results of the execution of the expression are <c>False</c>, the
/// assert fails and the correponding message will be displayed.</para>
/// <para>
/// Constructor is not public. To programatically create an instance of this
/// class use the <see cref="Rule.CreateAssert"/> factory method.
/// </para>
/// </remarks>
/// <author ref="kzu" />
/// <progress amount="100" />
public class Assert : Test
{
    /// <summary>Constructs a new Assert object.</summary>
    /// <param name="test">XPath expression to test.</param>
    /// <param name="message">Message to display if the assert fails.</param>
    internal protected Assert(string test, string message) : base(test, message)
    {
    }
}

