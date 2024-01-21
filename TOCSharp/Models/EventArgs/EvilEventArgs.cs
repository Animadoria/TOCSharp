namespace TOCSharp.Models.EventArgs
{
    public class EvilEventArgs
    {
        public int NewEvil { get; }
        public BuddyInfo? Eviler { get; }

        public EvilEventArgs(int newEvil, BuddyInfo? eviler)
        {
            this.NewEvil = newEvil;
            this.Eviler = eviler;
        }
    }
}