namespace Gehtsoft.FourCDesigner.Logic.Email.Sender;

/// <summary>
/// Exception thrown when SMTP authentication fails.
/// This exception indicates that email processing should stop to prevent account lockout.
/// </summary>
public class MailServerAuthFailureException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MailServerAuthFailureException"/> class.
    /// </summary>
    public MailServerAuthFailureException()
        : base("SMTP authentication failed. Email processing stopped to prevent account lockout.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MailServerAuthFailureException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public MailServerAuthFailureException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MailServerAuthFailureException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public MailServerAuthFailureException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
