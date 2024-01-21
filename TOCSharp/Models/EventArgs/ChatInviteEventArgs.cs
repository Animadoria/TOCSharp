namespace TOCSharp.Models
{
    public struct ChatInviteEventArgs
    {
        public string RoomName;
        public string RoomID;
        public BuddyInfo Sender;
        public string Message;
    }
}