using System.Reflection;
using Qmmands.Default;
using Qmmands.Text.Default;

namespace Qmmands.Text;

public class DefaultTextCommandReflector : CommandReflectorBase<ITextModuleBuilder, ITextCommandBuilder, ITextParameterBuilder, ITextCommandContext>
{
    protected override ITextModuleBuilder CreateModuleBuilder(ITextModuleBuilder? parent, TypeInfo typeInfo)
    {
        return new TextModuleBuilder(parent, typeInfo);
    }

    protected override ITextCommandBuilder CreateCommandBuilder(ITextModuleBuilder module, MethodInfo methodInfo)
    {
        return new TextCommandBuilder(module, methodInfo, ReflectionCommandCallback.Instance);
    }

    protected override ITextParameterBuilder CreateParameterBuilder(ITextCommandBuilder command, ParameterInfo parameterInfo)
    {
        var optionAttribute = parameterInfo.GetCustomAttribute<OptionAttribute>(true);
        if (optionAttribute != null)
            return new OptionParameterBuilder(command, parameterInfo);

        return new PositionalParameterBuilder(command, parameterInfo);
    }
}
