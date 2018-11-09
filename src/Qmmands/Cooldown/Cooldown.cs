using System;

namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="Command"/> cooldown.
    /// </summary>
    public sealed class Cooldown
    {
        /// <summary>
        ///     Gets the amount of times the <see cref="Command"/> can be used in the given time window.
        /// </summary>
        public int Amount { get; }

        /// <summary>
        ///     Gets the time window of this cooldown.
        /// </summary>
        public TimeSpan Per { get; }

        /// <summary>
        ///     Gets the <see langword="enum"/> bucket type to use with the <see cref="ICooldownBucketKeyGenerator"/>.
        /// </summary>
        public object BucketType { get; }

        /// <summary>
        ///     Initialises a new <see cref="Cooldown"/> with the specified properties.
        /// </summary>
        /// <param name="amount"> The starting uses amount. </param>
        /// <param name="per"> The use time window. </param>
        /// <param name="bucketType"> The <see langword="enum"/> bucket type. </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Amount and per must be positive values.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Bucket type mustn't be <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Bucket type must be an <see langword="enum"/>.
        /// </exception>
        public Cooldown(int amount, TimeSpan per, object bucketType)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be a positive integer.");

            if (per <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(per), "Per must be a positive time span.");

            if (bucketType is null)
                throw new ArgumentNullException(nameof(bucketType), "Bucket type mustn't be null.");

            if (!bucketType.GetType().IsEnum)
                throw new ArgumentException("Bucket type must be an enum.", nameof(bucketType));

            Amount = amount;
            Per = per;
            BucketType = bucketType;
        }
    }
}
