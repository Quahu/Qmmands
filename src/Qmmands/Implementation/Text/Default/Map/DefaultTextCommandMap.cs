using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qommon;
using Qommon.Collections;

namespace Qmmands.Text.Default;

public partial class DefaultTextCommandMap : ITextCommandMap
{
    public ILogger Logger { get; }

    public StringComparison StringComparison { get; }

    public string Separator { get; }

    public Node RootNode => _rootNode ??= CreateNode();

    private Node? _rootNode;

    public DefaultTextCommandMap(
        IOptions<DefaultTextCommandMapConfiguration> options,
        ILogger<DefaultTextCommandMap> logger)
    {
        Logger = logger;

        var configuration = options.Value;
        StringComparison = configuration.StringComparison;
        Separator = configuration.Separator;
    }

    public void MapModule(IModule module)
    {
        Guard.IsAssignableToType<ITextModule>(module);

        try
        {
            var path = new FastList<ReadOnlyMemory<char>>();
            MapModule((module as ITextModule)!, path);
        }
        catch
        {
            UnmapModule(module);
            throw;
        }
    }

    public void UnmapModule(IModule module)
    {
        Guard.IsAssignableToType<ITextModule>(module);

        var path = new FastList<ReadOnlyMemory<char>>();
        UnmapModule((module as ITextModule)!, path);
    }

    protected virtual void MapModule(ITextModule module, IList<ReadOnlyMemory<char>> path)
        => ModuleLoop(module, path, (map, path, module) => map.MapCommands(module, path));

    protected virtual void UnmapModule(ITextModule module, IList<ReadOnlyMemory<char>> path)
        => ModuleLoop(module, path, (map, path, module) => map.UnmapCommands(module, path));

    protected virtual void MapCommands(ITextModule module, IList<ReadOnlyMemory<char>> path)
        => CommandsLoop(module, path, (map, path, command) => map.RootNode.AddCommand(command, path, 0));

    protected virtual void UnmapCommands(ITextModule module, IList<ReadOnlyMemory<char>> path)
        => CommandsLoop(module, path, (map, path, command) => map.RootNode.RemoveCommand(command, path, 0));

    protected delegate void ModuleLoopDelegate(DefaultTextCommandMap map, IList<ReadOnlyMemory<char>> path, ITextModule module);

    protected virtual void ModuleLoop(ITextModule module, IList<ReadOnlyMemory<char>> path, ModuleLoopDelegate action)
    {
        if (module.Aliases.Count == 0)
        {
            action(this, path, module);
            for (var j = 0; j < module.Submodules.Count; j++)
                ModuleLoop(module.Submodules[j], path, action);

            return;
        }

        for (var i = 0; i < module.Aliases.Count; i++)
        {
            var alias = module.Aliases[i];
            if (alias.Length == 0)
            {
                action(this, path, module);
                for (var j = 0; j < module.Submodules.Count; j++)
                    ModuleLoop(module.Submodules[j], path, action);
            }
            else
            {
                path.Add(alias.AsMemory());

                action(this, path, module);
                for (var j = 0; j < module.Submodules.Count; j++)
                    ModuleLoop(module.Submodules[j], path, action);

                path.RemoveAt(path.Count - 1);
            }
        }
    }

    protected delegate void CommandsLoopDelegate(DefaultTextCommandMap map, IList<ReadOnlyMemory<char>> path, ITextCommand command);

    protected virtual void CommandsLoop(ITextModule module, IList<ReadOnlyMemory<char>> path, CommandsLoopDelegate action)
    {
        for (var i = 0; i < module.Commands.Count; i++)
        {
            var command = module.Commands[i];
            if (command.Aliases.Count == 0)
            {
                action(this, path, command);
                continue;
            }

            for (var j = 0; j < command.Aliases.Count; j++)
            {
                var alias = command.Aliases[j];

                // TODO: is this of any use?
                if (alias.Length == 0)
                {
                    if (CommandOverloadComparer.GetPathLength(path) == 0)
                        continue;

                    action(this, path, command);
                }
                else
                {
                    path.Add(alias.AsMemory());
                    action(this, path, command);
                    path.RemoveAt(path.Count - 1);
                }
            }
        }
    }

    protected virtual Node CreateNode()
        => new(this);

    protected virtual StringComparison GetStringComparison(Node node)
        => StringComparison;

    protected virtual string GetSeparator(Node node)
        => Separator;

    public virtual IList<ITextCommandMatch> FindMatches(ReadOnlyMemory<char> input)
    {
        IList<ITextCommandMatch>? matches = null;
        IImmutableStack<ReadOnlyMemory<char>> path = ImmutableStack<ReadOnlyMemory<char>>.Empty;
        RootNode.FindCommands(ref matches, ref path, input);
        return matches ?? Array.Empty<ITextCommandMatch>() as IList<ITextCommandMatch>;
    }
}
