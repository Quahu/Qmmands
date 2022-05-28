using Qommon;
using Qommon.Collections;

namespace Qmmands.Text;

/// <summary>
///     Marks the decorated method as a text command.
/// </summary>
public class TextCommandAttribute : CommandAttribute
{
    /// <summary>
    ///     Gets the aliases of the command.
    /// </summary>
    public string[] Aliases { get; }

    /// <summary>
    ///     Instantiates a new <see cref="TextCommandAttribute"/> with the specified aliases.
    /// </summary>
    /// <param name="aliases"> The aliases of the command. </param>
    public TextCommandAttribute(params string[] aliases)
    {
        Aliases = aliases;
    }

    /// <inheritdoc/>
    public override void Apply(ICommandBuilder builder)
    {
        var textBuilder = Guard.IsAssignableToType<ITextCommandBuilder>(builder);
        textBuilder.Aliases.AddRange(Aliases);
    }
}
