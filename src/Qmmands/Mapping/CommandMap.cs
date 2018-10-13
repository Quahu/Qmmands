using System.Collections.Generic;
using System.Linq;

namespace Qmmands
{
    internal sealed class CommandMap
    {
        private readonly CommandNode _rootNode;

        public CommandMap(CommandService service)
            => _rootNode = new CommandNode(service);

        public void AddModule(Module module, Stack<string> path)
        {
            if (module.Aliases.Count == 0)
                AddCommands(module, path);

            else
            {
                foreach (var alias in module.Aliases)
                {
                    path.Push(alias);
                    AddCommands(module, path);
                    path.Pop();
                }
            }
        }

        public void RemoveModule(Module module, Stack<string> path)
        {
            if (module.Aliases.Count == 0)
                RemoveCommands(module, path);

            else
            {
                foreach (var alias in module.Aliases)
                {
                    path.Push(alias);
                    RemoveCommands(module, path);
                    path.Pop();
                }
            }
        }

        public IEnumerable<CommandMatch> FindCommands(string text)
            => _rootNode.FindCommands(new Stack<string>(), text, 0);

        public void AddCommand(Command command, string[] path)
            => _rootNode.AddCommand(command, path, 0);

        public void RemoveCommand(Command command, string[] path)
            => _rootNode.RemoveCommand(command, path, 0);

        private void AddCommands(Module module, Stack<string> path)
        {
            foreach (var command in module.Commands)
            {
                if (command.Aliases.Count == 0)
                    AddCommand(command, path.Reverse().ToArray());

                else
                {
                    foreach (var alias in command.Aliases)
                    {
                        path.Push(alias);
                        AddCommand(command, path.Reverse().ToArray());
                        path.Pop();
                    }
                }
            }

            foreach (var submodule in module.Submodules)
                AddModule(submodule, path);
        }

        private void RemoveCommands(Module module, Stack<string> path)
        {
            foreach (var command in module.Commands)
            {
                if (command.Aliases.Count == 0)
                    RemoveCommand(command, path.Reverse().ToArray());

                else
                {
                    foreach (var alias in command.Aliases)
                    {
                        path.Push(alias);
                        RemoveCommand(command, path.Reverse().ToArray());
                        path.Pop();
                    }
                }
            }

            foreach (var submodule in module.Submodules)
                RemoveModule(submodule, path);
        }
    }
}
