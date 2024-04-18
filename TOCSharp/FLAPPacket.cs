using System;

namespace TOCSharp
{
    /// <summary>
    /// Base FLAP Packet
    /// </summary>
    public class FLAPPacket
    {
        /// <summary>
        /// FLAP Marker, always 0x2A (*)
        /// </summary>
        public const byte FLAP_MARKER = 0x2A;

        /// <summary>
        /// SIGNON frame
        /// </summary>
        public const byte FRAME_SIGNON = 0x01;

        /// <summary>
        /// DATA frame, encapsulates TOC commands
        /// </summary>
        public const byte FRAME_DATA = 0x02;

        /// <summary>
        /// ERROR frame, unused for TOC
        /// </summary>
        public const byte FRAME_ERROR = 0x03;

        /// <summary>
        /// SIGNOFF frame
        /// </summary>
        public const byte FRAME_SIGNOFF = 0x04;

        /// <summary>
        /// KEEPALIVE frame, not required
        /// </summary>
        public const byte FRAME_KEEPALIVE = 0x05;

        /// <summary>
        /// FLAP marker
        /// </summary>
        public readonly byte Marker = FLAP_MARKER;

        /// <summary>
        /// FLAP frame
        /// </summary>
        public byte Frame;

        /// <summary>
        /// FLAP sequence number
        /// </summary>
        public ushort Sequence;

        /// <summary>
        /// FLAP packet length
        /// </summary>
        public ushort Length;

        /// <summary>
        /// FLAP data
        /// </summary>
        public byte[] Data = Array.Empty<byte>();

        /// <summary>
        /// FLAP constructor
        /// </summary>
        public FLAPPacket()
        {
        }

        /// <summary>
        /// FLAP constructor with members
        /// </summary>
        /// <param name="marker">FLAP marker</param>
        /// <param name="frame">FLAP frame</param>
        /// <param name="sequence">FLAP sequence</param>
        /// <param name="length">FLAP data length</param>
        /// <param name="data">FLAP data</param>
        public FLAPPacket(byte marker, byte frame, ushort sequence, ushort length, byte[] data)
        {
            this.Marker = marker;
            this.Frame = frame;
            this.Sequence = sequence;
            this.Length = length;
            this.Data = data;
        }

        /// <summary>
        /// Convert FLAP packet to byte array
        /// </summary>
        /// <returns>Byte array of FLAP</returns>
        public byte[] ToBytes()
        {
            this.Length = (ushort)this.Data.Length;
            return ByteTools.Concatenate(
                new[] { this.Marker, this.Frame },
                ByteTools.PackUInt16(this.Sequence),
                ByteTools.PackUInt16(this.Length),
                this.Data);
        }
    }
}