using System;
using System.Collections.Generic;

namespace Qmmands.Text;

/// <inheritdoc/>
public interface ITextCommandMap : ICommandMap
{
    /// <summary>
    ///     Finds commands matching the raw input.
    /// </summary>
    /// <param name="input"> The raw input to search. </param>
    /// <returns>
    ///     A list of matches.
    /// </returns>
    IList<ITextCommandMatch> FindMatches(ReadOnlyMemory<char> input);

    bool ICommandMap.CanMap(Type moduleType)
    {
        return typeof(ITextModule).IsAssignableFrom(moduleType);
    }
}
