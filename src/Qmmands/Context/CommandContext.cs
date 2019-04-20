using System;
using System.Collections.Generic;
using Qommon.Collections;

namespace Qmmands
{
    /// <summary>
    ///     The base class for custom command contexts.
    /// </summary>
    /// <remarks>
    ///     The properties this class exposes may not always be present, depending on how the <see cref="Qmmands.Command"/> was executed.
    /// </remarks>
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
                    ? (_arguments = new ReadOnlyList<object>(InternalArguments ?? Array.Empty<object>()))
                    : arguments;
            }
        }
        private ReadOnlyList<object> _arguments;

        internal object[] InternalArguments;
    }
}