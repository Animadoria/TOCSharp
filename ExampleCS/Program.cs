using System.Globalization;
using TOCSharp;
using TOCSharp.Commands;
using TOCSharp.Models;

namespace ExampleCS;

internal static class Program
{
    private static TOCClient client = null!;
    private static string format = "";

    public static async Task Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        client = new TOCClient("SCREENNAME", "PASSWORD");
        format = client.Screenname;

        client.SignOnDone += SignOnDone;
        client.ChatBuddyUpdate += ClientOnChatBuddyUpdate;

        client.NickReceived += (sender, s) =>
        {
            format = s;
            return Task.CompletedTask;
        };

        CommandsSystem cmds = client.UseCommands(new CommandsSystemSettings()
        {
            StringPrefixes = new[] { "/", "!", "?", ".", "---" }
        });
        cmds.RegisterCommands(new RainbowCommand());

        await client.ConnectAsync();
        await Task.Delay(-1);
    }

    private static async Task ClientOnChatBuddyUpdate(object sender, ChatBuddyUpdate args)
    {
        List<string> buddies = args.Buddies.ToList();
        buddies.Remove(format);
        if (args.IsOnline)
        {
            await client.SendChatMessageAsync(args.RoomID, $"Hey, {string.Join(", ", buddies)}!");
        }
        else
        {
            await client.SendChatMessageAsync(args.RoomID, $"Bye, {string.Join(", ", buddies)}!");
        }
    }

    private static async Task SignOnDone(object sender, EventArgs args)
    {
        await client.JoinChatAsync("lastcall");
    }
}