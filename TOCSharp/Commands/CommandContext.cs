namespace TOCSharp.Commands;

public class CommandContext
{
    public required bool IsChat { get; init; }
    public required bool IsWhisper { get; init; }
    public required string Sender { get; init; }
    public required string ChatRoomID { get; init; }
    public required string Message { get; init; }
    public string Prefix { get; set; } = "";
    public required CommandsSystem CommandsSystem { get; init; }

    public async Task ReplyAsync(string message, string? toWhisper = null)
    {
        if (this.IsChat)
        {
            var split = message.Split("\n");
            for (int i = 0; i < split.Length; i++)
            {
                string? str = split[i];

                await this.CommandsSystem.Client.SendChatMessageAsync(this.ChatRoomID, str, toWhisper);
                if (i != split.Length - 1)
                {
                    await Task.Delay(1000);
                }
            }
        }
        else
        {
            await this.CommandsSystem.Client.SendIM(message, this.Sender);
        }
    }
}