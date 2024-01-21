using System.Collections.Generic;

namespace TOCSharp.Models
{
    public class ChatRoom
    {
        public string Name { get; set; }
        public string ChatID { get; set; }

        public HashSet<BuddyInfo> Users { get; } = new HashSet<BuddyInfo>();

        public ChatRoom(string name, string chatID)
        {
            this.Name = name;
            this.ChatID = chatID;
        }


    }
}