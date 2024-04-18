namespace TOCSharp.Models.EventArgs
{
    /// <summary>
    /// Evil event args
    /// </summary>
    public class EvilEventArgs
    {
        /// <summary>
        /// New evil
        /// </summary>
        public int NewEvil { get; }

        /// <summary>
        /// Eviler. Null if it was an anonymous eviler
        /// </summary>
        public BuddyInfo? Eviler { get; }

        internal EvilEventArgs(int newEvil, BuddyInfo? eviler)
        {
            this.NewEvil = newEvil;
            this.Eviler = eviler;
        }
    }
}