namespace Gehtsoft.FourCDesigner.Dao.Configuration;

/// <summary>
/// Configuration interface for hash provider.
/// </summary>
public interface IHashProviderConfiguration
{
    /// <summary>
    /// Gets the salt value used for hash generation.
    /// </summary>
    string Salt { get; }
}
