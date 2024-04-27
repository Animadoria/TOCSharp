using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TOCSharp.Models;
using TOCSharp.Models.EventArgs;

namespace TOCSharp
{
    /// <summary>
    /// TOC Client
    /// </summary>
    public partial class TOCClient
    {
        private static readonly byte[] TICTOC = Encoding.UTF8.GetBytes("Tic/Toc");

        /// <summary>
        /// Current screenname
        /// </summary>
        public readonly string Screenname;

        private readonly string password;

        private FLAPConnection? connection;
        private readonly TOCClientSettings settings;

        private bool keepAliveLoopRunning;

        /// <summary>
        /// User's buddy list
        /// </summary>
        public BuddyList BuddyList { get; private set; } = new BuddyList();

        private readonly ConcurrentDictionary<string, ChatRoom> chatRooms = new ConcurrentDictionary<string, ChatRoom>();

        /// <summary>
        /// List of chat rooms
        /// </summary>
        public IReadOnlyDictionary<string, ChatRoom> ChatRooms => this.chatRooms;

        /// <summary>
        /// Current format of the screenname
        /// </summary>
        public string Format { get; private set; }

        /// <summary>
        /// On sign-on done
        /// </summary>
        public event AsyncEventHandler? SignOnDone;

        /// <summary>
        /// On VERSION received
        /// </summary>
        public event AsyncEventHandler<string>? VersionReceived;

        /// <summary>
        /// On NICK received
        /// </summary>
        public event AsyncEventHandler<string>? NickReceived;

        /// <summary>
        /// On buddy list config received
        /// </summary>
        public event AsyncEventHandler<BuddyList>? BuddyListReceived;

        /// <summary>
        /// On buddy info received
        /// </summary>
        public event AsyncEventHandler<BuddyInfo>? BuddyInfoReceived;

        /// <summary>
        /// On message received in a chat room
        /// </summary>
        public event AsyncEventHandler<ChatMessage>? ChatMessageReceived;

        /// <summary>
        /// On chat invite received
        /// </summary>
        public event AsyncEventHandler<ChatInviteEventArgs>? ChatInviteReceived;

        /// <summary>
        /// On chat join event
        /// </summary>
        public event AsyncEventHandler<ChatRoom>? ChatJoined;

        /// <summary>
        /// On chat leave event
        /// </summary>
        public event AsyncEventHandler<ChatRoom>? ChatLeft;

        /// <summary>
        /// On chat buddy update received
        /// </summary>
        public event AsyncEventHandler<ChatBuddyUpdate>? ChatBuddyUpdate;

        /// <summary>
        /// On client event (i.e. typing) received
        /// </summary>
        public event AsyncEventHandler<ClientEvent>? ClientEvent;

        /// <summary>
        /// On evil (aka warn) received
        /// </summary>
        public event AsyncEventHandler<EvilEventArgs>? EvilReceived;

        /// <summary>
        /// On IM received
        /// </summary>
        public event AsyncEventHandler<InstantMessage>? IMReceived;

        /// <summary>
        /// On profile received
        /// </summary>
        public event AsyncEventHandler<GoToURL>? ProfileReceived;

        /// <summary>
        /// On name format success
        /// </summary>
        public event AsyncEventHandler<bool>? NameFormatSuccess;

        /// <summary>
        /// On password change success
        /// </summary>
        public event AsyncEventHandler<bool>? PasswordChangeSuccess;

        /// <summary>
        /// On TOC error received
        /// </summary>
        public event AsyncEventHandler<ErrorEventArgs>? ErrorReceived;

        /// <summary>
        /// On disconnected from TOC server
        /// </summary>
        public event AsyncEventHandler? Disconnected;

        /// <summary>
        /// Creates a new TOC client
        /// </summary>
        /// <param name="screenname">Screenname to use</param>
        /// <param name="password">Password for user</param>
        /// <param name="settings">Settings for the TOC client</param>
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

        /// <summary>
        /// Connects to the TOC server
        /// </summary>
        /// <param name="doKeepAlive">Do keep alive</param>
        public async Task ConnectAsync(bool doKeepAlive = true)
        {
            this.connection = new FLAPConnection(this.settings.Hostname, this.settings.Port);

            this.connection.Disconnected += this.ConnectionOnDisconnected;
            this.connection.PacketReceived += this.PacketReceived;

            await this.connection.ConnectAsync();

            if (this.connection.Connected && doKeepAlive && !this.keepAliveLoopRunning)
                _ = this.StartKeepAliveLoopAsync();
        }

        /// <summary>
        /// On packet received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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

