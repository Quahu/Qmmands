using System;
using Qommon;

namespace Qmmands;

/// <summary>
///     Applies a rate-limit to the decorated command.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RateLimitAttribute : Attribute
{
    /// <summary>
    ///     Gets the amount of uses this rate-limit allows within the given window.
    /// </summary>
    public int Uses { get; protected set; }

    /// <summary>
    ///     Gets the window of this rate-limit,
    ///     i.e. the time after which this rate-limit is reset.
    /// </summary>
    public TimeSpan Window { get; protected set; }

    /// <summary>
    ///     Gets the bucket type of this rate-limit.
    /// </summary>
    public object BucketType { get; protected set; }

    /// <summary>
    ///     Initializes a new <see cref="RateLimitAttribute"/> with the specified rate-limit properties.
    /// </summary>
    /// <param name="uses"> The amount of uses this rate-limit allows within the given window. </param>
    /// <param name="window"> The window of this rate-limit, i.e. the time after which this rate-limit is reset. </param>
    /// <param name="measure"> The unit of time of <paramref name="window"/>. </param>
    /// <param name="bucketType"> The bucket type of this rate-limit. </param>
    public RateLimitAttribute(int uses, double window, RateLimitMeasure measure, object bucketType)
    {
        Guard.IsGreaterThan(uses, 0);
        Guard.IsGreaterThan(window, 0);
        Guard.IsDefined(measure);
        Guard.IsNotNull(bucketType);

        Uses = uses;
        Window = measure switch
        {
            RateLimitMeasure.Milliseconds => TimeSpan.FromMilliseconds(window),
            RateLimitMeasure.Seconds => TimeSpan.FromSeconds(window),
            RateLimitMeasure.Minutes => TimeSpan.FromMinutes(window),
            RateLimitMeasure.Hours => TimeSpan.FromHours(window),
            RateLimitMeasure.Days => TimeSpan.FromDays(window)
        };

        BucketType = bucketType;
    }

    /// <summary>
    ///     Instantiates a new <see cref="RateLimitAttribute"/> with default values.
    /// </summary>
    protected RateLimitAttribute()
    {
        BucketType = default!;
    }
}
