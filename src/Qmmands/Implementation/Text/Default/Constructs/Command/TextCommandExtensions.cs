using System.Collections.Generic;
using System.ComponentModel;

namespace Qmmands.Text;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class TextCommandExtensions
{
    public static IEnumerable<string> EnumerateFullAliases(this ITextCommand command)
    {
        var parentFullAliases = command.Module.EnumerateFullAliases();
        var parentFullAliasesEnumerator = parentFullAliases.GetEnumerator();
        if (!parentFullAliasesEnumerator.MoveNext())
            return command.Aliases;

        var commandAliases = command.Aliases;
        var commandAliasCount = commandAliases.Count;
        if (commandAliasCount == 0)
        {
            static IEnumerable<string> YieldParentFullAliases(IEnumerator<string> enumerator)
            {
                do
                {
                    yield return enumerator.Current;
                }
                while (enumerator.MoveNext());
            }

            return YieldParentFullAliases(parentFullAliasesEnumerator);
        }

        static IEnumerable<string> YieldFullAliases(IEnumerator<string> parentEnumerator, IReadOnlyList<string> commandAliases, int commandAliasCount)
        {
            do
            {
                for (var j = 0; j < commandAliasCount; j++)
                {
                    var moduleAlias = commandAliases[j];
                    yield return string.Concat(parentEnumerator.Current, " ", moduleAlias);
                }
            }
            while (parentEnumerator.MoveNext());
        }

        return YieldFullAliases(parentFullAliasesEnumerator, commandAliases, commandAliasCount);
    }
}
