using Qommon;
using Qommon.Collections;

namespace Qmmands.Text;

public class TextCommandAttribute : CommandAttribute
{
    public string[] Aliases { get; }

    public TextCommandAttribute(params string[] aliases)
    {
        Aliases = aliases;
    }

    public override void Apply(ICommandBuilder builder)
    {
        var textBuilder = Guard.IsAssignableToType<ITextCommandBuilder>(builder);
        textBuilder.Aliases.AddRange(Aliases);
    }
}
