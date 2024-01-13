namespace TOCSharp.Models;

public class BuddyInfo
{
    public required string Screenname;
    public required bool Online;
    public required int Evil;
    public required DateTimeOffset SignonTime;
    public required int IdleTime;
    public required UserClass Class;
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