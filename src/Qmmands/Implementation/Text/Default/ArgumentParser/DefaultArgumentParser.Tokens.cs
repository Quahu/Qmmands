using System;
using System.Collections.Generic;
using Qommon.Collections;

namespace Qmmands.Text.Default;

public partial class DefaultArgumentParser
{
    /// <summary>
    ///     Represents a text slice.
    /// </summary>
    protected internal struct Slice
    {
        /// <summary>
        ///     Gets the text of this slice.
        /// </summary>
        public ReadOnlyMemory<char> Text { get; }

        /// <summary>
        ///     Gets whether this slice is quoted.
        /// </summary>
        public bool IsQuoted { get; }

        /// <summary>
        ///     Instantiates a new <see cref="Slice"/>.
        /// </summary>
        /// <param name="text"> The text of the slice. </param>
        /// <param name="isQuoted"> Whether the slice is quoted. </param>
        /// <param name="escapeIndices"> The indices of escape characters to skip. </param>
        public Slice(ReadOnlyMemory<char> text, bool isQuoted, FastList<int>? escapeIndices)
        {
            IsQuoted = isQuoted;
            if (escapeIndices == null)
            {
                Text = text;
            }
            else
            {
                Text = string.Create(text.Length - escapeIndices.Count, (text, backslashIndices: escapeIndices), static (span, state) =>
                {
                    var (originalText, escapeIndices) = state;
                    var originalTextSpan = originalText.Span;
                    var escapeIndiceCount = escapeIndices.Count;
                    var index = 0;
                    var escapeIndex = 0;
                    for (var i = 0; i < originalTextSpan.Length; i++)
                    {
                        var isEscape = false;
                        for (var j = escapeIndex; j < escapeIndiceCount; j++)
                        {
                            if (i == escapeIndices[j])
                            {
                                escapeIndex++;
                                isEscape = true;
                                break;
                            }
                        }

                        if (isEscape)
                            continue;

                        span[index++] = originalTextSpan[i];
                    }
                }).AsMemory();
            }
        }
    }

    /// <summary>
    ///     Represents an enumerator that slices the input text.
    /// </summary>
    protected internal struct SliceEnumerator
    {
        /// <summary>
        ///     Gets the current slice.
        /// </summary>
        public Slice Current => _current;

        private Slice _current;

        private readonly IDictionary<char, char> _quotationMarks;
        private ReadOnlyMemory<char> _text;

        /// <summary>
        ///     Instantiates a new <see cref="SliceEnumerator"/>.
        /// </summary>
        /// <param name="quotationMarks"> The quotation marks the enumerator should use. </param>
        /// <param name="text"> The text to slice. </param>
        public SliceEnumerator(IDictionary<char, char> quotationMarks, ReadOnlyMemory<char> text)
        {
            _current = default;
            _text = text.Trim();
            _quotationMarks = quotationMarks;
        }

