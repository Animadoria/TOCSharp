namespace TOCSharp.Models.EventArgs
{
    /// <summary>
    /// Chat buddy update event arguments
    /// </summary>
    public readonly struct ChatBuddyUpdate
    {
        /// <summary>
        /// Room of the update
        /// </summary>
        public readonly ChatRoom Room;

        /// <summary>
        /// Buddies in the list are now online
        /// </summary>
        public readonly bool IsOnline;

        /// <summary>
        /// List of buddies
        /// </summary>
        public readonly BuddyInfo[] Buddies;

        internal ChatBuddyUpdate(ChatRoom room, bool isOnline, BuddyInfo[] buddies)
        {
            this.Room = room;
            this.IsOnline = isOnline;
            this.Buddies = buddies;
        }
    }
}