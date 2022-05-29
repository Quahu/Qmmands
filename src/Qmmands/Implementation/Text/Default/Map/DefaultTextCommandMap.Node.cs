using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Qommon.Collections;

namespace Qmmands.Text.Default;

public partial class DefaultTextCommandMap
{
    public class Node
    {
        protected readonly DefaultTextCommandMap Map;

        protected readonly Dictionary<ReadOnlyMemory<char>, List<ITextCommand>> Commands;

        protected readonly Dictionary<ReadOnlyMemory<char>, Node> Nodes;

        public Node(DefaultTextCommandMap map)
        {
            Map = map;

            var stringComparison = map.GetStringComparison(this);
            var segmentComparer = SegmentComparer.FromComparison(stringComparison);

            Commands = new Dictionary<ReadOnlyMemory<char>, List<ITextCommand>>(segmentComparer);
            Nodes = new Dictionary<ReadOnlyMemory<char>, Node>(segmentComparer);
        }

        public class SegmentComparer : IEqualityComparer<ReadOnlyMemory<char>>
        {
            private static SegmentComparer Ordinal => _ordinal ??= new SegmentComparer(StringComparison.Ordinal);

            private static SegmentComparer? _ordinal;

            private static SegmentComparer OrdinalIgnoreCase => _ordinalIgnoreCase ??= new SegmentComparer(StringComparison.OrdinalIgnoreCase);

            private static SegmentComparer? _ordinalIgnoreCase;

            public readonly StringComparison StringComparison;

            public SegmentComparer(StringComparison stringComparison)
            {
                StringComparison = stringComparison;
            }

            public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y)
            {
                return x.Span.Equals(y.Span, StringComparison);
            }

            public int GetHashCode(ReadOnlyMemory<char> obj)
            {
                return string.GetHashCode(obj.Span, StringComparison);
            }

            public static SegmentComparer FromComparison(StringComparison stringComparison)
            {
                return stringComparison switch
                {
                    StringComparison.OrdinalIgnoreCase => OrdinalIgnoreCase,
                    StringComparison.Ordinal => Ordinal,
                    _ => new SegmentComparer(stringComparison)
                };
            }
        }

        protected virtual void ValidateCommand(ITextCommand command, ReadOnlyMemory<char> segment, List<ITextCommand> commands)
        {
            for (var i = 0; i < commands.Count; i++)
            {
                var otherCommand = commands[i];
                var signature = command.GetSignatureIdentifier();
                var otherSignature = otherCommand.GetSignatureIdentifier();
                if (signature.Identifier != otherSignature.Identifier)
                    continue;

                if (signature.HasRemainder == otherSignature.HasRemainder)
                {
                    throw new TextCommandMappingException(command, segment,
                        "Cannot map multiple overloads with the same signature.");
                }

                if (!signature.HasRemainder && command.IgnoresExtraArguments || !otherSignature.HasRemainder && otherCommand.IgnoresExtraArguments)
                {
                    throw new TextCommandMappingException(command, segment,
                        "Cannot map multiple overloads with the same argument types, with one of them being a remainder, if the other one ignores extra arguments.");
                }
            }
        }

        public virtual void AddCommand(ITextCommand command, IList<ReadOnlyMemory<char>> segments, int startIndex)
        {
            if (segments.Count == 0)
                throw new TextCommandMappingException(command, ReadOnlyMemory<char>.Empty, "Cannot map commands without aliases to the root node.");

            var segment = segments[startIndex];
            if (startIndex == segments.Count - 1)
            {
                if (Commands.TryGetValue(segment, out var commands))
                {
                    ValidateCommand(command, segment, commands);
                    commands.Add(command);
                }
                else
                {
                    Commands.Add(segment, new List<ITextCommand> { command });
                }
            }
            else
            {
                if (!Nodes.TryGetValue(segment, out var node))
                {
                    node = Map.CreateNode();
                    Nodes.Add(segment, node);
                }

                node.AddCommand(command, segments, startIndex + 1);
            }
        }

        public virtual void RemoveCommand(ITextCommand command, IList<ReadOnlyMemory<char>> segments, int startIndex)
        {
            if (segments.Count == 0)
                return;

            var segment = segments[startIndex];
            if (startIndex == segments.Count - 1)
            {
                if (Commands.TryGetValue(segment, out var commands))
                {
                    commands.Remove(command);
                    if (commands.Count == 0)
                        Commands.Remove(segment);
                }
            }
            else if (Nodes.TryGetValue(segment, out var node))
            {
                node.RemoveCommand(command, segments, startIndex + 1);

                if (node.Commands.Count == 0)
                    Nodes.Remove(segment);
            }
        }

