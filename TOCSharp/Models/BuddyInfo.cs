using System;

namespace TOCSharp.Models
{
    public struct BuddyInfo
    {
        public string Screenname;
        public bool Online;
        public int Evil;
        public DateTimeOffset SignonTime;
        public int IdleTime;
        public UserClass Class;
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