namespace Gehtsoft.FourCDesigner.Logic.AI.Testing;

/// <summary>
/// Configuration interface for AI testing driver settings.
/// </summary>
public interface IAITestingConfiguration
{
    /// <summary>
    /// Gets the path to the JSON file containing mock responses.
    /// </summary>
    string MockFilePath { get; }
}
