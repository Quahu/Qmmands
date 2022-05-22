using System.Collections.Generic;

namespace Qmmands.Text.Default;

/// <summary>
///     Represents the configuration for the default argument parser.
/// </summary>
public class DefaultArgumentParserConfiguration
{
    /// <summary>
    ///     Gets the default quotation mark pairs.
    /// </summary>
    public static Dictionary<char, char> DefaultQuotationMarks => new(6)
    {
        // double
        ['"'] = '"',

        // single
        ['\''] = '\'',

        // left/right
        ['“'] = '”',

        // low/high
        ['„'] = '‟',

        // guillemets
        ['«'] = '»',
        ['»'] = '«'
    };

    /// <summary>
    ///     Gets or sets the quotation mark pairs that the argument parser will use for arguments.
    /// </summary>
    public IDictionary<char, char> QuotationMarks { get; set; } = DefaultQuotationMarks;
}