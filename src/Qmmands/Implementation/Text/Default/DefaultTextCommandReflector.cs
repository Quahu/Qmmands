using System.Reflection;
using Qmmands.Default;

namespace Qmmands.Text;

public class DefaultTextCommandReflector : CommandReflectorBase<ITextModuleBuilder, ITextCommandBuilder, ITextParameterBuilder, ITextCommandContext>
{
    /// <inheritdoc />
    public DefaultTextCommandReflector(ICommandReflectorCallbackProvider callbackProvider)
        : base(callbackProvider)
    { }

    /// <inheritdoc/>
    protected override ITextModuleBuilder CreateModuleBuilder(ITextModuleBuilder? parent, TypeInfo typeInfo)
    {
        return new TextModuleBuilder(parent, typeInfo);
    }

    /// <inheritdoc/>
    protected override ITextCommandBuilder CreateCommandBuilder(ITextModuleBuilder module, MethodInfo methodInfo)
    {
        return new TextCommandBuilder(module, methodInfo, GetCallback(methodInfo));
    }

    /// <inheritdoc/>
    protected override ITextParameterBuilder CreateParameterBuilder(ITextCommandBuilder command, ParameterInfo parameterInfo)
    {
        var optionAttribute = parameterInfo.GetCustomAttribute<OptionAttribute>(true);
        if (optionAttribute != null)
            return new OptionParameterBuilder(command, parameterInfo);

        return new PositionalParameterBuilder(command, parameterInfo);
    }
}
