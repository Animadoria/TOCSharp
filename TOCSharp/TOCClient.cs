using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TOCSharp.Models;
using TOCSharp.Models.EventArgs;

namespace TOCSharp
{
    public partial class TOCClient
    {
        private static readonly byte[] TICTOC = Encoding.UTF8.GetBytes("Tic/Toc");

        public readonly string Screenname;
        private readonly string password;

        private FLAPConnection? connection;
        private readonly TOCClientSettings settings;

        private bool keepAliveLoopRunning;

        public BuddyList BuddyList { get; private set; } = new BuddyList();

        private readonly ConcurrentDictionary<string, ChatRoom> chatRooms = new ConcurrentDictionary<string, ChatRoom>();

        public IReadOnlyDictionary<string, ChatRoom> ChatRooms => this.chatRooms;
        public string Format { get; private set; }

        public event AsyncEventHandler? SignOnDone;
        public event AsyncEventHandler<string>? VersionReceived;
        public event AsyncEventHandler<string>? NickReceived;
        public event AsyncEventHandler<BuddyList>? BuddyListReceived;
        public event AsyncEventHandler<BuddyInfo>? BuddyInfoReceived;
        public event AsyncEventHandler<ChatMessage>? ChatMessageReceived;
        public event AsyncEventHandler<ChatInviteEventArgs>? ChatInviteReceived;
        public event AsyncEventHandler<ChatRoom>? ChatJoined;
        public event AsyncEventHandler<ChatRoom>? ChatLeft;
        public event AsyncEventHandler<ChatBuddyUpdate>? ChatBuddyUpdate;
        public event AsyncEventHandler<ClientEvent>? ClientEvent;
        public event AsyncEventHandler<EvilEventArgs>? EvilReceived;
        public event AsyncEventHandler<InstantMessage>? IMReceived;
        public event AsyncEventHandler<GoToURL>? ProfileReceived;
        public event AsyncEventHandler<bool>? NameFormatSuccess;
        public event AsyncEventHandler<bool>? PasswordChangeSuccess;
        public event AsyncEventHandler<ErrorEventArgs>? ErrorReceived;
        public event AsyncEventHandler? Disconnected;

        public TOCClient(string screenname, string password, TOCClientSettings? settings = null)
        {
            settings ??= new TOCClientSettings();
            this.Screenname = screenname;
            this.Format = screenname;
            this.password = password;

            this.settings = settings;
        }

        private async Task ConnectionOnDisconnected(object sender, EventArgs e)
        {
            if (this.Disconnected != null)
                await this.Disconnected.Invoke(this, EventArgs.Empty);
        }

        public async Task ConnectAsync(bool doKeepAlive = true)
        {
            this.connection = new FLAPConnection(this.settings.Hostname, this.settings.Port);

            this.connection.Disconnected += this.ConnectionOnDisconnected;
            this.connection.PacketReceived += this.PacketReceived;

            await this.connection.ConnectAsync();

            if (doKeepAlive && !this.keepAliveLoopRunning)
                _ = this.StartKeepAliveLoopAsync();
        }

