using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Qommon;

namespace Qmmands.Text.Default;

public partial class DefaultArgumentParser
{
    public readonly ConditionalWeakTable<ITextCommand, ParameterInformation> CommandParameterInformation = new();

    /// <summary>
    ///     Represents parameter information relevant to the parser.
    /// </summary>
    public class ParameterInformation
    {
        /// <summary>
        ///     Gets the option parameters of the command.
        /// </summary>
        public IOptionParameter[] OptionParameters { get; }

        /// <summary>
        ///     Gets the positional parameters of the command.
        /// </summary>
        public IPositionalParameter[] PositionalParameters { get; }

        /// <summary>
        ///     Instantiates a new <see cref="ParameterInformation"/> from the given command.
        /// </summary>
        /// <param name="command"> The command this information is for. </param>
        public ParameterInformation(ITextCommand command)
        {
            OptionParameters = command.Parameters.OfType<IOptionParameter>().ToArray();
            PositionalParameters = command.Parameters.OfType<IPositionalParameter>().ToArray();
        }

        /// <summary>
        ///     Tries to get an option parameter with the given short name.
        /// </summary>
        /// <param name="shortName"> The option short name. </param>
        /// <param name="optionParameter"> The output parameter. </param>
        /// <returns>
        ///     <see langword="true"/> if the parameter was found.
        /// </returns>
        public virtual bool TryGetOptionParameter(char shortName, [MaybeNullWhen(false)] out IOptionParameter optionParameter)
        {
            var optionParameters = OptionParameters;
            for (var i = 0; i < optionParameters.Length; i++)
            {
                optionParameter = optionParameters[i];
                var shortNames = optionParameter.ShortNames;
                var shortNameCount = shortNames.Count;
                for (var j = 0; j < shortNameCount; j++)
                {
                    if (shortName == shortNames[j])
                        return true;
                }
            }

            optionParameter = null;
            return false;
        }

        /// <summary>
        ///     Tries to get an option parameter with the given long name.
        /// </summary>
        /// <param name="longName"> The option long name. </param>
        /// <param name="optionParameter"> The output parameter. </param>
        /// <returns>
        ///     <see langword="true"/> if the parameter was found.
        /// </returns>
        public virtual bool TryGetOptionParameter(ReadOnlySpan<char> longName, [MaybeNullWhen(false)] out IOptionParameter optionParameter)
        {
            var optionParameters = OptionParameters;
            for (var i = 0; i < optionParameters.Length; i++)
            {
                optionParameter = optionParameters[i];
                var longNames = optionParameter.LongNames;
                var longNameCount = longNames.Count;
                for (var j = 0; j < longNameCount; j++)
                {
                    if (longName.Equals(longNames[j], StringComparison.Ordinal))
                        return true;
                }
            }

            optionParameter = null;
            return false;
        }
    }
}
