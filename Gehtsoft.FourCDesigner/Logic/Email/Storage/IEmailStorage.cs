using Gehtsoft.FourCDesigner.Logic.Email.Model;

namespace Gehtsoft.FourCDesigner.Logic.Email.Storage;

/// <summary>
/// Interface for email message storage operations.
/// </summary>
public interface IEmailStorage
{
    /// <summary>
    /// Checks if a message with the specified ID exists in storage.
    /// </summary>
    /// <param name="id">The message ID.</param>
    /// <returns>True if the message exists, false otherwise.</returns>
    bool DoesMessageExist(Guid id);

    /// <summary>
    /// Gets all message IDs from storage.
    /// </summary>
    /// <returns>Array of message IDs.</returns>
    Guid[] GetMessageIds();

    /// <summary>
    /// Reads a message from storage.
    /// </summary>
    /// <param name="id">The message ID.</param>
    /// <returns>The email message, or null if not found.</returns>
    EmailMessage ReadMessage(Guid id);

    /// <summary>
    /// Writes a message to storage.
    /// </summary>
    /// <param name="message">The email message to write.</param>
    void WriteMessage(EmailMessage message);

    /// <summary>
    /// Deletes a message from storage.
    /// </summary>
    /// <param name="id">The message ID.</param>
    void DeleteMessage(Guid id);

    /// <summary>
    /// Deletes all messages from storage.
    /// </summary>
    void DeleteAllMessages();

    /// <summary>
    /// Gets the count of messages in storage.
    /// </summary>
    /// <returns>The number of messages.</returns>
    int MessagesCount();

    /// <summary>
    /// Moves a failed message to the bad email folder.
    /// </summary>
    /// <param name="message">The failed email message.</param>
    void MoveToBadEmail(EmailMessage message);
}
