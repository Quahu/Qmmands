using System.Collections.Generic;

namespace Qmmands.Text;

/// <inheritdoc cref="IOptionParameter"/>
public class OptionParameter : TextParameter, IOptionParameter
{
    /// <inheritdoc/>
    public IReadOnlyList<char> ShortNames { get; }

    /// <inheritdoc/>
    public IReadOnlyList<string> LongNames { get; }

    /// <inheritdoc/>
    public bool IsGreedy { get; }

    /// <inheritdoc/>
    public object? Group { get; }

    public OptionParameter(ITextCommand command, IOptionParameterBuilder builder)
        : base(command, builder)
    {
        var builderShortNames = builder.ShortNames;
        var builderShortNameCount = builderShortNames.Count;
        var shortNames = new char[builderShortNameCount];
        for (var i = 0; i < builderShortNameCount; i++)
        {
            var shortName = builderShortNames[i];
            shortNames[i] = shortName;
        }

        ShortNames = shortNames;

        var builderLongNames = builder.LongNames;
        var builderLongNameCount = builderLongNames.Count;
        var longNames = new string[builderLongNameCount];
        for (var i = 0; i < builderLongNameCount; i++)
        {
            var longName = builderLongNames[i];
            longNames[i] = longName;
        }

        LongNames = longNames;

        IsGreedy = builder.IsGreedy;
        Group = builder.Group;
    }
}
