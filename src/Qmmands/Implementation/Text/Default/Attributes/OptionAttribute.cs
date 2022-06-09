using System;
using Qommon;
using Qommon.Collections;

namespace Qmmands.Text;

/// <summary>
///     Marks the decorated parameter as an <see cref="IOptionParameter"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class OptionAttribute : Attribute,
    IParameterBuilderAttribute<IOptionParameterBuilder>
{
    /// <summary>
    ///     Gets the short names of this option.
    /// </summary>
    public char[]? ShortNames { get; }

    /// <summary>
    ///     Gets the long names of this option.
    /// </summary>
    public string[]? LongNames { get; }

    /// <inheritdoc cref="IOptionParameterBuilder.IsGreedy"/>
    public bool IsGreedy { get; set; }

    /// <inheritdoc cref="IOptionParameterBuilder.Group"/>
    public object? Group { get; set; }

    /// <summary>
    ///     Instantiates a new <see cref="OptionAttribute"/> which denotes that the short and long name
    ///     should be based off of the parameter's name.
    /// </summary>
    /// <remarks>
    ///     If you specify a custom name for the parameter using <see cref="NameAttribute"/>,
    ///     make sure that attribute appears <b>before</b> this attribute so that the change is observed.
    /// </remarks>
    public OptionAttribute()
    { }

    /// <summary>
    ///     Instantiates a new <see cref="OptionAttribute"/> with the specified short names.
    /// </summary>
    /// <param name="shortNames"> The short names for the option. </param>
    public OptionAttribute(params char[] shortNames)
    {
        ShortNames = shortNames;
        LongNames = Array.Empty<string>();
    }

    /// <summary>
    ///     Instantiates a new <see cref="OptionAttribute"/> with the specified long names.
    /// </summary>
    /// <param name="longNames"> The long names for the option. </param>
    public OptionAttribute(params string[] longNames)
    {
        ShortNames = Array.Empty<char>();
        LongNames = longNames;
    }

    /// <summary>
    ///     Instantiates a new <see cref="OptionAttribute"/> with the specified short and long name.
    /// </summary>
    /// <param name="shortName"> The short name for the option. </param>
    /// <param name="longName"> The long name for the option. </param>
    public OptionAttribute(char shortName, string longName)
    {
        ShortNames = new[] { shortName };
        LongNames = new[] { longName };
    }

    /// <summary>
    ///     Instantiates a new <see cref="OptionAttribute"/> with the specified short and long names.
    /// </summary>
    /// <param name="shortName1"> The first short name for the option. </param>
    /// <param name="shortName2"> The second short name for the option. </param>
    /// <param name="longName1"> The first long name for the option. </param>
    /// <param name="longName2"> The second long name for the option. </param>
    public OptionAttribute(char shortName1, char shortName2, string longName1, string longName2)
    {
        ShortNames = new[] { shortName1, shortName2 };
        LongNames = new[] { longName1, longName2 };
    }

    /// <summary>
    ///     Instantiates a new <see cref="OptionAttribute"/> with the specified short and long names.
    /// </summary>
    /// <param name="shortNames"> The short names for the option. </param>
    /// <param name="longNames"> The long names for the option. </param>
    public OptionAttribute(char[] shortNames, string[] longNames)
    {
        ShortNames = shortNames;
        LongNames = longNames;
    }

    /// <inheritdoc/>
    public virtual void Apply(IOptionParameterBuilder builder)
    {
        if (ShortNames != null && LongNames != null)
        {
            builder.ShortNames.AddRange(ShortNames);
            builder.LongNames.AddRange(LongNames);
        }
        else
        {
            Guard.IsNotNullOrWhiteSpace(builder.Name);

            var name = CommandUtilities.ToKebabCase(builder.Name);
            builder.ShortNames.Add(name[0]);

            if (name.Length > 1)
                builder.LongNames.Add(name);
        }

        builder.IsGreedy = IsGreedy;
        builder.Group = Group;
    }
}
