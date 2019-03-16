﻿using System.Collections.Generic;

namespace Qmmands
{
    internal sealed class CommandMap
    {
        private readonly CommandMapNode _rootNode;

        public CommandMap(CommandService service)
            => _rootNode = new CommandMapNode(service);

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

        public void MapModule(Module module)
            => MapModule(module, new List<string>());

        private void MapModule(Module module, List<string> path)
        {
            if (module.Aliases.Count == 0)
            {
                MapCommands(module, path);

                for (var j = 0; j < module.Submodules.Count; j++)
                    MapModule(module.Submodules[j], path);

                AddModule(module, path);
            }

            else
            {
                for (var i = 0; i < module.Aliases.Count; i++)
                {
                    var alias = module.Aliases[i];
                    if (alias.Length == 0)
                    {
                        MapCommands(module, path);

                        for (var j = 0; j < module.Submodules.Count; j++)
                            MapModule(module.Submodules[j], path);

                        AddModule(module, path);
                    }
                    else
                    {
                        path.Add(alias);
                        MapCommands(module, path);

                        for (var j = 0; j < module.Submodules.Count; j++)
                            MapModule(module.Submodules[j], path);

                        AddModule(module, path);
                        path.RemoveAt(path.Count - 1);
                    }
                }
            }
        }

        public void UnmapModule(Module module)
            => UnmapModule(module, new List<string>());

        private void UnmapModule(Module module, List<string> path)
        {
            if (module.Aliases.Count == 0)
            {
                UnmapCommands(module, path);

                for (var j = 0; j < module.Submodules.Count; j++)
                    UnmapModule(module.Submodules[j], path);

                RemoveModule(module, path);
            }

            else
            {
                for (var i = 0; i < module.Aliases.Count; i++)
                {
                    var alias = module.Aliases[i];
                    if (alias.Length == 0)
                    {
                        UnmapCommands(module, path);

                        for (var j = 0; j < module.Submodules.Count; j++)
                            UnmapModule(module.Submodules[j], path);

                        RemoveModule(module, path);
                    }
                    else
                    {
                        path.Add(alias);
                        UnmapCommands(module, path);

                        for (var j = 0; j < module.Submodules.Count; j++)
                            UnmapModule(module.Submodules[j], path);

                        RemoveModule(module, path);
                        path.RemoveAt(path.Count - 1);
                    }
                }
            }
        }

        private void MapCommands(Module module, List<string> path)
        {
            foreach (var command in module.Commands)
            {
                if (command.Aliases.Count == 0)
                    AddCommand(command, path);

                else
                {
                    for (var i = 0; i < command.Aliases.Count; i++)
                    {
                        var alias = command.Aliases[i];
                        if (alias.Length == 0)
                        {
                            if (path.Count == 0)
                                continue;

                            AddCommand(command, path);
                        }
                        else
                        {
                            path.Add(alias);
                            AddCommand(command, path);
                            path.RemoveAt(path.Count - 1);
                        }
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
                    for (var i = 0; i < command.Aliases.Count; i++)
                    {
                        var alias = command.Aliases[i];
                        if (alias.Length == 0)
                        {
                            if (path.Count == 0)
                                continue;

                            RemoveCommand(command, path);
                        }
                        else
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
}
