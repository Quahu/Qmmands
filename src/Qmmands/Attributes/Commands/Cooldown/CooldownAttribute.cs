﻿using System;

namespace Qmmands
{
    /// <summary>
    ///     Applies a <see cref="Cooldown"/> to the <see cref="Command"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CooldownAttribute : Attribute
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
        ///     Gets the <see langword="enum"/> bucket type to use with the <see cref="CooldownBucketKeyGeneratorDelegate"/>.
        /// </summary>
        public object BucketType { get; }

        /// <summary>
        ///     Initialises a new <see cref="CooldownAttribute"/> with the specified <see cref="Cooldown"/> properties.
        /// </summary>
        /// <param name="amount"> The amount of uses per given window. </param>
        /// <param name="per"> The bucket time window. </param>
        /// <param name="cooldownMeasure"> The unit of time of the given window. </param>
        /// <param name="bucketType"> The bucket type. Has to be an <see langword="enum"/>. </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Not a valid cooldown measure.
        /// </exception>
        public CooldownAttribute(int amount, double per, CooldownMeasure cooldownMeasure, object bucketType)
        {
            Amount = amount;

            switch (cooldownMeasure)
            {
                case CooldownMeasure.Milliseconds:
                    Per = TimeSpan.FromMilliseconds(per);
                    break;

                case CooldownMeasure.Seconds:
                    Per = TimeSpan.FromSeconds(per);
                    break;

                case CooldownMeasure.Minutes:
                    Per = TimeSpan.FromMinutes(per);
                    break;

                case CooldownMeasure.Hours:
                    Per = TimeSpan.FromHours(per);
                    break;

                case CooldownMeasure.Days:
                    Per = TimeSpan.FromDays(per);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(cooldownMeasure), "Invalid cooldown measure.");
            }

            BucketType = bucketType;
        }
    }
}
