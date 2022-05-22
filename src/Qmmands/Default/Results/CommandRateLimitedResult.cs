using System;
using System.Collections.Generic;
using System.Linq;

namespace Qmmands;

/// <summary>
///     Represents a command being rate-limited.
/// </summary>
public class CommandRateLimitedResult : FailedResult
{
    /// <summary>
    ///     Gets the reason of this failed result.
    /// </summary>
    public override string FailureReason
    {
        get
        {
            if (RateLimits.Count == 1)
            {
                var (rateLimit, retryAfter) = RateLimits.First();
                return $"Command is on a '{rateLimit.BucketType}' cooldown. Retry after {retryAfter}.";
            }

            var rateLimitsString = string.Join(", ", RateLimits.Select(kvp =>
            {
                var (rateLimit, retryAfter) = kvp;
                return $"'{rateLimit.BucketType}' - retry after {retryAfter}";
            }));

            return $"Command is on multiple cooldowns: {rateLimitsString}.";
        }
    }

    /// <summary>
    ///     Gets the rate-limits and <see cref="TimeSpan"/>s after which the execution can be retried.
    /// </summary>
    public IReadOnlyDictionary<RateLimitAttribute, TimeSpan> RateLimits { get; }

    public CommandRateLimitedResult(IReadOnlyDictionary<RateLimitAttribute, TimeSpan> rateLimits)
    {
        RateLimits = rateLimits;
    }
}
