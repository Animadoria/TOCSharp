using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TOCSharp
{
    public class FLAPConnection
    {
        public const string DEFAULT_HOST = "testpea-n01a.blue.nina.chat";
        public const ushort DEFAULT_PORT = 9898;

        public static readonly byte[] FLAPON = Encoding.UTF8.GetBytes("FLAPON\r\n\r\n");

        private readonly string host;
        private readonly ushort port;

        public bool Connected { get; private set; }
        public AsyncEventHandler<FLAPPacket>? PacketReceived;

        private readonly Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

        private byte[] buffer = Array.Empty<byte>();

        private ushort seqNo = (ushort)new Random().Next(ushort.MaxValue);

        public FLAPConnection(string host = DEFAULT_HOST, ushort port = DEFAULT_PORT)
        {
            this.host = host;
            this.port = port;
        }

        public async Task ConnectAsync()
        {
            await this.socket.ConnectAsync(new DnsEndPoint(this.host, this.port));
            await this.socket.SendAsync(FLAPON, SocketFlags.None);
            this.Connected = true;

            _ = this.FLAPActivity();
        }

        public Task DisconnectAsync()
        {
            this.socket.Disconnect(false);
            return Task.CompletedTask;
        }

        private async Task FLAPActivity()
        {
            while (this.Connected)
            {
                try
                {
                    byte[] bytes = new byte[2048];
                    int bytesReceived = await this.socket.ReceiveAsync(bytes, SocketFlags.None);

                    if (bytesReceived <= 0)
                    {
                        break;
                    }
                    bytes = bytes[..bytesReceived];

                    this.buffer = this.buffer.Length != 0 ? ByteTools.Concatenate(this.buffer, bytes) : bytes;

                    while (true)
                    {
                        if (this.buffer.Length < 6)
                        {
                            break;
                        }

                        byte marker = this.buffer[0];
                        if (marker != 0x2A)
                        {
                            break;
                        }

                        byte frame = this.buffer[1];
                        ushort sequence = BinaryPrimitives.ReadUInt16BigEndian(this.buffer.AsSpan()[2..4]);
                        ushort length = BinaryPrimitives.ReadUInt16BigEndian(this.buffer.AsSpan()[4..6]);

                        if (this.buffer.Length < length + 6)
                        {
                            continue;
                        }

                        byte[] data = this.buffer[6..(length + 6)];

                        FLAPPacket packet = new FLAPPacket(marker, frame, sequence, length, data);

                        if (this.PacketReceived != null)
                        {
                            await this.PacketReceived(this, packet);
                        }

                        this.buffer = this.buffer[(length + 6)..];
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }

            this.Connected = false;
        }

        public async Task SendPacketAsync(FLAPPacket packet)
        {
            packet.Sequence = this.seqNo++;

            await this.socket.SendAsync(packet.ToBytes(), SocketFlags.None);
        }
    }
}