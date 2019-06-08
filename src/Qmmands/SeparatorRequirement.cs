namespace Qmmands
{
    /// <summary>
    ///     Defines whether groups and commands need a separator for pathing.
    /// </summary>
    public enum SeparatorRequirement
    {
        /// <summary>
        ///     Groups and commands must be separated by the specified separator.
        /// </summary>
        Separator,

        /// <summary>
        ///     Groups and commands must be separated either by the specified separator or whitespace characters.
        /// </summary>
        SeparatorOrWhitespace
    }
}