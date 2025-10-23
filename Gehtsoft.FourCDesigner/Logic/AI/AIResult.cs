using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Logic.AI;

/// <summary>
/// Represents the result of an AI operation.
/// </summary>
public class AIResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    [JsonPropertyName("successful")]
    public bool Successful { get; set; }

    /// <summary>
    /// Gets or sets the error code if the operation failed.
    /// </summary>
    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the output of the AI operation.
    /// </summary>
    [JsonPropertyName("output")]
    public string Output { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AIResult"/> class.
    /// </summary>
    public AIResult()
    {
        Successful = false;
        ErrorCode = string.Empty;
        Output = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AIResult"/> class.
    /// </summary>
    /// <param name="successful">Whether the operation was successful.</param>
    /// <param name="errorCode">The error code if the operation failed.</param>
    /// <param name="output">The output of the AI operation.</param>
    public AIResult(bool successful, string errorCode, string output)
    {
        Successful = successful;
        ErrorCode = errorCode ?? string.Empty;
        Output = output ?? string.Empty;
    }

    /// <summary>
    /// Creates a successful result with the given output.
    /// </summary>
    /// <param name="output">The output of the AI operation.</param>
    /// <returns>A successful AI result.</returns>
    public static AIResult Success(string output)
    {
        return new AIResult(true, string.Empty, output);
    }

    /// <summary>
    /// Creates a failed result with the given error code and message.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A failed AI result.</returns>
    public static AIResult Failed(string errorCode, string errorMessage)
    {
        return new AIResult(false, errorCode, errorMessage);
    }
}
