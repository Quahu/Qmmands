using System.Collections.Generic;
using Qommon;

namespace Qmmands.Text;

/// <summary>
///     Represents the result of an argument parser.
/// </summary>
public interface IArgumentParserResult : IResult
{
    /// <summary>
    ///     Gets the parsed raw arguments.
    /// </summary>
    IReadOnlyDictionary<ITextParameter, MultiString> Arguments { get; }
}