using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Logic.Email.Model;

/// <summary>
/// Represents an email attachment.
/// </summary>
public class EmailAttachment
{
    /// <summary>
    /// Gets or sets the file name of the attachment.
    /// </summary>
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MIME content type of the attachment.
    /// </summary>
    [JsonPropertyName("contentType")]
    public string ContentType { get; set; } = "application/octet-stream";

    /// <summary>
    /// Gets or sets the attachment content as base64 encoded string.
    /// </summary>
    [JsonPropertyName("contentBase64")]
    public string ContentBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAttachment"/> class.
    /// </summary>
    public EmailAttachment()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAttachment"/> class with specified parameters.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="contentType">The MIME content type.</param>
    /// <param name="content">The attachment content.</param>
    public EmailAttachment(string fileName, string contentType, byte[] content)
    {
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        ContentType = contentType ?? "application/octet-stream";
        ContentBase64 = Convert.ToBase64String(content ?? throw new ArgumentNullException(nameof(content)));
    }

    /// <summary>
    /// Gets the attachment content as byte array.
    /// </summary>
    /// <returns>The decoded content.</returns>
    public byte[] GetContent()
    {
        if (string.IsNullOrEmpty(ContentBase64))
            return Array.Empty<byte>();

        return Convert.FromBase64String(ContentBase64);
    }
}
