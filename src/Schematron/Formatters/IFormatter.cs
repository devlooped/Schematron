using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Schematron.Formatters;

/// <summary>
/// Interface for custom formatters, which are used to generate
/// output from schema validation.
/// </summary>
/// <remarks>
/// Provides formatting methods for Schematron specific elements and 
/// for <see cref="XmlSchema"/> validation through the <see cref="XmlValidatingReader"/>.
/// Custom formatters implement methods to provide message formatting.
/// <p>
/// Am abstract base implementation is provided in <see cref="FormatterBase"/> to ease
/// the extending process.
/// </p>
/// </remarks>
public interface IFormatter
{
    /// <summary>
    /// Provides formatting of both <see cref="Assert"/> and <see cref="Report"/> elements.
    /// </summary>
    /// <remarks>
    /// Implementations may use the received context to add details to the output message.
    /// The <paramref name="output" /> contains messages accumulated so far in the validation process.
    /// </remarks>
    /// <param name="context">The current navigator node where the test failed.</param>
    /// <param name="output">The message to output.</param>
    /// <param name="source">The <see cref="Assert"/> or <see cref="Report"/> element 
    /// which failed.</param>
    void Format(Test source, XPathNavigator context, StringBuilder output);

    /// <summary>
    /// Provides formatting for a <see cref="Rule"/> element.
    /// </summary>
    /// <remarks>
    /// Implementations may use the received context to add details to the output message.
    /// The <paramref name="output" /> contains messages accumulated so far in the validation process.
    /// </remarks>
    /// <param name="context">The navigator where the inner <see cref="Assert"/> or 
    /// <see cref="Report"/> elements failed.</param>
    /// <param name="output">The message to output.</param>
    /// <param name="source">The <see cref="Rule"/> element containing failed 
    /// <see cref="Assert"/> or <see cref="Report"/> elements.</param>
    void Format(Rule source, XPathNavigator context, StringBuilder output);

    /// <summary>
    /// Provides formatting for a <see cref="Pattern"/> element.
    /// </summary>
    /// <remarks>
    /// Implementations may use the received context to add details to the output message.
    /// The <paramref name="output" /> contains messages accumulated so far in the validation process.
    /// </remarks>
    /// <param name="context">The source document navigator where evaluation took place.</param>
    /// <param name="output">The message to output.</param>
    /// <param name="source">The <see cref="Pattern"/> element containing failed <see cref="Rule"/> elements.</param>
    void Format(Pattern source, XPathNavigator context, StringBuilder output);

    /// <summary>
    /// Provides formatting for a <see cref="Phase"/> element.
    /// </summary>
    /// <remarks>
    /// Implementations may use the received context to add details to the output message.
    /// The <paramref name="output" /> contains messages accumulated so far in the validation process.
    /// </remarks>
    /// <param name="context">The source document navigator where evaluation took place.</param>
    /// <param name="output">The message to output.</param>
    /// <param name="source">The <see cref="Phase"/> being evaluated.</param>
    void Format(Phase source, XPathNavigator context, StringBuilder output);

    /// <summary>
    /// Provides formatting for a <see cref="Schema"/> element.
    /// </summary>
    /// <remarks>
    /// Implementations may use the received context to add details to the output message.
    /// The <paramref name="output" /> contains messages accumulated so far in the validation process.
    /// </remarks>
    /// <param name="context">The source document navigator where evaluation took place.</param>
    /// <param name="output">The message to output.</param>
    /// <param name="source">The <see cref="Schema"/> being evaluated.</param>
    void Format(Schema source, XPathNavigator context, StringBuilder output);

    /// <summary>
    /// Provides formatting for a <see cref="XmlSchema"/> element being validated 
    /// through a <see cref="XmlValidatingReader"/>.
    /// </summary>
    /// <remarks>
    /// Usually will output schema-level formatting for XmlSchema validation. Recall that
    /// multiple schemas may have been configured with the reader and validated simultaneously.
    /// The <paramref name="output" /> contains messages accumulated so far in the validation process.
    /// </remarks>
    /// <param name="output">The message to output.</param>
    /// <param name="reader">The reader in use to validate the schema.</param>
    void Format(XmlReader reader, StringBuilder output);

    /// <summary>
    /// Formats the output of XmlSchema validation.
    /// </summary>
    /// <remarks>
    /// The <paramref name="output" /> contains messages accumulated so far in the validation process.
    /// </remarks>
    /// <param name="output">The message to output.</param>
    /// <param name="source">The argument received by the <see cref="ValidationEventHandler"/> handler
    /// during XmlSchema validation.</param>
    void Format(ValidationEventArgs source, StringBuilder output);

    /// <summary>
    /// Enclosing message for all schemas being validated.
    /// </summary>
    /// <remarks>
    /// Usually will add any enclosing message to the results of the global Xml validation.
    /// The <paramref name="output" /> contains messages accumulated so far in the validation process.
    /// </remarks>
    /// <param name="output">The message to output.</param>
    /// <param name="schemas">The collection of schemas in use for validation.</param>
    void Format(XmlSchemaSet schemas, StringBuilder output);

    /// <summary>
    /// Enclosing message for all schemas being validated.
    /// </summary>
    /// <remarks>
    /// Usually will add any enclosing message to the results of the global Schematron validation.
    /// The <paramref name="output" /> contains messages accumulated so far in the validation process.
    /// </remarks>
    /// <param name="output">The message to output.</param>
    /// <param name="schemas">The collection of schemas in use for validation.</param>
    void Format(SchemaCollection schemas, StringBuilder output);

    /// <summary>
    /// Formats the whole message built so far.
    /// </summary>
    /// <remarks>
    /// Usually will perform any last-minute formatting of the whole message before
    /// being returned by the calling application. For example, the <see cref="XmlFormatter"/> 
    /// uses this method to enclose the whole message in an &lt;output&gt; element.
    /// The <paramref name="output" /> contains messages accumulated so far in the validation process.
    /// </remarks>
    /// <param name="output">The message to output.</param>
    void Format(StringBuilder output);
}

