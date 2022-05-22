namespace Qmmands.Text;

public abstract class TextModuleBase<TCommandContext> : ModuleBase<TCommandContext>
    where TCommandContext : ITextCommandContext
{ }