        private async Task PacketReceived(object sender, FLAPPacket args)
        {
            try
            {
                await this.ParsePacket(args);
            }

            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + e);
            }
        }

        private async Task ParsePacket(FLAPPacket args)
        {
            if (this.connection == null)
            {
                return;
            }

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
                await this.SendCommandAsync("toc2_login", "login.oscar.aol.com", "5190", this.Screenname, roastedPW, "en",
                                            $"{this.settings.ClientName}-TOCSharp",
                                            "160",
                                            "us", "", "", "3", "0", "20812", "-utf8", "-kentucky", "-preakness",
                                            Utils.GenerateHash(this.Screenname, this.password).ToString());
            }
            else if (args.Frame == FLAPPacket.FRAME_DATA)
            {
                string data = Encoding.UTF8.GetString(args.Data);

                if (this.settings.DebugMode)
                {
                    Console.WriteLine($"Received: {data}");
                }

                string command = data[..data.IndexOf(':')];

                Task? parse = command switch
                {
                    "SIGN_ON"             => this.ParseSignOn(data),
                    "CONFIG2"             => this.ParseConfig(data),
                    "NICK"                => this.ParseNick(data),
                    "UPDATE_BUDDY2"       => this.ParseUpdateBuddy(data),
                    "CHAT_IN_ENC"         => this.ParseChatInEnc(data),
                    "IM_IN_ENC2"          => this.ParseIMInEnc2(data),
                    "CHAT_JOIN"           => this.ParseChatJoin(data),
                    "CHAT_UPDATE_BUDDY"   => this.ParseChatUpdateBuddy(data),
                    "CHAT_LEFT"           => this.ParseChatLeft(data),
                    "CHAT_INVITE"         => this.ParseChatInvite(data),
                    "CLIENT_EVENT2"       => this.ParseClientEvent(data),
                    "EVILED"              => this.ParseEviled(data),
                    "GOTO_URL"            => this.ParseGoToURL(data),
                    "ADMIN_NICK_STATUS"   => this.ParseAdminNickStatus(data),
                    "ADMIN_PASSWD_STATUS" => this.ParseAdminPasswordStatus(data),
                    "ERROR"               => this.ParseError(data),
                    _                     => null
                };

                if (parse != null)
                {
                    await parse;
                }
            }
        }

        private async Task SendCommandAsync(params string[] args)
        {
            if (this.connection == null)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                // Escaped: { '\\', '$', '"', '(', ')', '[', ']', '{', '}' }
                arg = arg.Replace("\\", @"\\")
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

        public async Task ChangePassword(string oldPassword, string newPassword)
        {
            await this.SendCommandAsync("toc_change_passwd", oldPassword, newPassword);
        }

        public async Task FormatScreenname(string screenName)
        {
            await this.SendCommandAsync("toc_format_nickname", screenName);
        }

        public async Task GetDirectoryInfo(string screenName)
        {
            await this.SendCommandAsync("toc_get_dir", screenName);
        }

        public async Task GetUserInfo(string screenName)
        {
            await this.SendCommandAsync("toc_get_info", screenName);
        }

        public async Task GetUserStatus(string screenName)
        {
            await this.SendCommandAsync("toc_get_status", screenName);
        }

        public async Task SetCapabilities(IEnumerable<string> caps)
        {
            List<string> parameters = new List<string> { "toc_set_caps" };
            parameters.AddRange(caps);
            await this.SendCommandAsync(parameters.ToArray());
        }

        public async Task SetProfile(string profile)
        {
            await this.SendCommandAsync("toc_set_info", profile);
        }

        public async Task EvilUser(string screenName, bool anonymous)
        {
            await this.SendCommandAsync("toc_evil", screenName, anonymous ? "anon" : "norm");
        }

        public async Task JoinChatAsync(string roomName, int exchange = 4)
        {
            await this.SendCommandAsync("toc_chat_join", exchange.ToString(), roomName);
        }

        public async Task LeaveChatAsync(ChatRoom room)
        {
            await this.SendCommandAsync("toc_chat_leave", room.ChatID);
        }

        public async Task AcceptChatInvite(string chatID)
        {
            await this.SendCommandAsync("toc_chat_accept", chatID);
        }

        public async Task SendChatInviteAsync(string roomID, string message, params string[] screennames)
        {
            if (screennames.Length == 0)
            {
                return;
            }

            List<string> param = new List<string> { "toc_chat_invite", roomID, message };
            param.AddRange(screennames);
            await this.SendCommandAsync(param.ToArray());
        }

        public async Task SendChatMessageAsync(ChatRoom room, string message, string? toWhisper = null)
        {
            if (toWhisper != null)
            {
                await this.SendCommandAsync("toc_chat_whisper_enc", room.ChatID, "U", toWhisper, message);
            }
            else
            {
                await this.SendCommandAsync("toc_chat_send_enc", room.ChatID, "U", message);
            }
        }

        public async Task SendIMAsync(BuddyInfo info, string message, bool autoResponse = false)
        {
            await this.SendIMAsync(info.Screenname, message, autoResponse);
        }

        public async Task SendIMAsync(string screenName, string message, bool autoResponse = false)
        {
            List<string> parameters = new List<string>()
            {
                "toc2_send_im_enc", screenName, "F", "U", "en", message
            };

            if (autoResponse)
            {
                parameters.Add("auto");
            }

            await this.SendCommandAsync(parameters.ToArray());
        }

        public async Task SetAwayMessage(string awayMessage)
        {
            await this.SendCommandAsync("toc_set_away", awayMessage);
        }

        public async Task AddBuddies(string group, IEnumerable<string> buddies)
        {
            string config = $"{{g:{group}\n";
            foreach (string buddy in buddies)
            {
                config += $"b:{buddy}\n";
            }
            config += "}";

            await this.SendCommandAsync("toc2_new_buddies", config);
        }

        /// <summary>
        /// Remove a buddy from your buddy list
        /// </summary>
        /// <param name="group"></param>
        /// <param name="screenname"></param>
        public async Task RemoveBuddy(string group, string screenname)
        {
            await this.SendCommandAsync("toc2_remove_buddy", screenname, group);
        }

        /// <summary>
        /// Disconnect TOC client
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (this.connection == null)
            {
                return;
            }
            await this.connection.DisconnectAsync();
        }

        private async Task StartKeepAliveLoopAsync()
        {
            if (this.settings.KeepAliveInterval.TotalMilliseconds < 5000)
            {
                Console.WriteLine("Keep-alive interval is too frequent!");
                return;
            }

            this.keepAliveLoopRunning = true;
            while (this.connection is { Connected: true })
            {
                await Task.Delay(this.settings.KeepAliveInterval);

                if (this.connection is { Connected: true })
                {
                    if (this.settings.DebugMode)
                    {
                        Console.WriteLine($"Sending keep-alive, {this.settings.KeepAliveInterval} timer elapsed");
                    }
                    await this.connection.SendKeepAliveAsync();
                }
            }

            this.keepAliveLoopRunning = false;
            if (this.settings.DebugMode)
            {
                Console.WriteLine("Keep-alive loop aborted; connection is dead!");
            }
        }
    }
}