namespace TOCSharp.Models
{
    /// <summary>
    /// Chat Message
    /// </summary>
    public readonly struct ChatMessage
    {
        /// <summary>
        /// Chat room where message was sent
        /// </summary>
        public readonly ChatRoom Room;

        /// <summary>
        /// Sender of the message
        /// </summary>
        public readonly BuddyInfo Sender;

        /// <summary>
        /// Message is a whisper
        /// </summary>
        public readonly bool Whisper;

        /// <summary>
        /// Message content
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="room"></param>
        /// <param name="sender"></param>
        /// <param name="whisper"></param>
        /// <param name="message"></param>
        internal ChatMessage(ChatRoom room, BuddyInfo sender, bool whisper, string message)
        {
            this.Room = room;
            this.Sender = sender;
            this.Whisper = whisper;
            this.Message = message;
        }
    }
}