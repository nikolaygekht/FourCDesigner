using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Controllers.Data;

/// <summary>
/// Response DTO for dequeued email message.
/// </summary>
public class DequeueEmailResponse
{
    /// <summary>
    /// Gets or sets the message ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recipient email addresses.
    /// </summary>
    [JsonPropertyName("to")]
    public string[] To { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the email subject.
    /// </summary>
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email body.
    /// </summary>
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the body is HTML.
    /// </summary>
    [JsonPropertyName("isHtml")]
    public bool IsHtml { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a priority message.
    /// </summary>
    [JsonPropertyName("priority")]
    public bool Priority { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the retry count.
    /// </summary>
    [JsonPropertyName("retryCount")]
    public int RetryCount { get; set; }
}
