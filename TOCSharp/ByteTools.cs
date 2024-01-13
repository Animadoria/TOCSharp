using System.Buffers.Binary;

namespace TOCSharp;

internal static class ByteTools
{
    internal static byte[] PackUInt16(ushort value)
    {
        byte[] bytes = new byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(bytes, value);
        return bytes;
    }
}