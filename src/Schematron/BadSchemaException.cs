using System.Runtime.Serialization;

namespace Schematron;

/// <summary>
/// Represents the an error in the Schematron schema.
/// </summary>
/// <author ref="kzu" />
/// <progress amount="100" />
[Serializable()]
public class BadSchemaException : ApplicationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BadSchemaException"/> class.
    /// </summary>
    public BadSchemaException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadSchemaException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public BadSchemaException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadSchemaException"/> class.
    /// </summary>
    /// <param name="info">Info</param>
    /// <param name="context">Context</param>
    protected BadSchemaException(SerializationInfo info, StreamingContext context) :
        base(info, context)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadSchemaException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public BadSchemaException(string message, Exception innerException) :
        base(message, innerException)
    {
    }
}

