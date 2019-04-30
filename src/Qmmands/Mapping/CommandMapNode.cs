using System;
using System.Collections.Generic;

namespace Qmmands
{
    internal sealed class CommandMapNode
    {
        private readonly CommandService _service;
        private readonly Dictionary<string, List<Command>> _commands;
        private readonly Dictionary<string, CommandMapNode> _nodes;
        private readonly bool _isNullOrWhitespaceSeparator;

        public CommandMapNode(CommandService service)
        {
            _service = service;
            _commands = new Dictionary<string, List<Command>>(_service.StringComparer);
            _nodes = new Dictionary<string, CommandMapNode>(_service.StringComparer);
            _isNullOrWhitespaceSeparator = string.IsNullOrWhiteSpace(_service.Separator);
        }

        public IEnumerable<CommandMatch> FindCommands(List<string> path, string text, int startIndex)
        {
            if (startIndex >= text.Length)
                yield break;

            foreach (var (segment, commands) in _commands)
            {
                var index = GetSegment(text, segment, startIndex, false, out var arguments, out _, out var hasWhitespaceSeparator);
                if (index == -1 || !(hasWhitespaceSeparator || string.IsNullOrWhiteSpace(arguments)))
                    continue;

                for (var i = 0; i < commands.Count; i++)
                {
                    path.Add(segment);
                    yield return new CommandMatch(commands[i], segment, path, arguments);
                    path.RemoveAt(path.Count - 1);
                }
            }

            foreach (var (segment, node) in _nodes)
            {
                var index = GetSegment(text, segment, startIndex, true, out _, out var hasSeparator, out _);
                if (index == -1 || !hasSeparator)
                    continue;

                path.Add(segment);
                foreach (var match in node.FindCommands(path, text, index))
                    yield return match;

                path.RemoveAt(path.Count - 1);
            }
        }

        private int GetSegment(ReadOnlySpan<char> text, ReadOnlySpan<char> key, int startIndex, bool checkForSeparator,
            out string arguments, out bool hasSeparator, out bool hasWhitespaceSeparator)
        {
            var index = text.Slice(startIndex).IndexOf(key, _service.StringComparison) + startIndex;
            if (index == -1 || index != startIndex)
            {
                arguments = null;
                hasSeparator = false;
                hasWhitespaceSeparator = false;
                return -1;
            }
            else
            {
                index += key.Length;
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

                    var separatorIndex = text.Slice(index).IndexOf(_service.Separator, _service.StringComparison) + index;
                    if (separatorIndex == index)
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

                arguments = new string(text.Slice(index));
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

        private void ValidateCommand(Command command, IReadOnlyList<string> segments, int startIndex,
            out string segment, out List<Command> commands)
        {
            if (segments.Count == 0)
                throw new CommandMappingException(command, null, "Cannot map commands without aliases to the root node.");

            segment = segments[startIndex];
            if (!_commands.TryGetValue(segment, out commands))
                return;

            for (var i = 0; i < commands.Count; i++)
            {
                var otherCommand = commands[i];
                var signature = command.SignatureIdentifier;
                var otherSignature = otherCommand.SignatureIdentifier;
                if (signature.Identifier != otherSignature.Identifier)
                    continue;

                if (signature.HasRemainder == otherSignature.HasRemainder)
                    throw new CommandMappingException(command, segment,
                        "Cannot map multiple overloads with the same signature.");

                else if (!signature.HasRemainder && command.IgnoresExtraArguments || !otherSignature.HasRemainder && otherCommand.IgnoresExtraArguments)
                    throw new CommandMappingException(command, segment,
                        "Cannot map multiple overloads with the same argument types, with one of them being a remainder, if the other one ignores extra arguments.");
            }
        }

        public void AddCommand(Command command, IReadOnlyList<string> segments, int startIndex)
        {
            ValidateCommand(command, segments, startIndex, out var segment, out var commands);
            if (startIndex == segments.Count - 1)
            {
                if (commands != null)
                    commands.Add(command);

                else
                    _commands.Add(segment, new List<Command> { command });
            }
            else
            {
                if (!_nodes.TryGetValue(segment, out var node))
                {
                    node = new CommandMapNode(_service);
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
            {
                node.RemoveCommand(command, segments, startIndex + 1);

                if (node._commands.Count == 0)
                    _nodes.Remove(segment);
            }
        }
    }
}
