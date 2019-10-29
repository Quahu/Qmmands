# Qmmands
[![Build Status](https://img.shields.io/appveyor/ci/Quahu/qmmands.svg?style=flat-square)](https://ci.appveyor.com/project/Quahu/qmmands)
[![NuGet](https://img.shields.io/nuget/v/Qmmands.svg?style=flat-square)](https://www.nuget.org/packages/Qmmands/)
[![MyGet](https://img.shields.io/myget/quahu/vpre/Qmmands.svg?style=flat-square&label=myget)](https://www.myget.org/feed/quahu/package/nuget/Qmmands)
[![The Lab](https://img.shields.io/discord/416256456505950215.svg?style=flat-square&label=discord)](https://discord.gg/eUMSXGZ)  

An asynchronous platform-independent .NET Core 2.1-2.2 command framework that can be used with any input source, whether that be Discord messages, IRC, or a terminal. 

Inspired by [Discord.Net.Commands](https://github.com/RogueException/Discord.Net/tree/dev/src/Discord.Net.Commands) and [DSharpPlus.CommandsNext](https://github.com/DSharpPlus/DSharpPlus/tree/master/DSharpPlus.CommandsNext).


## Installing
Stable Qmmands builds can be pulled from NuGet.
For nightly builds add `https://www.myget.org/F/quahu/api/v3/index.json` (the nightly feed) to your project's package sources and pull from there instead.


## Documentation
There's currently no official documentation for Qmmands other than the community projects below and the bundled XML docstrings. For support you should hop in my Discord guild:

[![The Lab](https://discordapp.com/api/guilds/416256456505950215/embed.png?style=banner2)](https://discord.gg/eUMSXGZ)


### Community Projects:
* [k-boyle](https://github.com/k-boyle)'s Discord bot: [Espeon](https://github.com/k-boyle/Espeon) (Disqord)
* [BlowaXD](https://github.com/BlowaXD)'s Nostale Server Emulator: [SaltyEmu](https://github.com/BlowaXD/SaltyEmu) 

### A Simple Usage Example
**CommandHandler.cs**
```cs
private readonly CommandService _service = new CommandService();

public void Setup()
    => _service.AddModule<CommandModule>();

// Imagine this being a message callback, whether it be from an IRC bot,
// a Discord bot, or any other chat based service.
private async Task MessageReceivedAsync(Message message)
{
    if (!CommandUtilities.HasPrefix(message.Content, '!', out string output))
        return;
        
    IResult result = await _service.ExecuteAsync(output, new CustomCommandContext(message));
    if (result is FailedResult failedResult)
        await message.Channel.SendMessageAsync(failedResult.Reason); 
}
```
**CustomCommandContext.cs**
```cs
public sealed class CustomCommandContext : CommandContext
{
    public Message Message { get; }
    
    public Channel Channel => Message.Channel;
  
    // Pass your service provider to the base command context.
    public CustomCommandContext(Message message, IServiceProvider provider = null) : base(provider)
    {
        Message = message;
    }
}
```
**CommandModule.cs**
```cs
public sealed class CommandModule : ModuleBase<CustomCommandContext>
{
    // Dependency Injection via the constructor or public settable properties.
    // CommandService and IServiceProvider self-inject into modules,
    // properties and other types are requested from the provided IServiceProvider
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
