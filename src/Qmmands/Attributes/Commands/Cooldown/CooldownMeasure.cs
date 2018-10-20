namespace Qmmands
{
    /// <summary>
    ///     Represents a time unit to use with the <see cref="CooldownAttribute"/>.
    /// </summary>
    public enum CooldownMeasure
    {
        /// <summary>
        ///     Uses milliseconds as the time unit.
        /// </summary>
        Milliseconds,

        /// <summary>
        ///     Uses seconds as the time unit.
        /// </summary>
        Seconds,

        /// <summary>
        ///     Uses minutes as the time unit.
        /// </summary>
        Minutes,

        /// <summary>
        ///     Uses hours as the time unit.
        /// </summary>
        Hours,

        /// <summary>
        ///     Uses days as the time unit.
        /// </summary>
        Days
    }
}