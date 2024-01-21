using System;

namespace TOCSharp.Models
{
    public class BuddyInfo
    {
        public string Screenname { get; }
        public string? Alias { get; internal set; }
        public bool Online { get; internal set; }
        public int Evil { get; internal set; }
        public DateTimeOffset SignonTime { get; internal set; }
        public int IdleTime { get; internal set; }
        public UserClass Class { get; internal set; }

        public BuddyInfo(string screenname)
        {
            this.Screenname = screenname;
        }


    }

    [Flags]
    public enum UserClass
    {
        None = 0x00,
        OnAOL = 0x01,
        OSCARAdmin = 0x02,
        OSCARUnconfirmed = 0x04,
        OSCARNormal = 0x08,
        Unavailable = 0x10
    }
}