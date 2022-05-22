using System.Collections.Generic;
using System.ComponentModel;

namespace Qmmands.Text;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class TextModuleExtensions
{
    public static IEnumerable<string> EnumerateFullAliases(this ITextModule module)
    {
        var parentFullAliases = module.Parent?.EnumerateFullAliases();
        if (parentFullAliases == null)
            return module.Aliases;

        var parentFullAliasesEnumerator = parentFullAliases.GetEnumerator();
        if (!parentFullAliasesEnumerator.MoveNext())
            return module.Aliases;

        var moduleAliases = module.Aliases;
        var moduleAliasCount = moduleAliases.Count;
        if (moduleAliasCount == 0)
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

        static IEnumerable<string> YieldFullAliases(IEnumerator<string> parentEnumerator, IReadOnlyList<string> moduleAliases, int moduleAliasCount)
        {
            do
            {
                for (var j = 0; j < moduleAliasCount; j++)
                {
                    var moduleAlias = moduleAliases[j];
                    yield return string.Concat(parentEnumerator.Current, " ", moduleAlias);
                }
            }
            while (parentEnumerator.MoveNext());
        }

        return YieldFullAliases(parentFullAliasesEnumerator, moduleAliases, moduleAliasCount);
    }
}
