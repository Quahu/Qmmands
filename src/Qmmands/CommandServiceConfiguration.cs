using System;
using System.Collections.Generic;

namespace Qmmands
{
    /// <summary>
    ///     Represents a configuration to use with the <see cref="CommandService"/>.
    /// </summary>
    public sealed class CommandServiceConfiguration
    {
        /// <summary>
        ///     Gets or sets the <see cref="bool"/> which determines whether the commands should
        ///     be case sensitive or not. Defaults to <see langword="false"/>.
        /// </summary>
        public bool CaseSensitive { get; set; } = false;

        private RunMode _defaultRunMode = RunMode.Sequential;

        /// <summary>
        ///     Gets or sets the <see cref="RunMode"/> which determines whether the commands should
        ///     run sequentially or in parallel. Defaults to <see cref="RunMode.Sequential"/>.
        /// </summary>
        public RunMode DefaultRunMode
        {
            get => _defaultRunMode;
            set
            {
                if (!Enum.IsDefined(typeof(RunMode), value))
                    throw new ArgumentOutOfRangeException(nameof(value), "Invalid run mode.");

                _defaultRunMode = value;
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="bool"/> which determines whether the extra arguments
        ///     provided should be ignored. Defaults to <see langword="false"/>.
        /// </summary>
        public bool IgnoreExtraArguments { get; set; } = false;

        /// <summary>
        ///     Gets or sets the <see cref="string"/> separator to use between groups and commands.
        ///     Defaults to a single whitespace character.
        /// </summary>
        public string Separator { get; set; } = " ";

        private SeparatorRequirement _separatorRequirement = SeparatorRequirement.Separator;

        /// <summary>
        ///     Gets or sets the <see cref="Qmmands.SeparatorRequirement"/> for group and command pathing.
        ///     Defaults to <see cref="Qmmands.SeparatorRequirement.Separator"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <see langword="value"/> must be a valid <see cref="Qmmands.SeparatorRequirement"/> value.
        /// </exception>
        public SeparatorRequirement SeparatorRequirement
        {
            get => _separatorRequirement;
            set
            {
                if (!Enum.IsDefined(typeof(SeparatorRequirement), value))
                    throw new ArgumentOutOfRangeException(nameof(value), "Invalid separator requirement.");

                _separatorRequirement = value;
            }
        }

        private IArgumentParser _argumentParser = new DefaultArgumentParser();

        /// <summary>
        ///     Gets or sets the raw argument parser. Defaults to <see cref="DefaultArgumentParser"/>.
        /// </summary>
        public IArgumentParser ArgumentParser
        {
            get => _argumentParser;
            set => _argumentParser = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IReadOnlyDictionary<char, char> _quoteMap = CommandUtilities.DefaultQuoteMap;

        /// <summary>
        ///     Gets or sets the quotation mark map. Defaults to <see cref="CommandUtilities.DefaultQuoteMap"/>.
        /// </summary>
        public IReadOnlyDictionary<char, char> QuoteMap
        {
            get => _quoteMap;
            set => _quoteMap = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IReadOnlyList<string> _nullableNouns = CommandUtilities.DefaultNullableNouns;

        /// <summary>
        ///     Gets or sets the collection of nouns to use for nullable value type parsing.
        ///     Defaults to <see cref="CommandUtilities.DefaultNullableNouns"/>.
        /// </summary>
        public IReadOnlyList<string> NullableNouns
        {
            get => _nullableNouns;
            set => _nullableNouns = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        ///     Initialises a new <see cref="CommandServiceConfiguration"/>.
        /// </summary>
        public CommandServiceConfiguration()
        { }

        /// <summary>
        ///     Gets the default <see cref="CommandServiceConfiguration"/>.
        ///     The equivalent of using <see langword="new"/> <see cref="CommandServiceConfiguration()"/>.
        /// </summary>
        public static CommandServiceConfiguration Default => new CommandServiceConfiguration();
    }
}