        public virtual void FindCommands(ref IList<ITextCommandMatch>? matches, ref IImmutableStack<ReadOnlyMemory<char>> path, ReadOnlyMemory<char> text)
        {
            if (path.IsEmpty)
                text = text.TrimStart();

            if (text.IsEmpty)
                return;

            var segment = FindNextSegment(text, out var encounteredSeparator, out var encounteredWhiteSpace, out var remainingText);
            if (encounteredSeparator && remainingText.IsEmpty)
                return;

            // switch (_service.SeparatorRequirement)
            // {
            //     case SeparatorRequirement.Separator when _isSeparatorSingleWhitespace:
            //     {
            //         hasSeparator = encounteredWhitespace;
            //         break;
            //     }
            //
            //     case SeparatorRequirement.Separator:
            //     {
            //         hasSeparator = encounteredSeparator;
            //         break;
            //     }
            //
            //     case SeparatorRequirement.SeparatorOrWhitespace:
            //     {
            //         hasSeparator = encounteredWhitespace || encounteredSeparator;
            //         break;
            //     }
            //
            //     default:
            //         throw new InvalidOperationException("Invalid separator requirement.");
            // }

            if (!encounteredSeparator && !(encounteredWhiteSpace && remainingText.IsEmpty))
            {
                if (Commands.TryGetValue(segment, out var commands))
                {
                    path = path.Push(segment);

                    matches ??= new FastList<ITextCommandMatch>(commands.Count);

                    for (var i = 0; i < commands.Count; i++)
                        matches.Add(new DefaultTextCommandMatch(commands[i], path, remainingText));

                    path = path.Pop();
                }
            }

            if (Nodes.TryGetValue(segment, out var node))
            {
                path = path.Push(segment);
                node.FindCommands(ref matches, ref path, remainingText);
                path = path.Pop();
            }
        }

        // protected virtual ReadOnlyMemory<char> FindNextSegment(ReadOnlyMemory<char> text,
        //     out bool encounteredSeparator, out ReadOnlyMemory<char> remainingText)
        // {
        //     encounteredSeparator = false;
        //     var segmentIndex = 0;
        //     var nextSegmentIndex = 0;
        //     var separator = Map.GetSeparator(this);
        //     var textSpan = text.Span;
        //     var separatorIndex = textSpan.IndexOf(separator, Map.GetStringComparison(this));
        //     if (separatorIndex == -1)
        //     {
        //         encounteredSeparator = false;
        //         remainingText = default;
        //         return text.TrimEnd();
        //     }
        //
        //     for (var i = 0; i < text.Length; i++)
        //     {
        //         if (segmentIndex != 0)
        //         {
        //             if (i == separatorIndex)
        //             {
        //                 encounteredSeparator = true;
        //                 i += separator.Length - 1;
        //                 nextSegmentIndex += separator.Length;
        //                 continue;
        //             }
        //
        //             if (char.IsWhiteSpace(textSpan[i]))
        //             {
        //                 nextSegmentIndex++;
        //                 continue;
        //             }
        //         }
        //
        //         if (segmentIndex != nextSegmentIndex)
        //             break;
        //
        //         segmentIndex++;
        //         nextSegmentIndex++;
        //     }
        //
        //     remainingText = text[nextSegmentIndex..];
        //     return text[..segmentIndex];
        // }

        protected virtual ReadOnlyMemory<char> FindNextSegment(ReadOnlyMemory<char> text,
            out bool encounteredSeparator, out bool encounteredWhiteSpace, out ReadOnlyMemory<char> remainingText)
        {
            encounteredSeparator = false;
            encounteredWhiteSpace = false;
            var segmentIndex = 0;
            var nextSegmentIndex = 0;
            var separator = Map.GetSeparator(this);
            var isSeparatorSingleWhitespace = separator.Length == 1 && char.IsWhiteSpace(separator[0]);
            var textSpan = text.Span;
            var separatorIndex = isSeparatorSingleWhitespace
                ? -1
                : textSpan.IndexOf(separator, Map.GetStringComparison(this));

            for (var i = 0; i < text.Length; i++)
            {
                if (segmentIndex != 0)
                {
                    if (i == separatorIndex)
                    {
                        encounteredSeparator = true;

                        if (!isSeparatorSingleWhitespace)
                            encounteredWhiteSpace = false;

                        i += separator.Length - 1;
                        nextSegmentIndex += separator.Length;
                        continue;
                    }

                    if (char.IsWhiteSpace(textSpan[i]))
                    {
                        encounteredWhiteSpace = true;
                        nextSegmentIndex++;
                        continue;
                    }
                }

                if (segmentIndex != nextSegmentIndex)
                    break;

                segmentIndex++;
                nextSegmentIndex++;
            }

            remainingText = text[nextSegmentIndex..];
            return text[..segmentIndex];
        }
    }
}
