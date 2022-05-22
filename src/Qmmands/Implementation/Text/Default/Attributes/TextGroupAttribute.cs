using System;
using Qommon.Collections;

namespace Qmmands.Text;

public class TextGroupAttribute : Attribute,
    IModuleBuilderAttribute<ITextModuleBuilder>
{
    public string[] Aliases { get; }

    public TextGroupAttribute(params string[] aliases)
    {
        Aliases = aliases;
    }

    public void Apply(ITextModuleBuilder builder)
    {
        builder.Aliases.AddRange(Aliases);
    }
}
