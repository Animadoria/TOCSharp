using System.Threading.Tasks;
using TOCSharp.Models;

namespace TOCSharp.Commands
{
    /// <summary>
    /// Command context
    /// </summary>
    public class CommandContext
    {
        /// <summary>
        /// Sender of the message
        /// </summary>
        public BuddyInfo Sender { get; set; }

        /// <summary>
        /// Chat Room the message was sent in
        /// <remarks>
        /// This is null if message went sent as an IM.
        /// </remarks>
        /// </summary>
        public ChatRoom? ChatRoom { get; set; }

        /// <summary>
        /// If message was a whisper in a chat (implies ChatRoom is not null)
        /// </summary>
        public bool IsWhisper { get; set; }

        /// <summary>
        /// Message content
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Prefix used
        /// </summary>
        public string Prefix { get; set; } = "";

        /// <summary>
        /// Underlying commands system
        /// </summary>
        public CommandsSystem CommandsSystem { get; set; }

        internal CommandContext(BuddyInfo sender, ChatRoom? chatRoom, bool isWhisper, string message, CommandsSystem commandsSystem)
        {
            this.Sender = sender;
            this.CommandsSystem = commandsSystem;
            this.ChatRoom = chatRoom;
            this.IsWhisper = isWhisper;
            this.Message = message;
        }

        /// <summary>
        /// Replies to the sender
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="isWhisper">Whether to respond with a whisper (if in chat room)</param>
        public async Task ReplyAsync(string message, bool isWhisper = false)
        {
            if (this.ChatRoom != null)
            {
                string? whisperTarget = isWhisper ? this.Sender.Screenname : null;
                string[]? split = message.Split("\n");
                for (int i = 0; i < split.Length; i++)
                {
                    string str = split[i];

                    await this.CommandsSystem.Client.SendChatMessageAsync(this.ChatRoom, str, whisperTarget);
                    if (i != split.Length - 1)
                    {
                        await Task.Delay(1000);
                    }
                }
            }
            else
            {
                await this.CommandsSystem.Client.SendIMAsync(this.Sender, message);
            }
        }
    }
}