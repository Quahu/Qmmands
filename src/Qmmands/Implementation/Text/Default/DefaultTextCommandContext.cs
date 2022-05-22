using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Qommon;

namespace Qmmands.Text.Default;

public class DefaultTextCommandContext : ITextCommandContext
{
    public IServiceProvider Services { get; }

    public CancellationToken CancellationToken { get; }

    public CultureInfo Locale { get; }

    public ICommandExecutionStep? ExecutionStep { get; set; }

    public ITextCommand? Command { get; set; }

    public ReadOnlyMemory<char>? InputString { get; set; }

    public ReadOnlyMemory<char> RawArgumentString { get; set; }

    public IEnumerable<ReadOnlyMemory<char>>? Path { get; set; }

    public IDictionary<IParameter, MultiString>? RawArguments { get; set; }

    public IDictionary<IParameter, object?>? Arguments { get; set; }

    public IModuleBase? ModuleBase { get; set; }

    public bool IsOverloadDeterminant { get; set; }

    public DefaultTextCommandContext(IServiceProvider services)
    {
        Services = services;
        Locale = CultureInfo.InvariantCulture;
    }

    public virtual ValueTask ResetAsync()
    {
        CommandUtilities.ResetContext(this);
        return default;
    }
}
