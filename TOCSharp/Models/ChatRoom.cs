using System.Collections.Generic;

namespace TOCSharp.Models
{
    /// <summary>
    /// Chat room
    /// </summary>
    public class ChatRoom
    {
        /// <summary>
        /// Chat name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Chat ID
        /// </summary>
        public string ChatID { get; set; }

        /// <summary>
        /// List of users in the chat
        /// </summary>
        public HashSet<BuddyInfo> Users { get; } = new HashSet<BuddyInfo>();

        internal ChatRoom(string name, string chatID)
        {
            this.Name = name;
            this.ChatID = chatID;
        }


    }
}