using System;
using System.Collections.Generic;

namespace Qmmands
{
    internal sealed class CommandMap
    {
        private readonly CommandMapNode _rootNode;

        public CommandMap(CommandService service)
        {
            _rootNode = new CommandMapNode(service);
        }

        public IReadOnlyList<CommandMatch> FindCommands(string text)
        {
            List<CommandMatch> matches = null;
            _rootNode.FindCommands(ref matches, new List<string>(), text);
            return matches ?? Array.Empty<CommandMatch>() as IReadOnlyList<CommandMatch>;
        }

        public void MapModule(Module module)
        {
            try
            {
                MapModule(module, new List<string>());
            }
            catch
            {
                UnmapModule(module);
                throw;
            }
        }

        public void UnmapModule(Module module)
            => UnmapModule(module, new List<string>());

        private void MapModule(Module module, List<string> path)
            => ModuleLoop(module, path, (map, path, x) => map.MapCommands(x, path));

        private void UnmapModule(Module module, List<string> path)
            => ModuleLoop(module, path, (map, path, x) => map.UnmapCommands(x, path));

        private void MapCommands(Module module, List<string> path)
            => CommandsLoop(module, path, (map, path, x) => map._rootNode.AddCommand(x, path, 0));

        private void UnmapCommands(Module module, List<string> path)
            => CommandsLoop(module, path, (map, path, x) => map._rootNode.RemoveCommand(x, path, 0));

        private void ModuleLoop(Module module, List<string> path, Action<CommandMap, List<string>, Module> action)
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
                    path.Add(alias);

                    action(this, path, module);
                    for (var j = 0; j < module.Submodules.Count; j++)
                        ModuleLoop(module.Submodules[j], path, action);

                    path.RemoveAt(path.Count - 1);
                }
            }
        }

        private void CommandsLoop(Module module, List<string> path, Action<CommandMap, List<string>, Command> action)
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
                    if (alias.Length == 0)
                    {
                        if (path.Count == 0)
                            continue;

                        action(this, path, command);
                    }
                    else
                    {
                        path.Add(alias);
                        action(this, path, command);
                        path.RemoveAt(path.Count - 1);
                    }
                }
            }
        }
    }
}
