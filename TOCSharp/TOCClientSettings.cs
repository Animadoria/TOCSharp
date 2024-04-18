using System;

namespace TOCSharp
{
    /// <summary>
    /// TOC Client settings
    /// </summary>
    public class TOCClientSettings
    {
        /// <summary>
        /// Hostname of the TOC server
        /// </summary>
        public string Hostname { get; set; } = FLAPConnection.DEFAULT_HOST;

        /// <summary>
        /// Port of the TOC server
        /// </summary>
        public ushort Port { get; set; } = FLAPConnection.DEFAULT_PORT;

        /// <summary>
        /// Client name
        /// </summary>
        public string ClientName { get; set; } = "TOCSharp";

        /// <summary>
        /// Wants console output for debugging
        /// </summary>
        public bool DebugMode { get; set; } = false;

        /// <summary>
        /// Keep-alive FLAP interval
        /// </summary>
        public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(120);
    }
}