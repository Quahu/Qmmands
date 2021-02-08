namespace Qmmands
{
    /// <summary>
    ///     Defines the separator requirement for pathing of groups and commands.
    /// </summary>
    public enum SeparatorRequirement
    {
        /// <summary>
        ///     Groups and commands must be separated by <see cref="CommandService.Separator"/>.
        /// </summary>
        Separator,

        /// <summary>
        ///     Groups and commands must be separated either by <see cref="CommandService.Separator"/> or whitespace characters.
        /// </summary>
        SeparatorOrWhitespace
    }
}