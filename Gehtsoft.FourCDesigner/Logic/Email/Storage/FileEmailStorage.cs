using System.Text.Json;
using Gehtsoft.FourCDesigner.Logic.Email.Configuration;
using Gehtsoft.FourCDesigner.Logic.Email.Model;

namespace Gehtsoft.FourCDesigner.Logic.Email.Storage;

/// <summary>
/// File-based implementation of email message storage using JSON serialization.
/// </summary>
public class FileEmailStorage : IEmailStorage
{
    private readonly string mQueueFolder;
    private readonly string mBadEmailFolder;
    private readonly ILogger<FileEmailStorage> mLogger;
    private readonly object mLock = new object();

    private const string FILE_EXTENSION = "json";

    /// <summary>
    /// Initializes a new instance of the <see cref="FileEmailStorage"/> class.
    /// </summary>
    /// <param name="configuration">The email configuration.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public FileEmailStorage(IEmailConfiguration configuration, ILogger<FileEmailStorage> logger)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        mQueueFolder = configuration.QueueFolder;
        mBadEmailFolder = configuration.BadEmailFolder;

        EnsureDirectoryExists(mQueueFolder);
        EnsureDirectoryExists(mBadEmailFolder);
    }

    /// <summary>
    /// Ensures that a directory exists, creating it if necessary.
    /// </summary>
    /// <param name="path">The directory path.</param>
    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            mLogger.LogInformation("Email: Creating directory {Path}", path);
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex, "Email: Failed to create directory {Path}", path);
                throw;
            }
        }
    }

    /// <summary>
    /// Gets the file path for a message in the queue folder.
    /// </summary>
    /// <param name="id">The message ID.</param>
    /// <returns>The full file path.</returns>
    private string GetQueueFilePath(Guid id) => Path.Combine(mQueueFolder, $"{id}.{FILE_EXTENSION}");

    /// <summary>
    /// Gets the file path for a message in the bad email folder.
    /// </summary>
    /// <param name="id">The message ID.</param>
    /// <returns>The full file path.</returns>
    private string GetBadEmailFilePath(Guid id) => Path.Combine(mBadEmailFolder, $"{id}.{FILE_EXTENSION}");

    /// <inheritdoc/>
    public bool DoesMessageExist(Guid id)
    {
        lock (mLock)
        {
            return File.Exists(GetQueueFilePath(id));
        }
    }

    /// <inheritdoc/>
    public Guid[] GetMessageIds()
    {
        lock (mLock)
        {
            try
            {
                string[] files = Directory.GetFiles(mQueueFolder, $"*.{FILE_EXTENSION}");
                List<Guid> ids = new List<Guid>();

                foreach (string file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    if (Guid.TryParse(fileName, out Guid id))
                        ids.Add(id);
                    else
                        mLogger.LogWarning("Email: Invalid message file name: {FileName}", fileName);
                }

                return ids.ToArray();
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex, "Email: Failed to get message IDs from {Folder}", mQueueFolder);
                return Array.Empty<Guid>();
            }
        }
    }

    /// <inheritdoc/>
    public EmailMessage? ReadMessage(Guid id)
    {
        lock (mLock)
        {
            string filePath = GetQueueFilePath(id);

            try
            {
                if (!File.Exists(filePath))
                {
                    mLogger.LogWarning("Email: Message file not found: {Id}", id);
                    return null;
                }

                string json = File.ReadAllText(filePath);
                EmailMessage? message = JsonSerializer.Deserialize<EmailMessage>(json);

                return message;
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex, "Email: Failed to read message {Id}", id);
                return null;
            }
        }
    }

    /// <inheritdoc/>
    public void WriteMessage(EmailMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        lock (mLock)
        {
            string filePath = GetQueueFilePath(message.Id);

            try
            {
                string json = JsonSerializer.Serialize(message, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(filePath, json);
                mLogger.LogDebug("Email: Message {Id} written to storage", message.Id);
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex, "Email: Failed to write message {Id}", message.Id);
                throw;
            }
        }
    }

    /// <inheritdoc/>
    public void DeleteMessage(Guid id)
    {
        lock (mLock)
        {
            string filePath = GetQueueFilePath(id);

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    mLogger.LogDebug("Email: Message {Id} deleted from storage", id);
                }
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex, "Email: Failed to delete message {Id}", id);
                throw;
            }
        }
    }

    /// <inheritdoc/>
    public void DeleteAllMessages()
    {
        lock (mLock)
        {
            try
            {
                string[] files = Directory.GetFiles(mQueueFolder, $"*.{FILE_EXTENSION}");
                foreach (string file in files)
                {
                    File.Delete(file);
                }

                mLogger.LogInformation("Email: All messages deleted from storage ({Count} files)", files.Length);
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex, "Email: Failed to delete all messages");
                throw;
            }
        }
    }

    /// <inheritdoc/>
    public int MessagesCount()
    {
        lock (mLock)
        {
            try
            {
                return Directory.GetFiles(mQueueFolder, $"*.{FILE_EXTENSION}").Length;
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex, "Email: Failed to count messages");
                return 0;
            }
        }
    }

    /// <inheritdoc/>
    public void MoveToBadEmail(EmailMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        lock (mLock)
        {
            string sourceFile = GetQueueFilePath(message.Id);
            string destFile = GetBadEmailFilePath(message.Id);

            try
            {
                if (File.Exists(sourceFile))
                {
                    // Write message with error info to bad email folder
                    string json = JsonSerializer.Serialize(message, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    File.WriteAllText(destFile, json);

                    // Delete from queue
                    File.Delete(sourceFile);

                    mLogger.LogWarning("Email: Message {Id} moved to bad email folder. Error: {Error}",
                        message.Id, message.LastError);
                }
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex, "Email: Failed to move message {Id} to bad email folder", message.Id);
            }
        }
    }
}
