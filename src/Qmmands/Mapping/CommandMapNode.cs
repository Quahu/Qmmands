using System;
using System.Collections.Generic;

namespace Qmmands
{
    internal sealed class CommandMapNode
    {
        private readonly CommandService _service;
        private readonly Dictionary<string, List<Command>> _commands;
        private readonly Dictionary<string, CommandMapNode> _nodes;
        private readonly bool _isSeparatorSingleWhitespace;

        public CommandMapNode(CommandService service)
        {
            _service = service;
            _commands = new Dictionary<string, List<Command>>(_service.StringComparer);
            _nodes = new Dictionary<string, CommandMapNode>(_service.StringComparer);
            _isSeparatorSingleWhitespace = _service.Separator.Length == 1 && char.IsWhiteSpace(_service.Separator[0]);
        }

        public void FindCommands(ref List<CommandMatch> matches, List<string> path, ReadOnlySpan<char> span)
        {
            if (path.Count == 0)
                span = span.TrimStart();

            if (span.IsEmpty)
                return;

            var segment = FindNextSegment(span, out var encounteredWhitespace, out var encounteredSeparator, out var remaining);
            if (encounteredSeparator && remaining.IsEmpty)
                return;

            var stringSegment = new string(segment);
            if (!encounteredSeparator && !(encounteredWhitespace && remaining.IsEmpty) && _commands.TryGetValue(stringSegment, out var commands))
            {
                path.Add(stringSegment);

                var stringRemaining = remaining.IsEmpty
                    ? string.Empty
                    : new string(remaining);

                if (matches == null)
                    matches = new List<CommandMatch>(commands.Count);

                for (var i = 0; i < commands.Count; i++)
                    matches.Add(new CommandMatch(commands[i], stringSegment, path, stringRemaining));

                path.RemoveAt(path.Count - 1);
            }

            bool hasSeparator;
            switch (_service.SeparatorRequirement)
            {
                case SeparatorRequirement.Separator when _isSeparatorSingleWhitespace:
                {
                    hasSeparator = encounteredWhitespace;
                    break;
                }

                case SeparatorRequirement.Separator:
                {
                    hasSeparator = encounteredSeparator;
                    break;
                }

                case SeparatorRequirement.SeparatorOrWhitespace:
                {
                    hasSeparator = encounteredWhitespace || encounteredSeparator;
                    break;
                }

                default:
                    throw new InvalidOperationException("Invalid separator requirement.");
            }

            if (hasSeparator && _nodes.TryGetValue(stringSegment, out var node))
            {
                path.Add(stringSegment);
                node.FindCommands(ref matches, path, remaining);
                path.RemoveAt(path.Count - 1);
            }
        }

        private ReadOnlySpan<char> FindNextSegment(ReadOnlySpan<char> span,
            out bool encounteredWhitespace, out bool encounteredSeparator, out ReadOnlySpan<char> remaining)
        {
            encounteredWhitespace = false;
            encounteredSeparator = false;
            var segmentIndex = 0;
            var nextSegmentIndex = 0;
            var separator = _service.Separator;
            var separatorIndex = _isSeparatorSingleWhitespace
                ? -1
                : span.IndexOf(separator, _service.StringComparison);
            for (var i = 0; i < span.Length; i++)
            {
                if (segmentIndex != 0)
                {
                    if (i == separatorIndex)
                    {
                        encounteredSeparator = true;
                        if (!_isSeparatorSingleWhitespace)
                            encounteredWhitespace = false;
                        i += separator.Length - 1;
                        nextSegmentIndex += separator.Length;
                        continue;
                    }

                    if (char.IsWhiteSpace(span[i]))
                    {
                        encounteredWhitespace = true;
                        nextSegmentIndex++;
                        continue;
                    }
                }

                if (segmentIndex != nextSegmentIndex)
                    break;

                segmentIndex++;
                nextSegmentIndex++;
            }

            remaining = span.Slice(nextSegmentIndex);
            return span.Slice(0, segmentIndex);
        }

        private void ValidateCommand(Command command, string segment, List<Command> commands)
        {
            for (var i = 0; i < commands.Count; i++)
            {
                var otherCommand = commands[i];
                var signature = command.SignatureIdentifier;
                var otherSignature = otherCommand.SignatureIdentifier;
                if (signature.Identifier != otherSignature.Identifier)
                    continue;

                if (signature.HasRemainder == otherSignature.HasRemainder)
                {
                    throw new CommandMappingException(command, segment,
                       "Cannot map multiple overloads with the same signature.");
                }
                else if (!signature.HasRemainder && command.IgnoresExtraArguments || !otherSignature.HasRemainder && otherCommand.IgnoresExtraArguments)
                {
                    throw new CommandMappingException(command, segment,
                       "Cannot map multiple overloads with the same argument types, with one of them being a remainder, if the other one ignores extra arguments.");
                }
            }
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
                    ValidateCommand(command, segment, commands);
                    commands.Add(command);
                }
                else
                {
                    _commands.Add(segment, new List<Command> { command });
                }
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
            if (segments.Count == 0)
                return;

            var segment = segments[startIndex];
            if (startIndex == segments.Count - 1)
            {
                if (_commands.TryGetValue(segment, out var commands))
                {
                    commands.Remove(command);
                    if (commands.Count == 0)
                        _commands.Remove(segment);
                }
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
