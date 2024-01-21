namespace TOCSharp.Models
{
    public class InstantMessage
    {
        public BuddyInfo Sender { get; }
        public string Message { get; }
        public bool AutoResponse { get; }

        public InstantMessage(BuddyInfo sender, string message, bool autoResponse)
        {
            this.Sender = sender;
            this.AutoResponse = autoResponse;
            this.Message = message;
        }
    }
}