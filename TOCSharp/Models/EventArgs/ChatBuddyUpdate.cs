namespace TOCSharp.Models
{
    public struct ChatBuddyUpdate
    {
        public ChatRoom Room;
        public bool IsOnline;
        public BuddyInfo[] Buddies;
    }
}