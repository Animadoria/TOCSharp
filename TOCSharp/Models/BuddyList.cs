using System;
using System.Collections.Generic;

namespace TOCSharp.Models
{
    /// <summary>
    /// List of buddies
    /// </summary>
    public class BuddyList
    {
        /// <summary>
        /// Dictionary of buddies by group
        /// </summary>
        public Dictionary<string, BuddyInfo[]> Buddies { get; } = new Dictionary<string, BuddyInfo[]>();

        /// <summary>
        /// Permit list
        /// </summary>
        public BuddyInfo[] PermitList { get; internal set; } = Array.Empty<BuddyInfo>();

        /// <summary>
        /// Deny list
        /// </summary>
        public BuddyInfo[] DenyList { get; internal set; } = Array.Empty<BuddyInfo>();

        /// <summary>
        /// PD mide
        /// </summary>
        public PDMode PDMode { get; internal set; }
    }

    /// <summary>
    /// Permit/Deny Mode
    /// </summary>
    public enum PDMode
    {
        /// <summary>
        /// Permit all buddies
        /// </summary>
        PermitAll = 0x01,

        /// <summary>
        /// Deny all buddies
        /// </summary>
        DenyAll = 0x02,

        /// <summary>
        /// Permit some (only on the permit list)
        /// </summary>
        PermitSome = 0x03,

        /// <summary>
        /// Deny some (only on the deny list)
        /// </summary>
        DenySome = 0x04,

        /// <summary>
        /// Permit all buddies on buddy list
        /// </summary>
        PermitOnList = 0x05,
    }
}