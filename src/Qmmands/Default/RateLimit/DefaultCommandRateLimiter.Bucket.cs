using System;

namespace Qmmands;

public partial class DefaultCommandRateLimiter
{
    /// <summary>
    ///     Represents a rate-limit bucket,
    ///     i.e. the current amount of uses, time window, etc.
    /// </summary>
    public class Bucket
    {
        /// <summary>
        ///     Gets the rate-limit information of this bucket.
        /// </summary>
        public RateLimitAttribute RateLimit { get; }

        /// <summary>
        ///     Gets the remaining uses of this bucket.
        /// </summary>
        public int Uses { get; protected set; }

        /// <summary>
        ///     Gets the current time window of this bucket.
        /// </summary>
        public DateTimeOffset Window { get; protected set; }

        /// <summary>
        ///     Gets the time of last call of this bucket.
        /// </summary>
        public DateTimeOffset LastCall { get; protected set; }

        /// <summary>
        ///     Instantiates a new <see cref="Bucket"/> with the specified rate-limit information.
        /// </summary>
        /// <param name="rateLimit"></param>
        public Bucket(RateLimitAttribute rateLimit)
        {
            RateLimit = rateLimit;
            Uses = rateLimit.Uses;
        }

        /// <summary>
        ///     Checks whether this bucket is rate-limited and returns the time after
        ///     which the execution can be retried.
        /// </summary>
        /// <param name="retryAfter"> The time to retry the execution after. </param>
        /// <returns>
        ///     <see langword="true"/> if this bucket is rate-limited.
        /// </returns>
        public virtual bool IsRateLimited(out TimeSpan retryAfter)
        {
            lock (this)
            {
                var now = DateTimeOffset.UtcNow;
                LastCall = now;

                if (Uses == RateLimit.Uses)
                    Window = now;

                if (now > Window + RateLimit.Window)
                {
                    Uses = RateLimit.Uses;
                    Window = now;
                }

                if (Uses == 0)
                {
                    retryAfter = RateLimit.Window - (now - Window);
                    return true;
                }

                retryAfter = default;
                return false;
            }
        }

        /// <summary>
        ///     Decrements the remaining uses of this bucket.
        /// </summary>
        public virtual void Decrement()
        {
            lock (this)
            {
                var now = DateTimeOffset.UtcNow;
                Uses--;

                if (Uses == 0)
                    Window = now;
            }
        }

        /// <summary>
        ///     Resets this bucket.
        /// </summary>
        public virtual void Reset()
        {
            lock (this)
            {
                Uses = RateLimit.Uses;
                LastCall = default;
                Window = default;
            }
        }
    }
}
