namespace TOCSharp
{
    public class TOCClientSettings
    {
        public string Hostname { get; set; } = FLAPConnection.DEFAULT_HOST;
        public ushort Port { get; set; } = FLAPConnection.DEFAULT_PORT;
        public string ClientName { get; set; } = "TOCSharp";
        public bool DebugMode { get; set; } = false;
    }
}