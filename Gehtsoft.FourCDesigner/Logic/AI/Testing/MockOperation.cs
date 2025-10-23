using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Logic.AI.Testing;

/// <summary>
/// Represents a mock AI operation for testing purposes.
/// </summary>
public class MockOperation
{
    /// <summary>
    /// Gets or sets the operation type (validate or process).
    /// </summary>
    [JsonPropertyName("operation")]
    public string Operation { get; set; }

    /// <summary>
    /// Gets or sets the regular expression pattern to match the request/instructions.
    /// </summary>
    [JsonPropertyName("requestPattern")]
    public string RequestPattern { get; set; }

    /// <summary>
    /// Gets or sets the regular expression pattern to match the user data/input.
    /// </summary>
    [JsonPropertyName("userDataPattern")]
    public string UserDataPattern { get; set; }

    /// <summary>
    /// Gets or sets the response to return when patterns match.
    /// </summary>
    [JsonPropertyName("response")]
    public AIResult Response { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockOperation"/> class.
    /// </summary>
    public MockOperation()
    {
        Operation = string.Empty;
        RequestPattern = string.Empty;
        UserDataPattern = string.Empty;
        Response = new AIResult();
    }
}
