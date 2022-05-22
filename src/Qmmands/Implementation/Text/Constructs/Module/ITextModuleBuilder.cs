using System.Collections.Generic;

namespace Qmmands.Text;

public interface ITextModuleBuilder : IModuleBuilder
{
    /// <inheritdoc cref="IModule.Parent"/>
    new ITextModuleBuilder? Parent { get; }

    IModuleBuilder? IModuleBuilder.Parent => Parent;

    /// <inheritdoc cref="IModule.Commands"/>
    new IList<ITextCommandBuilder> Commands { get; }

    /// <inheritdoc cref="IModule.Submodules"/>
    new IList<ITextModuleBuilder> Submodules { get; }

    IList<string> Aliases { get; }

    ITextModule Build(ITextModule? parent = null);

    IModule IModuleBuilder.Build(IModule? parent)
        => Build((ITextModule?) parent);
}
