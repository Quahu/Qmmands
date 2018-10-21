using System;
using System.Collections.Generic;
using System.Linq;

namespace Qmmands
{
    internal sealed class CommandNode
    {
        private readonly CommandService _service;
        private readonly Dictionary<string, List<Command>> _commands;
        private readonly Dictionary<string, List<Module>> _modules;
        private readonly Dictionary<string, CommandNode> _nodes;

        public CommandNode(CommandService service)
        {
            _service = service;
            _commands = new Dictionary<string, List<Command>>(_service.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            _modules = new Dictionary<string, List<Module>>(_service.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            _nodes = new Dictionary<string, CommandNode>(_service.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<CommandMatch> FindCommands(Stack<string> path, string text, int startIndex)
        {
            if (startIndex >= text.Length)
                yield break;

            foreach (var kvp in _commands)
            {
                var index = GetSegment(text, kvp.Key, startIndex, false, out var arguments, out _, out var hasWhitespaceSeparator);
                if (index == -1 || !(hasWhitespaceSeparator || string.IsNullOrWhiteSpace(arguments)))
                    continue;

                foreach (var command in kvp.Value)
                {
                    path.Push(kvp.Key);
                    yield return new CommandMatch(command, kvp.Key, path.Reverse().ToArray(), arguments);
                    path.Pop();
                }
            }

            foreach (var kvp in _nodes)
            {
                var index = GetSegment(text, kvp.Key, startIndex, true, out _, out var hasSeparator, out _);
                if (index == -1 || !hasSeparator)
                    continue;

                path.Push(kvp.Key);
                foreach (var match in kvp.Value.FindCommands(path, text, index))
                    yield return match;
                path.Pop();
            }
        }

        public IEnumerable<ModuleMatch> FindModules(Stack<string> path, string text, int startIndex)
        {
            if (startIndex >= text.Length)
                yield break;

            foreach (var kvp in _modules)
            {
                var index = GetSegment(text, kvp.Key, startIndex, false, out var arguments, out _, out var hasWhitespaceSeparator);
                if (index == -1 || !(hasWhitespaceSeparator || string.IsNullOrWhiteSpace(arguments)))
                    continue;

                foreach (var module in kvp.Value)
                {
                    path.Push(kvp.Key);
                    yield return new ModuleMatch(module, kvp.Key, path.Reverse().ToArray(), arguments);
                    path.Pop();
                }
            }

            foreach (var kvp in _nodes)
            {
                var index = GetSegment(text, kvp.Key, startIndex, true, out _, out var hasSeparator, out _);
                if (index == -1 || !hasSeparator)
                    continue;

                path.Push(kvp.Key);
                foreach (var match in kvp.Value.FindModules(path, text, index))
                    yield return match;
                path.Pop();
            }
        }

        private int GetSegment(string text, string key, int startIndex, bool checkForSeparator, out string arguments, out bool hasSeparator, out bool hasWhitespaceSeparator)
        {
            var index = text.IndexOf(key, startIndex, _service.StringComparison);
            if (index == -1 || index != startIndex)
            {
                arguments = null;
                hasSeparator = false;
                hasWhitespaceSeparator = false;
                return -1;
            }

            else
            {
                index = index + key.Length;
                var hasConfigSeparator = false;
                hasWhitespaceSeparator = false;
                if (!string.IsNullOrWhiteSpace(_service.Separator) && checkForSeparator)
                {
                    for (var i = index; i < text.Length; i++)
                    {
                        if (!char.IsWhiteSpace(text[i]))
                            break;

                        hasWhitespaceSeparator = true;
                        index++;
                    }

                    if (text.IndexOf(_service.Separator, index, _service.StringComparison) == index)
                    {
                        index += _service.Separator.Length;
                        hasConfigSeparator = true;
                        hasWhitespaceSeparator = false;
                    }
                }

                for (var i = index; i < text.Length; i++)
                {
                    if (!char.IsWhiteSpace(text[i]))
                        break;

                    hasWhitespaceSeparator = true;
                    index++;
                }

                arguments = text.Substring(index);
                switch (_service.SeparatorRequirement)
                {
                    case SeparatorRequirement.None:
                        hasSeparator = true;
                        break;

                    case SeparatorRequirement.SeparatorOrWhitespace:
                        hasSeparator = hasConfigSeparator || hasWhitespaceSeparator;
                        break;

                    case SeparatorRequirement.Separator:
                        hasSeparator = hasConfigSeparator || string.IsNullOrWhiteSpace(_service.Separator);
                        break;

                    default:
                        hasSeparator = false;
                        break;
                }

                return index;
            }
        }

        public void AddModule(Module module, string[] segments, int startIndex)
        {
            if (segments.Length == 0)
                return;

            var segment = segments[startIndex];
            if (startIndex == segments.Length - 1)
            {
                if (_modules.TryGetValue(segment, out var modules))
                    modules.Add(module);

                else
                    _modules.Add(segment, new List<Module> { module });
            }

            else
            {
                if (!_nodes.TryGetValue(segment, out var node))
                {
                    node = new CommandNode(_service);
                    _nodes.Add(segment, node);
                }

                node.AddModule(module, segments, startIndex + 1);
            }
        }

        public void RemoveModule(Module module, string[] segments, int startIndex)
        {
            var segment = segments[startIndex];
            if (startIndex == segments.Length - 1)
            {
                if (_modules.TryGetValue(segment, out var modules))
                    modules.Remove(module);
            }

            else
            {
                if (_nodes.TryGetValue(segment, out var node))
                    node.RemoveModule(module, segments, startIndex + 1);
            }
        }

        public void AddCommand(Command command, string[] segments, int startIndex)
        {
            if (segments.Length == 0)
                throw new InvalidOperationException("Cannot add commands to the root node.");

            var segment = segments[startIndex];
            if (startIndex == segments.Length - 1)
            {
                if (_commands.TryGetValue(segment, out var commands))
                    commands.Add(command);

                else
                    _commands.Add(segment, new List<Command> { command });
            }

            else
            {
                if (!_nodes.TryGetValue(segment, out var node))
                {
                    node = new CommandNode(_service);
                    _nodes.Add(segment, node);
                }

                node.AddCommand(command, segments, startIndex + 1);
            }
        }

        public void RemoveCommand(Command command, string[] segments, int startIndex)
        {
            var segment = segments[startIndex];
            if (startIndex == segments.Length - 1)
            {
                if (_commands.TryGetValue(segment, out var commands))
                    commands.Remove(command);
            }

            else
            {
                if (_nodes.TryGetValue(segment, out var node))
                    node.RemoveCommand(command, segments, startIndex + 1);
            }
        }
    }
}
