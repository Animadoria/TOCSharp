using System;
using System.Text;
using System.Threading.Tasks;
using TOCSharp.Models;

namespace TOCSharp
{
    public class TOCClient
    {
        private static readonly byte[] TICTOC = Encoding.UTF8.GetBytes("Tic/Toc");

        public readonly string Screenname;
        private readonly string password;

        private readonly FLAPConnection connection = new FLAPConnection();

        public event AsyncEventHandler? SignOnDone;
        public event AsyncEventHandler<string>? NickReceived;
        public event AsyncEventHandler<BuddyInfo>? BuddyInfoReceived;
        public event AsyncEventHandler<ChatMessage>? ChatMessageReceived;
        public event AsyncEventHandler<ChatRoom>? ChatJoined;
        public event AsyncEventHandler<ChatBuddyUpdate>? ChatBuddyUpdate;
        public event AsyncEventHandler<InstantMessage>? IMReceived;
        public event AsyncEventHandler<string>? ErrorReceived;
        public event AsyncEventHandler? Disconnected;

        public TOCClient(string screenname, string password)
        {
            this.Screenname = screenname;
            this.password = password;

            this.connection.Disconnected += this.ConnectionOnDisconnected;
            this.connection.PacketReceived += this.PacketReceived;
        }

        private async Task ConnectionOnDisconnected(object sender, EventArgs e)
        {
            if (this.Disconnected != null)
                await this.Disconnected.Invoke(this, EventArgs.Empty);
        }

        public async Task ConnectAsync()
        {
            await this.connection.ConnectAsync();
        }

        private async Task PacketReceived(object sender, FLAPPacket args)
        {
            try
            {
                await this.ParsePacket(args);
            }
            catch (TaskCanceledException)
            {
                // continue;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
            }
        }

        private async Task ParsePacket(FLAPPacket args)
        {
            if (args.Frame == FLAPPacket.FRAME_SIGNON)
            {
                string normalized = Utils.NormalizeScreenname(this.Screenname);
                // Send FLAPON
                await this.connection.SendPacketAsync(new FLAPPacket
                {
                    Frame = FLAPPacket.FRAME_SIGNON,
                    Data =
                        ByteTools.Concatenate(
                            new byte[]
                            {
                                0x00, 0x00, 0x00, 0x01, // FLAP version
                                0x00, 0x01, // Username TLV
                            },
                            ByteTools.PackUInt16((ushort)normalized.Length),
                            Encoding.UTF8.GetBytes(normalized))
                });

                string roastedPW = $"0x{ByteTools.ToHexString(Utils.XorArray(this.password, TICTOC))}";
                await this.SendCommandAsync("toc2_login", "login.oscar.aol.com", "5190", this.Screenname, roastedPW, "en", "TOCSharp",
                                            "160",
                                            "us", "", "", "3", "0", "20812", "-utf8", "-kentucky", "-preakness",
                                            Utils.GenerateHash(this.Screenname, this.password).ToString());
            }
            else if (args.Frame == FLAPPacket.FRAME_DATA)
            {
                string data = Encoding.UTF8.GetString(args.Data);

                Console.WriteLine(data);
                string command = data[..data.IndexOf(':')];

                if (command == "SIGN_ON")
                {
                    await this.SendCommandAsync("toc_init_done");

                    if (this.SignOnDone != null)
                    {
                        await this.SignOnDone.Invoke(this, EventArgs.Empty);
                    }
                }
                else if (command == "NICK")
                {
                    string[] split = data.Split(':', 2);
                    string nick = split[1];

                    if (this.NickReceived != null)
                    {
                        await this.NickReceived.Invoke(this, nick);
                    }
                }
                else if (command == "UPDATE_BUDDY")
                {
                    string[] split = data.Split(':', 8);
                    string name = split[1];
                    bool online = split[2] == "T";
                    int evil = int.Parse(split[3]);
                    DateTimeOffset signon = DateTimeOffset.FromUnixTimeSeconds(long.Parse(split[4]));
                    int idle = int.Parse(split[5]);
                    string rawClass = split[6];

                    UserClass userClass = UserClass.None;

                    if (rawClass[0] == 'A')
                    {
                        userClass |= UserClass.OnAOL;
                    }
                    if (rawClass[1] == 'A')
                    {
                        userClass |= UserClass.OSCARAdmin;
                    }
                    else if (rawClass[1] == 'O')
                    {
                        userClass |= UserClass.OSCARNormal;
                    }
                    else if (rawClass[1] == 'U')
                    {
                        userClass |= UserClass.OSCARUnconfirmed;
                    }
                    if (rawClass.Length > 2 && rawClass[2] == 'U')
                    {
                        userClass |= UserClass.Unavailable;
                    }

                    BuddyInfo info = new BuddyInfo
                    {
                        Screenname = name,
                        Online = online,
                        Evil = evil,
                        SignonTime = signon,
                        IdleTime = idle,
                        Class = userClass
                    };

                    if (this.BuddyInfoReceived != null)
                    {
                        await this.BuddyInfoReceived.Invoke(this, info);
                    }
                }
                else if (command == "CHAT_IN_ENC")
                {
                    await this.ParseChatInEnc(data);
                }
                else if (command == "IM_IN_ENC2")
                {
                    await this.ParseIMInEnc2(data);
                }
                else if (command == "CHAT_JOIN")
                {
                    string[] split = data.Split(':', 4);
                    string roomID = split[1];
                    string roomName = split[2];
                    if (this.ChatJoined != null)
                    {
                        await this.ChatJoined.Invoke(this, new ChatRoom()
                        {
                            ChatID = roomID,
                            Name = roomName
                        }).ContinueWith(x => Console.WriteLine("Exception on ChatJoined: " + x.Exception),
                                        TaskContinuationOptions.OnlyOnFaulted);
                    }
                }
                else if (command == "CHAT_UPDATE_BUDDY")
                {
                    string[] split = data.Split(':');
                    string roomID = split[1];
                    bool isOnline = split[2] == "T";
                    string[] buddies = split[3..];

                    if (this.ChatBuddyUpdate != null)
                    {
                        await this.ChatBuddyUpdate.Invoke(this, new ChatBuddyUpdate()
                        {
                            RoomID = roomID,
                            IsOnline = isOnline,
                            Buddies = buddies
                        }).ContinueWith(x => Console.WriteLine("Exception on ChatBuddyUpdate: " + x.Exception),
                                        TaskContinuationOptions.OnlyOnFaulted);
                        ;
                    }
                }
                else if (command == "ERROR")
                {
                    string[] split = data.Split(':');
                    string error = split[1];

                    if (this.ErrorReceived != null)
                    {
                        await this.ErrorReceived.Invoke(this, error)
                                  .ContinueWith(x => Console.WriteLine("Exception on ErrorReceived: " + x.Exception),
                                                TaskContinuationOptions.OnlyOnFaulted);
                        ;
                    }
                }
            }
        }

