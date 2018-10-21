namespace Qmmands
{
    /// <summary>
    ///     Represents a found <see cref="Qmmands.Module"/>, the path to it, and raw arguments.
    /// </summary>
    public sealed class ModuleMatch
    {
        /// <summary>
        ///     Gets the found <see cref="Qmmands.Module"/>.
        /// </summary>
        public Module Module { get; }

        /// <summary>
        ///     Gets the matching alias.
        /// </summary>
        public string Alias { get; }

        /// <summary>
        ///     Gets the path to the found <see cref="Qmmands.Module"/>.
        /// </summary>
        public string[] Path { get; }

        /// <summary>
        ///     Gets the raw arguments.
        /// </summary>
        public string RawArguments { get; }

        internal ModuleMatch(Module module, string alias, string[] path, string rawArguments)
        {
            Module = module;
            Alias = alias;
            Path = path;
            RawArguments = rawArguments;
        }
    }
}
