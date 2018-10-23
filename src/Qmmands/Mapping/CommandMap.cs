using System.Collections.Generic;

namespace Qmmands
{
    internal sealed class CommandMap
    {
        private readonly CommandNode _rootNode;

        public CommandMap(CommandService service)
            => _rootNode = new CommandNode(service);

        public IEnumerable<CommandMatch> FindCommands(string text)
            => _rootNode.FindCommands(new List<string>(), text, 0);

        public IEnumerable<ModuleMatch> FindModules(string text)
            => _rootNode.FindModules(new List<string>(), text, 0);

        public void AddModule(Module module, IReadOnlyList<string> path)
            => _rootNode.AddModule(module, path, 0);

        public void RemoveModule(Module module, IReadOnlyList<string> path)
            => _rootNode.RemoveModule(module, path, 0);

        public void AddCommand(Command command, IReadOnlyList<string> path)
            => _rootNode.AddCommand(command, path, 0);

        public void RemoveCommand(Command command, IReadOnlyList<string> path)
            => _rootNode.RemoveCommand(command, path, 0);


        public void MapModule(Module module, List<string> path)
        {
            if (module.Aliases.Count == 0)
            {
                MapCommands(module, path);
                AddModule(module, path);
            }

            else
            {
                foreach (var alias in module.Aliases)
                {
                    path.Add(alias);
                    MapCommands(module, path);
                    AddModule(module, path);
                    path.RemoveAt(path.Count - 1);
                }
            }

            for (var i = 0; i < module.Submodules.Count; i++)
                MapModule(module.Submodules[i], path);
        }

        public void UnmapModule(Module module, List<string> path)
        {
            if (module.Aliases.Count == 0)
            {
                UnmapCommands(module, path);
                RemoveModule(module, path);
            }

            else
            {
                foreach (var alias in module.Aliases)
                {
                    path.Add(alias);
                    UnmapCommands(module, path);
                    RemoveModule(module, path);
                    path.RemoveAt(path.Count - 1);
                }
            }

            for (var i = 0; i < module.Submodules.Count; i++)
                UnmapModule(module.Submodules[i], path);
        }

        private void MapCommands(Module module, List<string> path)
        {
            foreach (var command in module.Commands)
            {
                if (command.Aliases.Count == 0)
                    AddCommand(command, path);

                else
                {
                    foreach (var alias in command.Aliases)
                    {
                        path.Add(alias);
                        AddCommand(command, path);
                        path.RemoveAt(path.Count - 1);
                    }
                }
            }
        }

        private void UnmapCommands(Module module, List<string> path)
        {
            foreach (var command in module.Commands)
            {
                if (command.Aliases.Count == 0)
                    RemoveCommand(command, path);

                else
                {
                    foreach (var alias in command.Aliases)
                    {
                        path.Add(alias);
                        RemoveCommand(command, path);
                        path.RemoveAt(path.Count - 1);
                    }
                }
            }
        }
    }
}
