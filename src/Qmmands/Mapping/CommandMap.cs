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
            => ModuleLoop(module, path, x => MapCommands(x, path));

        private void UnmapModule(Module module, List<string> path)
            => ModuleLoop(module, path, x => UnmapCommands(x, path));

        private void MapCommands(Module module, List<string> path)
            => CommandsLoop(module, path, x => _rootNode.AddCommand(x, path, 0));

        private void UnmapCommands(Module module, List<string> path)
            => CommandsLoop(module, path, x => _rootNode.RemoveCommand(x, path, 0));

        private void ModuleLoop(Module module, List<string> path, Action<Module> action)
        {
            if (module.Aliases.Count == 0)
            {
                action(module);
                for (var j = 0; j < module.Submodules.Count; j++)
                    ModuleLoop(module.Submodules[j], path, action);
            }
            else
            {
                for (var i = 0; i < module.Aliases.Count; i++)
                {
                    var alias = module.Aliases[i];
                    if (alias.Length == 0)
                    {
                        action(module);
                        for (var j = 0; j < module.Submodules.Count; j++)
                            ModuleLoop(module.Submodules[j], path, action);
                    }
                    else
                    {
                        path.Add(alias);

                        action(module);
                        for (var j = 0; j < module.Submodules.Count; j++)
                            ModuleLoop(module.Submodules[j], path, action);

                        path.RemoveAt(path.Count - 1);
                    }
                }
            }
        }

        private void CommandsLoop(Module module, List<string> path, Action<Command> action)
        {
            foreach (var command in module.Commands)
            {
                if (command.Aliases.Count == 0)
                {
                    action(command);
                    continue;
                }

                for (var i = 0; i < command.Aliases.Count; i++)
                {
                    var alias = command.Aliases[i];
                    if (alias.Length == 0)
                    {
                        if (path.Count == 0)
                            continue;

                        action(command);
                    }
                    else
                    {
                        path.Add(alias);
                        action(command);
                        path.RemoveAt(path.Count - 1);
                    }
                }
            }
        }
    }
}
