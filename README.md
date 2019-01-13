# Qmmands
[![Build Status](https://img.shields.io/appveyor/ci/Quahu/qmmands.svg?style=flat-square)](https://ci.appveyor.com/project/Quahu/qmmands)
[![NuGet](https://img.shields.io/nuget/v/Qmmands.svg?style=flat-square)](https://www.nuget.org/packages/Qmmands/)
[![MyGet](https://img.shields.io/myget/qmmands/vpre/Qmmands.svg?style=flat-square&label=myget)](https://www.myget.org/gallery/qmmands)
[![The Lab](https://img.shields.io/discord/416256456505950215.svg?style=flat-square&label=Discord)](https://discord.gg/eUMSXGZ)  

An asynchronous .NET Standard 2.0 command framework.   
  
Unlike other bundled command frameworks, Qmmands is completely platform-independent and can be used with any input source, whether that be Discord messages, IRC, or a terminal. This makes Qmmands very powerful, but many platform-related entities like type parsers and execution contexts have to be implemented by the consumer.

Inspired by [Discord.Net.Commands](https://github.com/RogueException/Discord.Net/tree/dev/src/Discord.Net.Commands) and [DSharpPlus.CommandsNext](https://github.com/DSharpPlus/DSharpPlus/tree/master/DSharpPlus.CommandsNext).

Qmmands can be pulled from NuGet. For nightly builds add `https://www.myget.org/F/qmmands/api/v3/index.json` (the nightly feed) to your project's package sources.

## Features
- Commands are asynchronous C# methods, with advanced parameter parsing support (including optional parameters)
- Rich command metadata with including data attributes and support for custom attributes
- Custom type parsers, with replaceable type parsers for language primitives built-in (`string`, `int`, `bool`, + more)
- Advanced command node trees, with support for overloads, command groups and module structures
- Support for automatically discovering command modules with reflection
- Support for adding custom modules and commands at runtime with `ModuleBuilder`
- Advanced post-execution handling with diverse result types
- Built-in (optional) command cooldown system with support for custom cooldown keys and types
- Support for asynchronous checks at the command, module and parameter level


## Documentation
There's currently no official documentation for Qmmands other than the usage examples below and the bundled XML docstrings. For support you should hop in my Discord guild:

[![The Lab](https://discordapp.com/api/guilds/416256456505950215/embed.png?style=banner2)](https://discord.gg/eUMSXGZ)


### Community Examples:
* [Kiritsu](https://github.com/Kiritsu)'s Discord bot: [FoxBot](https://github.com/Kiritsu/FoxBot) (DSharpPlus)
* [idolcoder](https://github.com/idolcoder)'s Discord bot: [Ichigo](https://github.com/idolcoder/Ichigo) (Discord.Net)

### A Simple Example
**CommandHandler.cs**
```cs
private readonly CommandService _service = new CommandService();

// Imagine this being a message callback, whether it'd be from an IRC bot,
// a Discord bot, or any other chat based commands bot.
private async Task MessageReceivedAsync(Message message)
{
    if (!CommandUtilities.HasPrefix(message.Content, '!', out string output))
        return;
        
    IResult result = await _service.ExecuteAsync(output, new CommandContext(message));
    if (!result.IsSuccessful)
        await message.Channel.SendMessageAsync((result as FailedResult).Reason); 
}
```
**CommandContext.cs**
```cs
public sealed class CommandContext : ICommandContext
{
    public Message Message { get; }
    
    public Channel Channel => Message.Channel;
  
    public CommandContext(Message message)
      => Message = message;
}
```
**CommandModule.cs**
```cs
public sealed class CommandModule : ModuleBase<CommandContext>
{
    public CommandService Service { get; set; }

    // Invoked with:   !help
    // Responds with:  `help` - Lists available commands.
    //                 `sum` - Sums two given numbers.
    //                 `echo` - Echoes given text.
    [Command("help", "commands")]
    [Description("Lists available commands.")]
    public Task HelpAsync()
        => Context.Channel.SendMessageAsync(
            string.Join('\n', Service.GetAllCommands().Select(x => $"`{x.Name}` - {x.Description}")));

    // Invoked with:  !sum 3 5
    // Responds with: 3 + 5 = 8
    [Command("sum")]
    [Description("Sums two given numbers.")]
    public Task SumAsync(int firstNumber, int secondNumber)
      => Context.Channel.SendMessageAsync(
          $"{firstNumber} + {secondNumber} = {firstNumber + secondNumber}");

    // Invoked with:  !echo Hello, world.
    // Responds with: Hello, world.
    [Command("echo")]
    [Description("Echoes given text.")]
    public Task EchoAsync([Remainder] string text)
      => Context.Channel.SendMessageAsync(text);
}
```