        private async Task ParseChatInEnc(string data)
        {
            string[] split = data.Split(':', 7);
            string roomID = split[1];
            string sender = split[2];
            bool whisper = split[3] == "T";
            string message = split[6];

            if (this.ChatMessageReceived != null)
            {
                await this.ChatMessageReceived.Invoke(this, new ChatMessage
                {
                    RoomID = roomID,
                    Sender = sender,
                    Whisper = whisper,
                    Message = message
                }).ContinueWith(x => Console.WriteLine("Exception on ChatMessageReceived: " + x.Exception),
                                TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        private async Task ParseIMInEnc2(string data)
        {
            string[] split = data.Split(':', 10);
            string username = split[1];
            string message = split[9];

            if (this.IMReceived != null)
            {
                await this.IMReceived.Invoke(this, new InstantMessage()
                {
                    Sender = username,
                    Message = message
                }).ContinueWith(x => Console.WriteLine("Exception on IMReceived: " + x.Exception), TaskContinuationOptions.OnlyOnFaulted);
                ;
            }
        }

        private async Task SendCommandAsync(params string[] args)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                string? arg = args[i];

                // Escaped: { '\\', '$', '"', '(', ')', '[', ']', '{', '}' }
                arg = arg.Replace("\\", "\\\\")
                         .Replace("$", "\\$")
                         .Replace("\"", "\\\"")
                         .Replace("(", "\\(")
                         .Replace(")", "\\)")
                         .Replace("[", "\\[")
                         .Replace("]", "\\]")
                         .Replace("{", "\\{")
                         .Replace("}", "\\}");

                if (arg.Contains(' ') || arg.Length == 0)
                {
                    arg = $"\"{arg}\"";
                }

                sb.Append(arg);
                if (i != args.Length - 1)
                {
                    sb.Append(' ');
                }
            }

            await this.connection.SendPacketAsync(new FLAPPacket
            {
                Frame = FLAPPacket.FRAME_DATA,
                Data = Encoding.UTF8.GetBytes(sb.Append('\0').ToString())
            });
        }

        public async Task JoinChatAsync(string roomName, int exchange = 4)
        {
            await this.SendCommandAsync("toc_chat_join", exchange.ToString(), roomName);
        }

        public async Task SendChatMessageAsync(string roomID, string message, string? toWhisper = null)
        {
            if (toWhisper != null)
            {
                await this.SendCommandAsync("toc_chat_whisper_enc", roomID, "U", toWhisper, message);
            }
            else
            {
                await this.SendCommandAsync("toc_chat_send_enc", roomID, "U", message);
            }
        }

        public async Task SendIMAsync(string message, string sender)
        {
            await this.SendCommandAsync("toc2_send_im_enc", sender, "F", "U", "en", message);
        }

        public async Task DisconnectAsync()
        {
            await this.connection.DisconnectAsync();
        }
    }
}