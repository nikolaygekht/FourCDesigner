namespace Gehtsoft.FourCDesigner.Utils;

/// <summary>
/// Factory for creating message providers based on language code.
/// </summary>
public class MessagesFactory : IMessagesFactory
{
    private readonly Dictionary<string, IMessages> mMessages;
    private readonly string mDefaultLanguage;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagesFactory"/> class.
    /// </summary>
    /// <param name="configuration">The configuration provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public MessagesFactory(IConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        mMessages = new Dictionary<string, IMessages>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = new MessagesEn()
        };

        mDefaultLanguage = configuration["system:defaultLanguage"] ?? "en";
    }

    /// <inheritdoc/>
    public IMessages GetMessages(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            throw new ArgumentException("Language code cannot be empty", nameof(languageCode));

        if (mMessages.TryGetValue(languageCode, out IMessages? messages))
            return messages;

        throw new ArgumentException($"Language code '{languageCode}' is not supported", nameof(languageCode));
    }

    /// <inheritdoc/>
    public IMessages GetDefaultMessages()
    {
        return GetMessages(mDefaultLanguage);
    }
}
