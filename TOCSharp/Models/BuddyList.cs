using System;
using System.Collections.Generic;

namespace TOCSharp.Models
{
    public class BuddyList
    {
        public Dictionary<string, BuddyInfo[]> Buddies { get; } = new Dictionary<string, BuddyInfo[]>();
        public BuddyInfo[] PermitList { get; internal set; } = Array.Empty<BuddyInfo>();
        public BuddyInfo[] DenyList { get; internal set; } = Array.Empty<BuddyInfo>();
        public PDMode PDMode { get; internal set; }
    }

    public enum PDMode
    {
        PermitAll = 0x01,
        DenyAll = 0x02,
        PermitSome = 0x03,
        DenySome = 0x04,
        PermitOnList = 0x05,
    }
}