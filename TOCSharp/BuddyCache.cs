using System.Collections.Concurrent;
using System.Collections.Generic;
using TOCSharp.Models;

namespace TOCSharp
{
    /// <summary>
    /// Internal buddy cache
    /// </summary>
    internal static class BuddyCache
    {
        /// <summary>
        /// Collection to store buddy infos
        /// </summary>
        private static readonly ConcurrentDictionary<string, BuddyInfo> buddyInfos = new ConcurrentDictionary<string, BuddyInfo>();

        /// <summary>
        /// Adds or updates a buddy
        /// </summary>
        /// <param name="buddyInfo">Buddy</param>
        internal static void AddOrUpdate(BuddyInfo buddyInfo)
        {
            string normalizedScreenname = Utils.NormalizeScreenname(buddyInfo.Screenname);


            if (buddyInfos.TryGetValue(normalizedScreenname, out BuddyInfo? bi))
            {
                bi.Alias = buddyInfo.Alias;
                bi.Class = buddyInfo.Class;
                bi.Evil = buddyInfo.Evil;
                bi.Online = buddyInfo.Online;
                bi.IdleTime = buddyInfo.IdleTime;
                bi.SignonTime = buddyInfo.SignonTime;
            }
            else
            {
                buddyInfos[normalizedScreenname] = buddyInfo;
            }
        }

        /// <summary>
        /// Gets a buddy
        /// </summary>
        /// <param name="screenname">Screenname to search</param>
        /// <returns>Null if we don't have it in cache, BuddyInfo if we do</returns>
        internal static BuddyInfo? Get(string screenname)
        {
            return buddyInfos.GetValueOrDefault(Utils.NormalizeScreenname(screenname));
        }
    }
}