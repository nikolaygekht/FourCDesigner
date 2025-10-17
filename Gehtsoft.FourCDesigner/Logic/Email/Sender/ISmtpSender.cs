using Gehtsoft.FourCDesigner.Logic.Email.Model;

namespace Gehtsoft.FourCDesigner.Logic.Email.Sender;

/// <summary>
/// Interface for SMTP email sending operations.
/// </summary>
public interface ISmtpSender
{
    /// <summary>
    /// Gets a value indicating whether the SMTP connection is currently open.
    /// </summary>
    bool Connected { get; }

    /// <summary>
    /// Opens a connection to the SMTP server.
    /// </summary>
    void Open();

    /// <summary>
    /// Sends an email message through the SMTP server.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <param name="senderAddress">The sender email address.</param>
    void Send(EmailMessage message, string senderAddress);

    /// <summary>
    /// Closes the connection to the SMTP server.
    /// </summary>
    void Close();
}
