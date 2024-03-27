using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TOCSharp
{
    /// <summary>
    /// Base FLAP connection
    /// </summary>
    public class FLAPConnection
    {
        /// <summary>
        /// Amount of bytes to read
        /// </summary>
        public const int READ_SIZE = 0x800;

        /// <summary>
        /// Default host
        /// </summary>
        public const string DEFAULT_HOST = "testpea-n01a.blue.nina.chat";

        /// <summary>
        /// Default port
        /// </summary>
        public const ushort DEFAULT_PORT = 9898;

        /// <summary>
        /// FLAPON bytes
        /// </summary>
        public static readonly byte[] FLAPON = Encoding.UTF8.GetBytes("FLAPON\r\n\r\n");

        private readonly string host;
        private readonly ushort port;

        public bool Connected { get; private set; }
        public event AsyncEventHandler<FLAPPacket>? PacketReceived;
        public event AsyncEventHandler? Disconnected;

        private Socket? socket;

        /// <summary>
        /// Previous buffer
        /// </summary>
        private byte[] buffer = Array.Empty<byte>();

        /// <summary>
        /// Sequence number
        /// </summary>
        private ushort seqNo = (ushort)new Random().Next(ushort.MaxValue);

        /// <summary>
        /// FLAP connection constructor
        /// </summary>
        /// <param name="host">Host to connect to</param>
        /// <param name="port">Port to connect to</param>
        internal FLAPConnection(string host = DEFAULT_HOST, ushort port = DEFAULT_PORT)
        {
            this.host = host;
            this.port = port;
        }

        /// <summary>
        /// Connect FLAP connection
        /// </summary>
        public async Task ConnectAsync()
        {
            this.socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            await this.socket.ConnectAsync(new DnsEndPoint(this.host, this.port));
            await this.socket.SendAsync(FLAPON, SocketFlags.None);
            this.Connected = true;

            _ = Task.Factory.StartNew(this.FLAPActivity, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Disconnect FLAP connection
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (!this.Connected || this.socket == null) return;
            this.socket.Disconnect(false);

            await this.OnDisconnected();
        }

        /// <summary>
        /// On FLAP activity
        /// </summary>
        private async Task FLAPActivity()
        {
            while (this.Connected)
            {
                byte[]? pooled = null;
                try
                {
                    pooled = ArrayPool<byte>.Shared.Rent(READ_SIZE);
                    int bytesReceived = await this.socket.ReceiveAsync(pooled, SocketFlags.None);

                    if (bytesReceived <= 0)
                    {
                        break;
                    }
                    var bytes = pooled[..bytesReceived];

                    this.buffer = this.buffer.Length != 0 ? ByteTools.Concatenate(this.buffer, bytes) : bytes;

                    while (true)
                    {
                        if (this.buffer.Length < 6)
                        {
                            ArrayPool<byte>.Shared.Return(pooled, true);
                            break;
                        }

                        byte marker = this.buffer[0];
                        if (marker != 0x2A)
                        {
                            ArrayPool<byte>.Shared.Return(pooled, true);
                            break;
                        }

                        byte frame = this.buffer[1];
                        ushort sequence = BinaryPrimitives.ReadUInt16BigEndian(this.buffer.AsSpan()[2..4]);
                        ushort length = BinaryPrimitives.ReadUInt16BigEndian(this.buffer.AsSpan()[4..6]);

                        if (this.buffer.Length < length + 6)
                        {
                            ArrayPool<byte>.Shared.Return(pooled, true);
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
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e);
                    ArrayPool<byte>.Shared.Return(pooled, true);
                    break;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(pooled, true);
                }
            }

            await this.OnDisconnected();
        }

        private async Task OnDisconnected()
        {
            if (!this.Connected) return;
            this.Connected = false;

            if (this.Disconnected != null)
            {
                await this.Disconnected.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Send FLAP packet
        /// </summary>
        /// <param name="packet">FLAP Packet</param>
        public async Task SendPacketAsync(FLAPPacket packet)
        {
            if (this.socket == null || !this.Connected)
            {
                throw new InvalidOperationException("Socket is not connected");
            }

            packet.Sequence = this.seqNo++;
            await this.socket.SendAsync(packet.ToBytes(), SocketFlags.None);
        }
    }
}