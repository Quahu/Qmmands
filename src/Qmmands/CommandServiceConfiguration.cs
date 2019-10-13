using System;
using System.Collections.Generic;
using Qmmands.Delegates;

namespace Qmmands
{
    /// <summary>
    ///     Represents a configuration to use with the <see cref="CommandService"/>.
    /// </summary>
    public sealed class CommandServiceConfiguration
    {
        /// <summary>
        ///     Gets or sets the <see cref="System.StringComparison"/> used for finding <see cref="Command"/>s and <see cref="Module"/>s,
        ///     used by the default <see langword="enum"/> parsers, and comparing <see cref="NullableNouns"/>. Defaults to <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </summary>
        public StringComparison StringComparison
        {
            get => _stringComparison;
            set
            {
                if (!Enum.IsDefined(typeof(StringComparison), value))
                    throw new ArgumentOutOfRangeException(nameof(value), "Invalid string comparison.");

                _stringComparison = value;
            }
        }
        private StringComparison _stringComparison = StringComparison.OrdinalIgnoreCase;

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
        private RunMode _defaultRunMode = RunMode.Sequential;

        /// <summary>
        ///     Gets or sets the <see cref="bool"/> which determines whether the extra arguments
        ///     provided should be ignored. Defaults to <see langword="false"/>.
        /// </summary>
        public bool IgnoresExtraArguments { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="string"/> separator to use between groups and commands.
        ///     Defaults to a single whitespace character.
        /// </summary>
        public string Separator
        {
            get => _separator;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "The separator must not be null.");

                _separator = value;
            }
        }
        private string _separator = " ";

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
        private SeparatorRequirement _separatorRequirement = SeparatorRequirement.Separator;

        /// <summary>
        ///     Gets or sets the default argument parser.
        ///     If <see langword="null"/>, will default to <see cref="DefaultArgumentParser.Instance"/>.
        /// </summary>
        public IArgumentParser DefaultArgumentParser { get; set; }

        /// <summary>
        ///     Gets or sets the generator <see langword="delegate"/> to use for <see cref="Cooldown"/> bucket keys.
        ///     Defaults to <see langword="null"/>.
        /// </summary>
        public CooldownBucketKeyGeneratorDelegate CooldownBucketKeyGenerator { get; set; }

        /// <summary>
        ///     Gets or sets the quotation mark map.
        ///     If <see langword="null"/>, will default to <see cref="CommandUtilities.DefaultQuotationMarkMap"/>.
        /// </summary>
        public IReadOnlyDictionary<char, char> QuoteMap { get; set; }

        /// <summary>
        ///     Gets or sets the collection of nouns to use for <see cref="Nullable{T}"/> parsing.
        ///     If <see langword="null"/>, will default to <see cref="CommandUtilities.DefaultNullableNouns"/>.
        /// </summary>
        public IEnumerable<string> NullableNouns { get; set; }

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