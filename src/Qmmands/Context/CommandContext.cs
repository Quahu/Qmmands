using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Qmmands
{
    /// <summary>
    ///     The interface for custom command contexts.
    /// </summary>
    public abstract class CommandContext
    {
        /// <summary>
        ///     Gets the currently executed <see cref="Qmmands.Command"/>.
        /// </summary>
        public Command Command { get; internal set; }

        /// <summary>
        ///     Gets the alias used.
        ///     <see langword="null"/> if the <see cref="Qmmands.Command"/> was invoked without searching.
        /// </summary>
        public string Alias { get; internal set; }

        /// <summary>
        ///     Gets the alias path used.
        ///     <see langword="null"/> if the <see cref="Qmmands.Command"/> was invoked without searching.
        /// </summary>
        public IReadOnlyList<string> Path { get; internal set; }

        /// <summary>
        ///     Gets the raw arguments.
        ///     <see langword="null"/> if the <see cref="Qmmands.Command"/> was invoked with already parsed arguments.
        /// </summary>
        public string RawArguments { get; internal set; }

        /// <summary>
        ///     Gets the parsed arguments.
        /// </summary>
        public IReadOnlyList<object> Arguments
        {
            get
            {
                var arguments = _arguments;
                return arguments == null
                    ? (_arguments = new ReadOnlyCollection<object>(InternalArguments))
                    : arguments;
            }
        }
        private ReadOnlyCollection<object> _arguments;

        internal object[] InternalArguments { get; set; }
    }
}