using System;
using System.Buffers.Binary;

namespace TOCSharp
{
    public class FLAPPacket
    {
        public const byte FLAP_MARKER = 0x2A;
        public const byte FRAME_SIGNON = 0x01;
        public const byte FRAME_DATA = 0x02;
        public const byte FRAME_ERROR = 0x03;
        public const byte FRAME_SIGNOFF = 0x04;
        public const byte FRAME_KEEPALIVE = 0x05;

        public byte Marker = FLAP_MARKER;
        public byte Frame;
        public ushort Sequence;
        public ushort Length;
        public byte[] Data = Array.Empty<byte>();

        public FLAPPacket()
        {
        }

        public FLAPPacket(byte marker, byte frame, ushort sequence, ushort length, byte[] data)
        {
            this.Marker = marker;
            this.Frame = frame;
            this.Sequence = sequence;
            this.Length = length;
            this.Data = data;
        }

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