        /// <summary>
        ///     Advances the enumerator to the next <see cref="Slice"/>.
        /// </summary>
        /// <returns>
        ///     <see langword="true"/> if another slice was found.
        /// </returns>
        public bool MoveNext()
        {
            var text = _text;
            var textSpan = text.Span;
            if (textSpan.IsEmpty)
                return false;

            var isEscaping = false;
            FastList<int>? escapeIndices = null;
            for (var currentCharacterIndex = 0; currentCharacterIndex < textSpan.Length; currentCharacterIndex++)
            {
                var currentCharacter = textSpan[currentCharacterIndex];
                if (char.IsWhiteSpace(currentCharacter))
                {
                    _current = new Slice(text[..currentCharacterIndex], false, escapeIndices);
                    _text = text[currentCharacterIndex..].TrimStart();
                    return true;
                }

                if (currentCharacter == '\\')
                {
                    if (isEscaping)
                    {
                        (escapeIndices ??= new()).Add(currentCharacterIndex - 1);
                        isEscaping = false;
                        continue;
                    }

                    isEscaping = true;
                    continue;
                }

                if (_quotationMarks.TryGetValue(currentCharacter, out var expectedQuote))
                {
                    if (isEscaping)
                    {
                        (escapeIndices ??= new()).Add(currentCharacterIndex - 1);
                        isEscaping = false;
                        continue;
                    }

                    for (var nestedCharacterIndex = currentCharacterIndex + 1; nestedCharacterIndex < textSpan.Length; nestedCharacterIndex++)
                    {
                        var nestedCharacter = textSpan[nestedCharacterIndex];
                        if (currentCharacter == '\\')
                        {
                            if (isEscaping)
                            {
                                (escapeIndices ??= new()).Add(currentCharacterIndex - 1);
                                isEscaping = false;
                                continue;
                            }

                            isEscaping = true;
                            continue;
                        }

                        if (isEscaping)
                        {
                            if (nestedCharacter == expectedQuote)
                                (escapeIndices ??= new()).Add(currentCharacterIndex - 1);

                            isEscaping = false;
                            continue;
                        }

                        if (nestedCharacter != expectedQuote)
                        {
                            continue;
                        }

                        _current = new Slice(text[1..nestedCharacterIndex], true, escapeIndices);
                        _text = ++nestedCharacterIndex < textSpan.Length
                            ? text[nestedCharacterIndex..].TrimStart()
                            : ReadOnlyMemory<char>.Empty;

                        return true;
                    }

                    _current = new Slice(text, true, escapeIndices);
                    _text = ReadOnlyMemory<char>.Empty;
                    return true;
                }

                isEscaping = false;

                if (currentCharacterIndex == textSpan.Length - 1)
                {
                    _current = new Slice(text, false, escapeIndices);
                    _text = ReadOnlyMemory<char>.Empty;
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    ///     Represents a token.
    /// </summary>
    protected internal readonly struct Token
    {
        /// <summary>
        ///     Gets the text of this token.
        /// </summary>
        public ReadOnlyMemory<char> Text { get; }

        /// <summary>
        ///     Gets the type of this token.
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        ///     Gets whether this token instance is a default,
        ///     i.e. has no values set.
        /// </summary>
        public bool IsDefault => Text.IsEmpty;

        /// <summary>
        ///     Instantiates a new <see cref="Token"/>.
        /// </summary>
        /// <param name="text"> The text of the token. </param>
        /// <param name="type"> The type of the token. </param>
        public Token(ReadOnlyMemory<char> text, TokenType type)
        {
            Text = text;
            Type = type;
        }

        /// <summary>
        ///     Returns a new <see cref="Token"/> of type <see cref="TokenType.Value"/>.
        /// </summary>
        /// <param name="text"> The text of the token. </param>
        /// <returns>
        ///     A new <see cref="Token"/>.
        /// </returns>
        public static Token Value(ReadOnlyMemory<char> text)
        {
            return new(text, TokenType.Value);
        }

        /// <summary>
        ///     Returns a new <see cref="Token"/> of type <see cref="TokenType.ShortOption"/>.
        /// </summary>
        /// <param name="text"> The text of the token. </param>
        /// <returns>
        ///     A new <see cref="Token"/>.
        /// </returns>
        public static Token ShortOption(ReadOnlyMemory<char> text)
        {
            return new(text, TokenType.ShortOption);
        }

        /// <summary>
        ///     Returns a new <see cref="Token"/> of type <see cref="TokenType.LongOption"/>.
        /// </summary>
        /// <param name="text"> The text of the token. </param>
        /// <returns>
        ///     A new <see cref="Token"/>.
        /// </returns>
        public static Token LongOption(ReadOnlyMemory<char> text)
        {
            return new(text, TokenType.LongOption);
        }
    }

    /// <summary>
    ///     Represents the type of a token.
    /// </summary>
    protected internal enum TokenType
    {
        /// <summary>
        ///     The token is a value.
        /// </summary>
        Value,

        /// <summary>
        ///     The token is a short option.
        /// </summary>
        ShortOption,

        /// <summary>
        ///     The token is a long option.
        /// </summary>
        LongOption
    }

    /// <summary>
    ///     Represents an enumerator that splits the input text into tokens.
    /// </summary>
    protected internal struct TokenEnumerator
    {
        /// <summary>
        ///     Gets the current token.
        /// </summary>
        public Token Current => _current;

        private Token _current;

        /// <summary>
        ///     Gets whether the option input is terminated using <c>--</c>.
        /// </summary>
        public bool IsTerminated => _isTerminated;

        private bool _isTerminated;

        private SliceEnumerator _sliceEnumerator;

        /// <summary>
        ///     Instantiates a new <see cref="TokenEnumerator"/>.
        /// </summary>
        /// <param name="sliceEnumerator"> The slice enumerator to use. </param>
        public TokenEnumerator(SliceEnumerator sliceEnumerator)
        {
            _isTerminated = false;
            _current = default;
            _sliceEnumerator = sliceEnumerator;
        }

        /// <summary>
        ///     Advances the enumerator to the next <see cref="Token"/>.
        /// </summary>
        /// <returns>
        ///     <see langword="true"/> if another token was found.
        /// </returns>
        public bool MoveNext()
        {
            while (_sliceEnumerator.MoveNext())
            {
                var split = _sliceEnumerator.Current;
                if (split.IsQuoted || _isTerminated)
                {
                    _current = Token.Value(split.Text);
                    return true;
                }

                var splitValue = split.Text;
                var splitValueSpan = splitValue.Span;
                var firstCharacter = splitValueSpan[0];
                if (splitValueSpan.Length > 1)
                {
                    if (firstCharacter == '-')
                    {
                        if (splitValueSpan[1] == '-')
                        {
                            if (splitValueSpan.Length == 2)
                            {
                                _isTerminated = true;
                                continue;
                            }

                            _current = Token.LongOption(splitValue[2..]);
                            return true;
                        }

                        if (!char.IsDigit(splitValueSpan[1]))
                        {
                            _current = Token.ShortOption(splitValue[1..]);
                            return true;
                        }
                    }
                    else if (firstCharacter == '\\' && splitValueSpan[1] == '-')
                    {
                        _current = Token.Value(splitValue[1..]);
                        return true;
                    }
                }

                _current = Token.Value(splitValue);
                return true;
            }

            return false;
        }
    }
}
