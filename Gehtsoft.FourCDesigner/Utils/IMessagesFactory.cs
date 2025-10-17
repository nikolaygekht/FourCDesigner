namespace Gehtsoft.FourCDesigner.Utils;

/// <summary>
/// Factory interface for creating message providers based on language code.
/// </summary>
public interface IMessagesFactory
{
    /// <summary>
    /// Gets a message provider for the specified language code.
    /// </summary>
    /// <param name="languageCode">The language code (e.g., "en", "es", "fr").</param>
    /// <returns>A message provider for the specified language.</returns>
    /// <exception cref="ArgumentException">Thrown when the language code is not supported.</exception>
    IMessages GetMessages(string languageCode);

    /// <summary>
    /// Gets the default message provider.
    /// </summary>
    /// <returns>The default message provider.</returns>
    IMessages GetDefaultMessages();
}
