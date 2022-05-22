using System;
using System.Globalization;

namespace Qmmands.Default;

/// <summary>
///     Represents a delegate that attempts to parse the input value to <typeparamref name="T"/>.
/// </summary>
/// <param name="value"> The value to parse. </param>
/// <param name="result"> The parsed value. </param>
/// <typeparam name="T"> The type to parse to. </typeparam>
public delegate bool TryParseDelegate<T>(ReadOnlySpan<char> value, out T result);

public delegate bool TryParseNumberDelegate<T>(ReadOnlySpan<char> value, NumberStyles styles, IFormatProvider? formatProvider, out T result);