        /// <summary>
        /// Parse FLAP packet
        /// </summary>
        /// <param name="flap">FLAP packet</param>
        private async Task ParsePacket(FLAPPacket flap)
        {
            if (this.connection == null)
            {
                return;
            }

            if (flap.Frame == FLAPPacket.FRAME_SIGNON)
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
            else if (flap.Frame == FLAPPacket.FRAME_DATA)
            {
                string data = Encoding.UTF8.GetString(flap.Data);

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

        /// <summary>
        /// Sends a raw TOC command
        /// </summary>
        /// <param name="args">Command arguments</param>
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

        /// <summary>
        /// Change own password
        /// </summary>
        /// <param name="oldPassword">Old password</param>
        /// <param name="newPassword">New password</param>
        public async Task ChangePassword(string oldPassword, string newPassword)
        {
            await this.SendCommandAsync("toc_change_passwd", oldPassword, newPassword);
        }

        /// <summary>
        /// Set own screenname format
        /// </summary>
        /// <param name="format">Format to set</param>
        public async Task FormatScreenname(string format)
        {
            await this.SendCommandAsync("toc_format_nickname", format);
        }

        /// <summary>
        /// Get directory info
        /// </summary>
        /// <param name="screenName">Target screenname</param>
        public async Task GetDirectoryInfo(string screenName)
        {
            await this.SendCommandAsync("toc_get_dir", screenName);
        }

        /// <summary>
        /// Get user info
        /// </summary>
        /// <param name="screenName">Target screenname</param>
        public async Task GetUserInfo(string screenName)
        {
            await this.SendCommandAsync("toc_get_info", screenName);
        }

        /// <summary>
        /// Get status of user
        /// </summary>
        /// <param name="screenName">Target screenname</param>
        public async Task GetUserStatus(string screenName)
        {
            await this.SendCommandAsync("toc_get_status", screenName);
        }

        /// <summary>
        /// Set capabilities
        /// </summary>
        /// <param name="caps">List of capabilities</param>
        public async Task SetCapabilities(IEnumerable<string> caps)
        {
            List<string> parameters = new List<string> { "toc_set_caps" };
            parameters.AddRange(caps);
            await this.SendCommandAsync(parameters.ToArray());
        }

        /// <summary>
        /// Set profile TOC info
        /// </summary>
        /// <param name="profile">Profile message</param>
        public async Task SetProfile(string profile)
        {
            await this.SendCommandAsync("toc_set_info", profile);
        }

        /// <summary>
        /// Evil (aka warn) user
        /// </summary>
        /// <param name="screenName">Screenname to warn</param>
        /// <param name="anonymous">If anonymous warn</param>
        public async Task EvilUser(string screenName, bool anonymous)
        {
            await this.SendCommandAsync("toc_evil", screenName, anonymous ? "anon" : "norm");
        }

        /// <summary>
        /// Join a chat room
        /// </summary>
        /// <param name="roomName">Room name</param>
        /// <param name="exchange">Room exchange</param>
        public async Task JoinChatAsync(string roomName, int exchange = 4)
        {
            await this.SendCommandAsync("toc_chat_join", exchange.ToString(), roomName);
        }

        /// <summary>
        /// Leave a chat room
        /// </summary>
        /// <param name="room">Chat room to leave</param>
        public async Task LeaveChatAsync(ChatRoom room)
        {
            await this.SendCommandAsync("toc_chat_leave", room.ChatID);
        }

        /// <summary>
        /// Accept a chat invite
        /// </summary>
        /// <param name="chatID">Chat room ID</param>
        public async Task AcceptChatInvite(string chatID)
        {
            await this.SendCommandAsync("toc_chat_accept", chatID);
        }

        /// <summary>
        /// Send chat invite to users
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="message">Message to send</param>
        /// <param name="screennames">List of screennames</param>
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

        /// <summary>
        /// Send message to chat room
        /// </summary>
        /// <param name="room">Chat room to send</param>
        /// <param name="message">Message</param>
        /// <param name="toWhisper">Target to whisper, if needed</param>
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

        /// <summary>
        /// Send IM to user
        /// </summary>
        /// <param name="info">Target buddy info</param>
        /// <param name="message">Message to send</param>
        /// <param name="autoResponse">If it's an autoresponse</param>
        public async Task SendIMAsync(BuddyInfo info, string message, bool autoResponse = false)
        {
            await this.SendIMAsync(info.Screenname, message, autoResponse);
        }

        /// <summary>
        /// Send IM to user
        /// </summary>
        /// <param name="screenName">Target screenname</param>
        /// <param name="message">Message to send</param>
        /// <param name="autoResponse">If it's an autoresponse</param>
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

        /// <summary>
        /// Set your away message
        /// </summary>
        /// <param name="awayMessage">Away message to set</param>
        public async Task SetAwayMessage(string awayMessage)
        {
            await this.SendCommandAsync("toc_set_away", awayMessage);
        }

        /// <summary>
        /// Add a buddy to your buddy list
        /// </summary>
        /// <param name="group">Group to add the buddies</param>
        /// <param name="buddies">Buddies to add to the specified list</param>
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
        /// Send a client event to the target user
        /// </summary>
        /// <param name="target">Target screenname</param>
        /// <param name="type">Client event type</param>
        public async Task SendClientEvent(string target, ClientEventType type)
        {
            await this.SendCommandAsync("toc2_client_event", target, ((int)type).ToString());
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

        /// <summary>
        /// Starts the keep-alive loop
        /// </summary>
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