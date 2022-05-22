using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Qmmands.Text.Default;

public class CommandOverloadComparer : IComparer<ITextCommandMatch>
{
    public static readonly CommandOverloadComparer Instance = new();

    private CommandOverloadComparer()
    { }

    public int Compare(ITextCommandMatch? x, ITextCommandMatch? y)
    {
        if (x == null && y == null)
            return 0;

        if (x == null || y == null)
            return x == null ? 1 : -1;

        var pathCompare = GetPathLength(y.Path).CompareTo(GetPathLength(x.Path));
        if (pathCompare != 0)
            return pathCompare;

        var priorityCompare = y.Command.OverloadPriority.CompareTo(x.Command.OverloadPriority);
        if (priorityCompare != 0)
            return priorityCompare;

        var parametersCompare = y.Command.Parameters.Count.CompareTo(x.Command.Parameters.Count);
        return parametersCompare;
    }

    public static int GetPathLength(IEnumerable<ReadOnlyMemory<char>> path)
    {
        var pathLength = 0;
        if (path is ImmutableStack<ReadOnlyMemory<char>> stack)
        {
            var enumerator = stack.GetEnumerator();
            while (enumerator.MoveNext())
                pathLength++;
        }
        else
        {
            using (var enumerator = path.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    pathLength++;
            }
        }

        return pathLength;
    }
}
