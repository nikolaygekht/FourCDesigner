namespace Gehtsoft.FourCDesigner.Logic.Email;

/// <summary>
/// Shared state for email sender service.
/// This class is used to share state between EmailService and EmailSenderService
/// without creating a circular dependency.
/// </summary>
public class EmailSenderState
{
    /// <summary>
    /// Gets or sets a value indicating whether the email sender is currently active.
    /// </summary>
    public bool IsSenderActive { get; set; } = false;

    /// <summary>
    /// Gets or sets the last error that occurred during sending.
    /// </summary>
    public Exception? LastError { get; set; } = null;

    /// <summary>
    /// Gets or sets the date and time of the last error.
    /// </summary>
    public DateTime? LastErrorTime { get; set; } = null;

    /// <summary>
    /// Gets or sets the date and time when the sender will resume after an error.
    /// </summary>
    public DateTime? PauseAfterErrorUntil { get; set; } = null;
}
