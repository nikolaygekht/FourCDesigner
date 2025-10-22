namespace Gehtsoft.FourCDesigner.Middleware.Throttling;

/// <summary>
/// Service for resetting throttle state by invalidating partition keys.
/// When reset is called, increments a counter that changes all partition keys.
/// </summary>
public interface IThrottleResetService
{
    /// <summary>
    /// Gets the current reset generation number.
    /// This changes each time Reset() is called.
    /// </summary>
    int Generation { get; }

    /// <summary>
    /// Resets throttling by incrementing the generation counter.
    /// This effectively invalidates all existing rate limiter partitions.
    /// </summary>
    void Reset();
}

/// <summary>
/// Default implementation of throttle reset service.
/// </summary>
public class ThrottleResetService : IThrottleResetService
{
    private int _generation = 0;

    /// <inheritdoc/>
    public int Generation => _generation;

    /// <inheritdoc/>
    public void Reset()
    {
        Interlocked.Increment(ref _generation);
    }
}
