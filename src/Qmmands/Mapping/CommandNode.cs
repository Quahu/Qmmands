using System;
using System.Collections.Generic;

namespace Qmmands
{
    internal sealed class CommandNode
    {
        private readonly CommandService _service;
        private readonly Dictionary<string, List<Command>> _commands;
        private readonly Dictionary<string, List<Module>> _modules;
        private readonly Dictionary<string, CommandNode> _nodes;
        private readonly bool _isNullOrWhitespaceSeparator;

        public CommandNode(CommandService service)
        {
            _service = service;
            _commands = new Dictionary<string, List<Command>>(_service.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            _modules = new Dictionary<string, List<Module>>(_service.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            _nodes = new Dictionary<string, CommandNode>(_service.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            _isNullOrWhitespaceSeparator = string.IsNullOrWhiteSpace(_service.Separator);
        }

        public IEnumerable<CommandMatch> FindCommands(List<string> path, string text, int startIndex)
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
                    path.Add(kvp.Key);
                    yield return new CommandMatch(command, kvp.Key, path, arguments);
                    path.RemoveAt(path.Count - 1);
                }
            }

            foreach (var kvp in _nodes)
            {
                var index = GetSegment(text, kvp.Key, startIndex, true, out _, out var hasSeparator, out _);
                if (index == -1 || !hasSeparator)
                    continue;

                path.Add(kvp.Key);
                foreach (var match in kvp.Value.FindCommands(path, text, index))
                    yield return match;
                path.RemoveAt(path.Count - 1);
            }
        }

        public IEnumerable<ModuleMatch> FindModules(List<string> path, string text, int startIndex)
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
                    path.Add(kvp.Key);
                    yield return new ModuleMatch(module, kvp.Key, path, arguments);
                    path.RemoveAt(path.Count - 1);
                }
            }

            foreach (var kvp in _nodes)
            {
                var index = GetSegment(text, kvp.Key, startIndex, true, out _, out var hasSeparator, out _);
                if (index == -1 || !hasSeparator)
                    continue;

                path.Add(kvp.Key);
                foreach (var match in kvp.Value.FindModules(path, text, index))
                    yield return match;
                path.RemoveAt(path.Count - 1);
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
                if (!_isNullOrWhitespaceSeparator && checkForSeparator)
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
                        hasSeparator = _isNullOrWhitespaceSeparator ? hasConfigSeparator || hasWhitespaceSeparator : hasConfigSeparator;
                        break;

                    default:
                        throw new InvalidOperationException("Invalid separator requirement.");
                }

                return index;
            }
        }

        public void AddModule(Module module, IReadOnlyList<string> segments, int startIndex)
        {
            if (segments.Count == 0)
                return;

            var segment = segments[startIndex];
            if (startIndex == segments.Count - 1)
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

        public void RemoveModule(Module module, IReadOnlyList<string> segments, int startIndex)
        {
            if (segments.Count == 0)
                return;

            var segment = segments[startIndex];
            if (startIndex == segments.Count - 1)
            {
                if (_modules.TryGetValue(segment, out var modules))
                    modules.Remove(module);
            }

            else if (_nodes.TryGetValue(segment, out var node))
                node.RemoveModule(module, segments, startIndex + 1);
        }

        public void AddCommand(Command command, IReadOnlyList<string> segments, int startIndex)
        {
            if (segments.Count == 0)
                throw new CommandMappingException(command, null, "Cannot map commands without aliases to the root node.");

            var segment = segments[startIndex];
            if (startIndex == segments.Count - 1)
            {
                if (_commands.TryGetValue(segment, out var commands))
                {
                    for (var i = 0; i < commands.Count; i++)
                    {
                        var otherCommand = commands[i];
                        var signature = command.SignatureIdentifier;
                        var otherSignature = otherCommand.SignatureIdentifier;
                        if (signature.Identifier == otherSignature.Identifier)
                        {
                            if (signature.HasRemainder == otherSignature.HasRemainder)
                                throw new CommandMappingException(command, segment, "Cannot map multiple overloads with the same signature.");

                            else if (!signature.HasRemainder && command.IgnoreExtraArguments || !otherSignature.HasRemainder && otherCommand.IgnoreExtraArguments)
                                throw new CommandMappingException(command, segment, "Cannot map multiple overloads with the same argument types, with one of them being a remainder, if the other one ignores extra arguments.");
                        }
                    }

                    commands.Add(command);
                }

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

        public void RemoveCommand(Command command, IReadOnlyList<string> segments, int startIndex)
        {
            var segment = segments[startIndex];
            if (startIndex == segments.Count - 1)
            {
                if (_commands.TryGetValue(segment, out var commands))
                    commands.Remove(command);
            }

            else if (_nodes.TryGetValue(segment, out var node))
                node.RemoveCommand(command, segments, startIndex + 1);
        }
    }
}
