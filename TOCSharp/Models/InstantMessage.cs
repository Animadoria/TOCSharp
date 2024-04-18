namespace TOCSharp.Models
{
    /// <summary>
    /// Instant message
    /// </summary>
    public class InstantMessage
    {
        /// <summary>
        /// Sender of the message
        /// </summary>
        public BuddyInfo Sender { get; }

        /// <summary>
        /// Message contents
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Is auto response
        /// </summary>
        public bool AutoResponse { get; }

        internal InstantMessage(BuddyInfo sender, string message, bool autoResponse)
        {
            this.Sender = sender;
            this.AutoResponse = autoResponse;
            this.Message = message;
        }
    }
}