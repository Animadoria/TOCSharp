namespace TOCSharp.Models
{
    public class ClientEvent
    {
        public BuddyInfo Sender { get; }
        public ClientEventType EventType { get; }

        public ClientEvent(BuddyInfo sender, ClientEventType eventType)
        {
            this.Sender = sender;
            this.EventType = eventType;
        }
    }

    public enum ClientEventType
    {
        None = 0,
        Typed = 0x01,
        Typing = 0x02,
    }
}