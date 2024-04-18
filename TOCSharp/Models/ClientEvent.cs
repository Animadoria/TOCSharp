namespace TOCSharp.Models
{
    /// <summary>
    /// Client event
    /// </summary>
    public class ClientEvent
    {
        /// <summary>
        /// Sender of the event
        /// </summary>
        public BuddyInfo Sender { get; }

        /// <summary>
        /// Event type
        /// </summary>
        public ClientEventType EventType { get; }

        internal ClientEvent(BuddyInfo sender, ClientEventType eventType)
        {
            this.Sender = sender;
            this.EventType = eventType;
        }
    }

    /// <summary>
    /// Client Events
    /// </summary>
    public enum ClientEventType
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Was typing, but stopped
        /// </summary>
        Typed = 0x01,

        /// <summary>
        /// Is typing
        /// </summary>
        Typing = 0x02,
    }
}