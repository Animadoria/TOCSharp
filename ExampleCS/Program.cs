using System.Globalization;
using TOCSharp;
using TOCSharp.Commands;
using TOCSharp.Models;

namespace ExampleCS;

internal static class Program
{
    private static TOCClient client = null!;

    public static async Task Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        client = new TOCClient("SCREENNAME", "PASSWORD");

        client.SignOnDone += SignOnDone;
        client.ChatBuddyUpdate += ClientOnChatBuddyUpdate;

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
        List<BuddyInfo> buddies = args.Buddies.Where(x => x.Screenname != client.Format).ToList();
        if (args.IsOnline)
        {
            await client.SendChatMessageAsync(args.Room, $"Hey, {string.Join(", ", buddies)}!");
        }
        else
        {
            await client.SendChatMessageAsync(args.Room, $"Bye, {string.Join(", ", buddies)}!");
        }
    }

    private static async Task SignOnDone(object sender, EventArgs args)
    {
        await client.JoinChatAsync("lastcall");
    }
}