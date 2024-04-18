using System;

namespace TOCSharp.Models
{
    /// <summary>
    /// Buddy info
    /// </summary>
    public class BuddyInfo
    {
        /// <summary>
        /// Buddy screenname
        /// </summary>
        public string Screenname { get; }

        /// <summary>
        /// Buddy alias. Might be null.
        /// </summary>
        public string? Alias { get; internal set; }

        /// <summary>
        /// If the buddy is online
        /// </summary>
        public bool Online { get; internal set; }

        /// <summary>
        /// Buddy's evil level
        /// </summary>
        public int Evil { get; internal set; }

        /// <summary>
        /// Sign on time for the buddy
        /// </summary>
        public DateTimeOffset SignonTime { get; internal set; }

        /// <summary>
        /// Idle time for the buddy. 0 means buddy is not idle
        /// </summary>
        public int IdleTime { get; internal set; }

        /// <summary>
        /// Buddy class
        /// </summary>
        public UserClass Class { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="screenname"></param>
        public BuddyInfo(string screenname)
        {
            this.Screenname = screenname;
        }

        /// <summary>
        /// Get hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return this.Screenname.GetHashCode();
        }
    }

    /// <summary>
    /// Buddy class
    /// </summary>
    [Flags]
    public enum UserClass
    {
        /// <summary>
        /// No class
        /// </summary>
        None = 0x00,

        /// <summary>
        /// User is on AOL
        /// </summary>
        OnAOL = 0x01,

        /// <summary>
        /// User is an OSCAR admin
        /// </summary>
        OSCARAdmin = 0x02,

        /// <summary>
        /// User is unconfirmed / trial
        /// </summary>
        OSCARUnconfirmed = 0x04,

        /// <summary>
        /// Normal user
        /// </summary>
        OSCARNormal = 0x08,

        /// <summary>
        /// Unavailable/away
        /// </summary>
        Unavailable = 0x10
    }
}