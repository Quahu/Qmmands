using System;

namespace Qmmands
{
    /// <summary>
    ///     The configuration to use with the <see cref="CommandService"/>.
    /// </summary>
    public sealed class CommandServiceConfiguration
    {
        /// <summary>
        ///     The <see cref="bool"/> which determines whether the commands should be case sensitive or not.
        /// </summary>
        public bool CaseSensitive { get; set; } = false;

        private RunMode _defaultRunMode = RunMode.Sequential;

        /// <summary>
        ///     The <see cref="RunMode"/> which determines whether the commands should run sequentially or in parallel.
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
        ///     The <see cref="bool"/> which determines whether the extra arguments provided should be ignored.
        /// </summary>
        public bool IgnoreExtraArguments { get; set; } = false;

        /// <summary>
        ///     The <see cref="string"/> separator to use between groups and commands.
        /// </summary>
        public string Separator { get; set; } = " ";

        private SeparatorRequirement _separatorRequirement = SeparatorRequirement.Separator;

        /// <summary>
        ///     The <see cref="Qmmands.SeparatorRequirement"/> for group and command pathing.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"> <see langword="value"/> must be a valid <see cref="Qmmands.SeparatorRequirement"/> value. </exception>
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
        ///     The raw argument parser. Defaults to <see cref="DefaultArgumentParser"/>.
        /// </summary>
        public IArgumentParser ArgumentParser
        {
            get => _argumentParser;
            set => _argumentParser = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        ///     Initialises a new <see cref="CommandServiceConfiguration"/>.
        /// </summary>
        public CommandServiceConfiguration()
        {

        }

        /// <summary>
        ///     Gets the default <see cref="CommandServiceConfiguration"/>.
        /// </summary>
        public static CommandServiceConfiguration Default => new CommandServiceConfiguration();
    }
}