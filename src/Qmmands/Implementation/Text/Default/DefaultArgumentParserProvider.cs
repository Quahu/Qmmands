using System;
using System.Collections.Generic;
using System.Threading;

namespace Qmmands.Text.Default;

public class DefaultArgumentParserProvider : IArgumentParserProvider
{
    public IArgumentParser? DefaultParser => _defaultParser;

    private IArgumentParser? _defaultParser;
    private readonly Dictionary<Type, IArgumentParser> _argumentParsers;

    public DefaultArgumentParserProvider()
    {
        _argumentParsers = new Dictionary<Type, IArgumentParser>();
    }

    public bool SetDefaultParser(Type parserType)
    {
        if (_argumentParsers.TryGetValue(parserType, out var parser))
        {
            Interlocked.Exchange(ref _defaultParser, parser);
            return true;
        }

        return false;
    }

    public void Add(IArgumentParser parser)
    {
        _argumentParsers[parser.GetType()] = parser;
        Interlocked.CompareExchange(ref _defaultParser, parser, null);
    }

    public IArgumentParser? GetParser(ITextCommand command)
    {
        var specificArgumentParserType = command.CustomArgumentParserType;
        if (specificArgumentParserType == null)
            return _defaultParser;

        return _argumentParsers.GetValueOrDefault(specificArgumentParserType);
    }
}
