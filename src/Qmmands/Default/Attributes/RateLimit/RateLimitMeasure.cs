namespace Qmmands;

/// <summary>
///     Represents the time measure for the window value when instantiating <see cref="RateLimitAttribute"/>.
/// </summary>
public enum RateLimitMeasure
{
    /// <summary>
    ///     The value is in milliseconds.
    /// </summary>
    Milliseconds,

    /// <summary>
    ///     The value is in seconds.
    /// </summary>
    Seconds,

    /// <summary>
    ///     The value is in minutes.
    /// </summary>
    Minutes,

    /// <summary>
    ///     The value is in hours.
    /// </summary>
    Hours,

    /// <summary>
    ///     The value is in days.
    /// </summary>
    Days
}
