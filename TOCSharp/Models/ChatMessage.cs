namespace TOCSharp.Models;

public class ChatMessage
{
    public required string RoomID;
    public required string Sender;
    public required bool Whisper;
    public required string Message;
}