using System;

namespace Qmmands.Text.Default;

/// <summary>
///     Represents the reason why the argument parsing failed.
/// </summary>
[Obsolete(ClassicArgumentParser.ObsoletionReason)]
public enum ClassicArgumentParserFailure
{
    /// <summary>
    ///     The <see cref="ClassicArgumentParser"/> was not able to find a matching closing quotation mark.
    /// </summary>
    UnclosedQuotationMark,

    /// <summary>
    ///     The <see cref="ClassicArgumentParser"/> encountered an unexpected quotation mark.
    /// </summary>
    UnexpectedQuotationMark,

    /// <summary>
    ///     The <see cref="ClassicArgumentParser"/> was unable to parse some arguments as there was no whitespace between them.
    /// </summary>
    NoWhitespaceBetweenArguments,

    /// <summary>
    ///     The <see cref="ClassicArgumentParser"/> found too few arguments.
    /// </summary>
    TooFewArguments,

    /// <summary>
    ///     The <see cref="ClassicArgumentParser"/> found too many arguments.
    /// </summary>
    TooManyArguments
}