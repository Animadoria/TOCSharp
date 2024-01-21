using System;
using System.Buffers.Binary;
using System.Linq;

namespace TOCSharp
{
    /// <summary>
    /// Utilities for working with bytes.
    /// </summary>
    internal static class ByteTools
    {
        /// <summary>
        /// Packs a UInt16, big endian
        /// </summary>
        /// <param name="value">UInt16 value to pack</param>
        /// <returns>byte[] of uint16</returns>
        internal static byte[] PackUInt16(ushort value)
        {
            byte[] bytes = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(bytes, value);
            return bytes;
        }

        /// <summary>
        /// Concatenates byte arrays
        /// </summary>
        /// <param name="bytes">Arrays to concatenate</param>
        /// <returns>Concatenated byte array</returns>
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

        /// <summary>
        /// Converts a byte[] to hex string, uppercase
        /// </summary>
        /// <param name="byteArray">Byte[] to convert</param>
        /// <returns>Hex string</returns>
        public static string ToHexString(byte[] byteArray)
        {
            return BitConverter.ToString(byteArray).Replace("-", "");
        }
    }
}