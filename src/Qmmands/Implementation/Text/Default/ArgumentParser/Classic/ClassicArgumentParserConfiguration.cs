using System;
using System.Collections.Generic;

namespace Qmmands.Text.Default;

/// <summary>
///     Represents the configuration for the classic argument parser.
/// </summary>
[Obsolete(ClassicArgumentParser.ObsoletionReason)]
public class ClassicArgumentParserConfiguration
{
    /// <inheritdoc cref="DefaultArgumentParserConfiguration.QuotationMarks"/>
    public IDictionary<char, char> QuotationMarks { get; set; } = DefaultArgumentParserConfiguration.DefaultQuotationMarks;
}