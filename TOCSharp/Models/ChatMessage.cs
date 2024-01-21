namespace TOCSharp.Models
{
    public struct ChatMessage
    {
        public ChatRoom RoomID;
        public BuddyInfo Sender;
        public bool Whisper;
        public string Message;
    }
}