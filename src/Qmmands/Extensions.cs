using System.Collections.Immutable;

namespace Qmmands
{
    internal static class Extensions
    {
        public static ImmutableArray<T> TryMoveToImmutable<T>(this ImmutableArray<T>.Builder builder)
            => builder.Capacity == builder.Count
                ? builder.MoveToImmutable()
                : builder.ToImmutable();
    }
}
