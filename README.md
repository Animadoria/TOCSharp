# TOCSharp

Quick TOC2 client I made. Works on [the NINA server](https://nina.chat). 

Has a command system inspired by DSharpPlus/Discord.NET.

## Quick Start
Create an instance of `TOCClient`. Subscribe to events, and do `ConnectAsync()`. Note that `ConnectAsync` is non blocking and starts in a new thread!

## Command System
You can subscribe to the `IMReceived` and `ChatMessageReceived` events and handle it manually, or use the Command System. 

To use the Command System, use `client.UseCommands(CommandSystemSettings)`:
```cs
CommandsSystem cmds = client.UseCommands(new CommandsSystemSettings()
  {
      StringPrefixes = new[] { "/" }
  });
```

Create a class that implements `ICommandModule`, and make a public command method with the `[Command]` attribute, with the parameter for the name(s) for that command. You must have at least *one* argument, CommandContext. The other arguments will be parsed automatically.

```cs
public class HelloCommand : ICommandModule
{
    [Command("hello")]
    public async Task Hello(CommandContext ctx, [RemainingText] string text)
    {
        await ctx.ReplyAsync($"Hello, {text}!");
    }
}
```

Don't forget to register it!
```cs
cmds.RegisterCommands(new HelloCommand());
```