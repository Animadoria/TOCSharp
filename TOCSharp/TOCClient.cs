using System;
using System.Collections.Generic;
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
        public event AsyncEventHandler<string>? VersionReceived;
        public event AsyncEventHandler<int>? BuddyListPrivacyMode;
        public event AsyncEventHandler<string>? BuddyListPermit;
        public event AsyncEventHandler<string>? BuddyListDeny;
        public event AsyncEventHandler<Buddy>? BuddyListBuddy;
        public event AsyncEventHandler<string>? NickReceived;
        public event AsyncEventHandler<BuddyInfo>? BuddyInfoReceived;
        public event AsyncEventHandler<ChatMessage>? ChatMessageReceived;
        public event AsyncEventHandler<ChatInvite>? ChatInviteReceived;
        public event AsyncEventHandler<ChatRoom>? ChatJoined;
        public event AsyncEventHandler<string>? ChatLeft;
        public event AsyncEventHandler<ChatBuddyUpdate>? ChatBuddyUpdate;
        public event AsyncEventHandler<ClientEvent>? ClientEvent;
        public event AsyncEventHandler<Evil>? EvilReceived;
        public event AsyncEventHandler<InstantMessage>? IMReceived;
        public event AsyncEventHandler<string>? ProfileReceived;
        public event AsyncEventHandler<bool>? NameFormatSuccess;
        public event AsyncEventHandler<bool>? PasswordChangeSuccess;
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

            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + e);
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

                //System.Diagnostics.Debug.WriteLine(data);
                string command = data[..data.IndexOf(':')];

                if (command == "SIGN_ON")
                {
                    string[] split = data.Split(':', 2);
                    string version = split[1];

                    if (this.VersionReceived != null)
                    {
                        await this.VersionReceived.Invoke(this, version);
                    }

                    /* moved to the end of CONFIG2
                    await this.SendCommandAsync("toc_init_done");

                    if (this.SignOnDone != null)
                    {
                        await this.SignOnDone.Invoke(this, EventArgs.Empty);
                    }
                    */
                }
                else if (command == "CONFIG2")
                {
                    string[] split = data.Split('\n');
                    int groups = 0;
                    string lastGroup = "";

                    for (int i = 0; i < split.GetUpperBound(0) - 1; i++)
                    {
                        switch (split[i].Substring(0,1))
                        {
                            case "m": //PRIVACY
                                if (this.BuddyListPrivacyMode != null)
                                {
                                    await this.BuddyListPrivacyMode.Invoke(this,int.Parse(split[i][2..]));
                                }
                                break;
                            case "p": //PERMIT
                                if (this.BuddyListPermit != null)
                                {
                                    await this.BuddyListPermit.Invoke(this, split[i][2..]);
                                }
                                break;
                            case "d": //DENY
                                if (this.BuddyListDeny != null)
                                {
                                    await this.BuddyListDeny.Invoke(this, split[i][2..]);
                                }
                                break;
                            case "g": //GROUP
                                groups++;

                                lastGroup = split[i][2..];
                                break;
                            case "b": //BUDDY
                                if (this.BuddyListBuddy != null)
                                {
                                    await this.BuddyListBuddy.Invoke(this, new Buddy()
                                    {
                                        Group = lastGroup,
                                        ScreenName = split[i][2..]
                                    });
                                }
                                break;
                        }
                    }
                    await this.SendCommandAsync("toc_init_done");
                    await this.SendCommandAsync("toc_set_caps", "748F2420628711D18222444553540000"); //Chat Capabilities

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
                else if (command == "UPDATE_BUDDY2")
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

                    BuddyInfo info = new BuddyInfo()
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
                        });
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
                        });
                    }
                }
                else if (command == "CHAT_INVITE")
                {
                    await this.ParseChatInvite(data);
                }
                else if (command == "CHAT_LEFT")
                {
                    string[] split = data.Split(':', 2);
                    string chatLeft = split[1];

                    if (this.ChatLeft != null)
                    {
                        await this.ChatLeft.Invoke(this, chatLeft);
                    }
                }
                else if (command == "CLIENT_EVENT2")
                {
                    string[] split = data.Split(':', 3);
                    string sender = split[1];
                    bool isTyping = split[2] == "2";

                    if (this.ClientEvent != null)
                    {
                        await this.ClientEvent.Invoke(this, new ClientEvent()
                        {
                            Sender = sender,
                            IsTyping = isTyping
                        });
                    }
                }
                else if (command == "EVILED")
                {
                    string[] split = data.Split(':', 3);
                    string newEvil = split[1];
                    string eviler = split[2];

                    if (this.EvilReceived != null)
                    {
                        await this.EvilReceived.Invoke(this, new Evil()
                        {
                            NewEvil = newEvil,
                            Eviler = eviler
                        });
                    }
                }
                else if (command == "GOTO_URL")
                {
                    string[] split = data.Split(':', 3);
                    string profile = split[2];
                    string profileURL = "http://" + FLAPConnection.DEFAULT_HOST + ":" + FLAPConnection.DEFAULT_PORT + "/" + profile;

                    if (this.ProfileReceived != null)
                    {
                        await this.ProfileReceived.Invoke(this, profileURL);
                    }
                }
                else if (command == "ADMIN_NICK_STATUS")
                {
                    string[] split = data.Split(':', 2);
                    string status = split[1];

                    if (this.NameFormatSuccess != null)
                    {
                        if (status == "0")
                        {
                            await this.NameFormatSuccess.Invoke(this, true);
                        }
                        else
                        {
                            await this.NameFormatSuccess.Invoke(this, false);
                        }
                    }
                }
                else if (command == "ADMIN_PASSWD_STATUS")
                {
                    string[] split = data.Split(':', 2);
                    string status = split[1];

                    if (this.PasswordChangeSuccess != null)
                    {
                        if (status == "0")
                        {
                            await this.PasswordChangeSuccess.Invoke(this, true);
                        }
                        else
                        {
                            await this.PasswordChangeSuccess.Invoke(this, false);
                        }
                    }

                }
                else if (command == "ERROR")
                {
                    string[] split = data.Split(':');
                    string error = split[1];

                    if (this.ErrorReceived != null)
                    {
                        await this.ErrorReceived.Invoke(this, error);
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
                });
            }
        }
        private async Task ParseChatInvite(string data)
        {
            string[] split = data.Split(':', 5);
            string roomName = split[1];
            string roomID = split[2];
            string sender = split[3];
            string message = split[4];

            if (this.ChatInviteReceived != null)
            {
                await this.ChatInviteReceived.Invoke(this, new ChatInvite
                {
                    RoomName = roomName,
                    RoomID = roomID,
                    Sender = sender,
                    Message = message
                });
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
                });
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

        public async Task AddBuddy(string screenName)
        {
            if (screenName == null) { return; }

            await this.SendCommandAsync("toc_add_buddy", screenName);
        }

        public async Task RemoveBuddy(string screenName)
        {
            if (screenName == null) { return; }

            await this.SendCommandAsync("toc_remove_buddy", screenName);
        }

        public async Task AddPermit(string screenName)
        {
            if (screenName == null) { return; }

            await this.SendCommandAsync("toc_add_permit", screenName);
        }

        public async Task AddDeny(string screenName)
        {
            if (screenName == null) { return; }

            await this.SendCommandAsync("toc_add_deny", screenName);
        }

        public async Task ChangePassword(string oldPassword, string newPassword)
        {
            if (oldPassword == null || newPassword == null) { return; }

            await this.SendCommandAsync("toc_change_passwd", oldPassword, newPassword);
        }

        public async Task FormatSN(string screenName)
        {
            if (screenName == null) { return; }

            await this.SendCommandAsync("toc_format_nickname", screenName);
        }

        public async Task GetDirectoryInfo(string screenName)
        {
            if (screenName == null) { return; }

            await this.SendCommandAsync("toc_get_dir", screenName);
        }

        public async Task GetUserInfo(string screenName)
        {
            if (screenName == null) { return; }

            await this.SendCommandAsync("toc_get_info", screenName);
        }

        public async Task GetUserStatus(string screenName)
        {
            if (screenName == null) { return; }

            await this.SendCommandAsync("toc_get_status", screenName);
        }

        public async Task SetCapabilities(string caps)
        {
            if (caps == null) { return; }

            await this.SendCommandAsync("toc_set_caps", caps);
        }

        public async Task SetProfile(string profile)
        {
            if (profile == null) { return; }

            await this.SendCommandAsync("toc_set_info", profile);
        }
        
        public async Task WarnSN(string screenName, bool anonymous)
        {
            if (screenName == null) { return; }

            await this.SendCommandAsync("toc_evil", screenName, anonymous ? "anon" : "norm");
        }

        public async Task JoinChatAsync(string roomName, int exchange = 4)
        {
            await this.SendCommandAsync("toc_chat_join", exchange.ToString(), roomName);
        }

        public async Task LeaveChatAsync(string roomID)
        {
            if (roomID == null) { return; }

            await this.SendCommandAsync("toc_chat_leave", roomID);
        }

        public async Task AcceptChatInvite(string roomID)
        {
            if (roomID == null) { return; }

            await this.SendCommandAsync("toc_chat_accept", roomID);
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

        public async Task SendIMAsync(string screenName, string message, bool autoResponse = false)
        {
            if (autoResponse == true)
            {
                await this.SendCommandAsync("toc2_send_im_enc", screenName, "F", "U", "en", message, "auto");
            }
            else
            {
                await this.SendCommandAsync("toc2_send_im_enc", screenName, "F", "U", "en", message);
            }
        }

        public async Task SetAwayMessage(string awayMessage)
        {
            await this.SendCommandAsync("toc_set_away", awayMessage);
        }

        public async Task DisconnectAsync()
        {
            await this.connection.DisconnectAsync();
        }
    }
}
