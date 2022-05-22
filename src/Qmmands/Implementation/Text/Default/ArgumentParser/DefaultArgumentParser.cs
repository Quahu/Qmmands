using System.Threading.Tasks;
using Microsoft.Extensions.Options;

#pragma warning disable CS0618

namespace Qmmands.Text.Default;

public class DefaultArgumentParser : IArgumentParser
{
    public bool SupportsOptionalParameters => _classicArgumentParser.SupportsOptionalParameters;

    private readonly ClassicArgumentParser _classicArgumentParser;

    public DefaultArgumentParser(
        IOptions<DefaultArgumentParserConfiguration> options)
    {
        _classicArgumentParser = new ClassicArgumentParser(options);
    }

    public void Validate(ITextCommand command)
    {
        _classicArgumentParser.Validate(command);
    }

    public ValueTask<IArgumentParserResult> ParseAsync(ITextCommandContext context)
    {
        return _classicArgumentParser.ParseAsync(context);

        // context.CancellationToken.ThrowIfCancellationRequested();
        //
        // var command = context.Command;
        // var argumentBuilder = new StringBuilder();
        // var rawArgumentSpan = context.RawArgumentString.AsSpan().TrimStart();
        //
        // var currentQuotationMark = '\0';
        // var expectedQuotationMark = '\0';
        // Dictionary<IParameter, MultiString> arguments = null;
        // IParameter currentParameter = null;
        //
        // for (var currentPosition = 0; currentPosition < rawArgumentSpan.Length; currentPosition++)
        // {
        //     var currentCharacter = rawArgumentSpan[currentPosition];
        //     if (currentParameter == null)
        //     {
        //         if (char.IsWhiteSpace(currentCharacter))
        //             continue;
        //     }
        // }
    }
}