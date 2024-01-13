using System.Runtime.InteropServices;

namespace NativeShim;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct IM
{
    public uint HClient;
    public string? Sender;
    public string? Message;
}

public delegate void IMRecv(IM im);