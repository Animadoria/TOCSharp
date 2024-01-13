using System;
using System.Buffers.Binary;
using System.Linq;

namespace TOCSharp
{
    internal static class ByteTools
    {
        internal static byte[] PackUInt16(ushort value)
        {
            byte[] bytes = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(bytes, value);
            return bytes;
        }

        internal static byte[] Concatenate(params byte[][] bytes)
        {
            int length = bytes.Sum(b => b.Length);

            byte[] result = new byte[length];
            int offset = 0;
            foreach (byte[] b in bytes)
            {
                Buffer.BlockCopy(b, 0, result, offset, b.Length);
                offset += b.Length;
            }

            return result;
        }

        public static string ToHexString(byte[] byteArray)
        {
            return BitConverter.ToString(byteArray).Replace("-", "");
        }
    }
}