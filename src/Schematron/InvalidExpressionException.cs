using System.Runtime.Serialization;

namespace Schematron;

/// <summary>
/// The exception that is thrown when an invalid XPath expression is used.
/// </summary>
/// <author ref="kzu" />
/// <progress amount="100" />
public class InvalidExpressionException : ApplicationException
{
    /// <summary>Initializes a new instance of the exception class.</summary>
    public InvalidExpressionException() : base() { }

    /// <summary>
    /// Initializes an instance of the class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public InvalidExpressionException(string message) : base(message) { }

    /// <summary>
    /// For serialization purposes.
    /// </summary>
    /// <param name="info">Info</param>
    /// <param name="context">Context</param>
    protected InvalidExpressionException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    /// <summary>
    /// Initializes an instance of the class with a specified error message
    /// and a reference to the inner exception that is the cause for this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public InvalidExpressionException(string message, Exception innerException) : base(message, innerException) { }
}

