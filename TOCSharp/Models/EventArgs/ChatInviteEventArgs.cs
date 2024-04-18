namespace TOCSharp.Models.EventArgs
{
    /// <summary>
    /// Chat invite event args
    /// </summary>
    public readonly struct ChatInviteEventArgs
    {
        /// <summary>
        /// Room name
        /// </summary>
        public readonly string RoomName;
        /// <summary>
        /// Room ID
        /// </summary>
        public readonly string RoomID;
        /// <summary>
        /// Sender of invitation
        /// </summary>
        public readonly BuddyInfo Sender;
        /// <summary>
        /// Sender message
        /// </summary>
        public readonly string Message;

        internal ChatInviteEventArgs(string roomName, string roomID, BuddyInfo sender, string message)
        {
            this.RoomName = roomName;
            this.RoomID = roomID;
            this.Sender = sender;
            this.Message = message;
        }
    }
}