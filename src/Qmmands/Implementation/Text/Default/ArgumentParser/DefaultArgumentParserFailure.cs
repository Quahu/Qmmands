namespace Qmmands.Text.Default;

/// <summary>
///     Represents a parsing failure of <see cref="DefaultArgumentParser.ParseAsync"/>.
/// </summary>
public enum DefaultArgumentParserFailure
{
    /// <summary>
    ///     The parser was not able to find a matching closing quotation mark.
    /// </summary>
    UnclosedQuotationMark,

    /// <summary>
    ///     The parser could not find a matching option name.
    /// </summary>
    UnknownOptionName,

    /// <summary>
    ///     The parser encountered a duplicate option name for a parameter that does not accept multiple values.
    /// </summary>
    DuplicateOptionName,

    /// <summary>
    ///     The parser encountered another option matching the same group it is mutually exclusive with.
    /// </summary>
    MutuallyExclusiveOption,

    /// <summary>
    ///     The parser encountered another option name when it expected a value.
    /// </summary>
    ExpectedOptionValue,

    /// <summary>
    ///     The parser found too many positional parameter values.
    /// </summary>
    TooManyValues
}
