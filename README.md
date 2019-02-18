# Qmmands
[![Build Status](https://img.shields.io/appveyor/ci/Quahu/qmmands.svg?style=flat-square)](https://ci.appveyor.com/project/Quahu/qmmands)
[![NuGet](https://img.shields.io/nuget/v/Qmmands.svg?style=flat-square)](https://www.nuget.org/packages/Qmmands/)
[![MyGet](https://img.shields.io/myget/qmmands/vpre/Qmmands.svg?style=flat-square&label=myget)](https://www.myget.org/gallery/qmmands)
[![The Lab](https://img.shields.io/discord/416256456505950215.svg?style=flat-square&label=discord)](https://discord.gg/eUMSXGZ)  

An asynchronous platform-independent .NET Standard 2.0 command framework that can be used with any input source, whether that be Discord messages, IRC, or a terminal. 

Inspired by [Discord.Net.Commands](https://github.com/RogueException/Discord.Net/tree/dev/src/Discord.Net.Commands) and [DSharpPlus.CommandsNext](https://github.com/DSharpPlus/DSharpPlus/tree/master/DSharpPlus.CommandsNext).

Qmmands can be pulled from NuGet. For nightly builds add `https://www.myget.org/F/qmmands/api/v3/index.json` (the nightly feed) to your project's package sources.

## Key Features
- Advanced parameter parsing support (including custom type parsers, optional parameters, and remainder support)
- Support for returning custom `CommandResult` implementations for advanced post-execution handling
- Command module discovery via assembly crawling for valid types
- Support for adding custom modules and commands at runtime with builders or types
- Built-in (optional) command cooldown system with support for custom cooldown types


## Documentation
There's currently no official documentation for Qmmands other than the usage examples below and the bundled XML docstrings. For support you should hop in my Discord guild:

[![The Lab](https://discordapp.com/api/guilds/416256456505950215/embed.png?style=banner2)](https://discord.gg/eUMSXGZ)


### Community Examples:
* [Kiritsu](https://github.com/Kiritsu)'s Discord bot: [FoxBot](https://github.com/Kiritsu/FoxBot) (DSharpPlus)
* [GreemDev](https://github.com/GreemDev)'s Discord bot: [Volte](https://github.com/GreemDev/Volte) (Discord.Net)

### A Simple Usage Example
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
    if (result is FailedResult failedResult)
        await message.Channel.SendMessageAsync(failedResult.Reason); 
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
