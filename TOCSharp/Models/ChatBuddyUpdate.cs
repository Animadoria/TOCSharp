namespace TOCSharp.Models;

public class ChatBuddyUpdate
{
    public required string RoomID;
    public required bool IsOnline;
    public required string[] Buddies;
}