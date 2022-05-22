using System;
using System.Collections;
using System.Globalization;

namespace Qmmands.Default;

public static class CheckUtilities
{
    public static int GetCount(object value)
    {
        if (value is Array array)
            return array.Length;

        if (value is ICollection collection)
            return collection.Count;

        if (value is IEnumerable enumerable)
        {
            var count = 0;
            var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
                count++;

            return count;
        }

        throw new ArgumentException("Invalid argument type");
    }

    public static int GetLength(object value)
    {
        if (value is string stringValue)
            return stringValue.Length;

        if (value is ReadOnlyMemory<char> memory)
            return memory.Length;

        throw new ArgumentException("Invalid argument type.");
    }

    public static long GetInteger(CultureInfo locale, object value)
    {
        if (value is IConvertible convertible)
        {
            if (convertible.GetTypeCode() is TypeCode.Byte or TypeCode.SByte
                or TypeCode.Int16 or TypeCode.UInt16
                or TypeCode.Int32 or TypeCode.UInt32
                or TypeCode.Int64 or TypeCode.UInt64)
                return convertible.ToInt64(locale);
        }

        throw new ArgumentException("Invalid argument type.");
    }

    public static decimal GetNumber(CultureInfo locale, object value)
    {
        if (value is Half half)
            return new decimal((double) half);

        if (value is IConvertible convertible)
        {
            if (convertible.GetTypeCode() is TypeCode.Single or TypeCode.Double or TypeCode.Decimal)
                return convertible.ToDecimal(locale);
        }

        throw new ArgumentException("Invalid argument type.");
    }

    public static bool IsCaseSensitive(StringComparison comparison)
    {
        switch (comparison)
        {
            case StringComparison.CurrentCulture:
            case StringComparison.InvariantCulture:
            case StringComparison.Ordinal:
                return true;

            case StringComparison.CurrentCultureIgnoreCase:
            case StringComparison.InvariantCultureIgnoreCase:
            case StringComparison.OrdinalIgnoreCase:
                return false;

            default:
                throw new ArgumentOutOfRangeException(nameof(comparison));
        }
    }
}
