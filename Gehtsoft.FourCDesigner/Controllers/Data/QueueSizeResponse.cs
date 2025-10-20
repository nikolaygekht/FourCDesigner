using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Controllers.Data;

/// <summary>
/// Response DTO for email queue size information.
/// </summary>
public class QueueSizeResponse
{
    /// <summary>
    /// Gets or sets the number of messages in the queue.
    /// </summary>
    [JsonPropertyName("queueSize")]
    public int QueueSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the email sender is active.
    /// </summary>
    [JsonPropertyName("isSenderActive")]
    public bool IsSenderActive { get; set; }
}
