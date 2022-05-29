using System;

namespace Qmmands.Text;

// TODO: add module-level support?
/// <summary>
///     Specifies the custom <see cref="IArgumentParser"/> for the decorated text command.
/// </summary>
/// <remarks>
///     This parser has to be retrievable from the <see cref="IArgumentParserProvider"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class CustomArgumentParserAttribute : Attribute,
    ICommandBuilderAttribute<ITextCommandBuilder>
{
    /// <summary>
    ///     Gets the custom <see cref="IArgumentParser"/> type.
    /// </summary>
    public Type CustomArgumentParserType { get; }

    /// <summary>
    ///     Instantiates a new <see cref="CustomArgumentParserAttribute"/> with the specified custom <see cref="IArgumentParser"/> type.
    /// </summary>
    /// <param name="customArgumentParserType"> The type of the custom <see cref="IArgumentParser"/>. </param>
    public CustomArgumentParserAttribute(Type customArgumentParserType)
    {
        CustomArgumentParserType = customArgumentParserType;
    }

    /// <inheritdoc />
    public void Apply(ITextCommandBuilder builder)
    {
        builder.CustomArgumentParserType = CustomArgumentParserType;
    }
}
