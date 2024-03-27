using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TOCSharp.Models;
using TOCSharp.Models.EventArgs;

namespace TOCSharp
{
    public partial class TOCClient
    {
        /// <summary>
        /// Parses a CONFIG2 response from the server.
        /// </summary>
        /// <param name="data">Raw TOC data</param>
        private async Task ParseConfig(string data)
        {
            string[] split = data.Split('\n');
            this.BuddyList = new BuddyList();
            string? currentGroup = null;
            List<BuddyInfo> buddies = new List<BuddyInfo>();

            List<BuddyInfo> permit = new List<BuddyInfo>();
            List<BuddyInfo> deny = new List<BuddyInfo>();

            foreach (string element in split)
            {
                string[] lineSplit = element.Split(':');

                switch (lineSplit[0])
                {
                    case "g":
                    {
                        // We must flush the current group if we change groups
                        if (currentGroup != null)
                        {
                            this.BuddyList.Buddies.Add(currentGroup, buddies.ToArray());
                            buddies.Clear();
                        }
                        currentGroup = lineSplit[1];
                        break;
                    }
                    case "b":
                    {
                        string screenname = lineSplit[1];
                        string? alias = lineSplit.Length > 2 ? lineSplit[2] : null;

                        BuddyInfo buddy = BuddyCache.Get(screenname) ?? new BuddyInfo(screenname);
                        buddy.Alias = alias;

                        BuddyCache.AddOrUpdate(buddy);
                        buddies.Add(buddy);
                        break;
                    }
                    case "m":
                    {
                        int pdMode = int.Parse(lineSplit[1]);
                        this.BuddyList.PDMode = (PDMode)pdMode;
                        break;
                    }
                    case "p":
                    {
                        string screenname = lineSplit[1];
                        BuddyInfo buddy = BuddyCache.Get(screenname) ?? new BuddyInfo(screenname);
                        BuddyCache.AddOrUpdate(buddy);

                        permit.Add(buddy);
                        break;
                    }
                    case "d":
                    {
                        string screenname = lineSplit[1];
                        BuddyInfo buddy = BuddyCache.Get(screenname) ?? new BuddyInfo(screenname);
                        BuddyCache.AddOrUpdate(buddy);

                        deny.Add(buddy);
                        break;
                    }
                    case "done":
                    {
                        // We must flush the current group because we're done.
                        if (currentGroup != null)
                        {
                            this.BuddyList.Buddies.Add(currentGroup, buddies.ToArray());
                            buddies.Clear();
                        }
                        break;
                    }
                }

                this.BuddyList.DenyList = deny.ToArray();
                this.BuddyList.PermitList = permit.ToArray();
            }

            await this.SendCommandAsync("toc_init_done");

            if (this.BuddyListReceived != null)
            {
                await this.BuddyListReceived.Invoke(this, this.BuddyList);
            }

            if (this.SignOnDone != null)
            {
                await this.SignOnDone.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task ParseUpdateBuddy(string data)
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

            BuddyInfo info = BuddyCache.Get(name) ?? new BuddyInfo(name);
            info.Online = online;
            info.Evil = evil;
            info.SignonTime = signon;
            info.IdleTime = idle;
            info.Class = userClass;

            BuddyCache.AddOrUpdate(info);

            if (this.BuddyInfoReceived != null)
            {
                await this.BuddyInfoReceived.Invoke(this, info);
            }
        }

        private async Task ParseChatInEnc(string data)
        {
            string[] split = data.Split(':', 7);
            string roomID = split[1];
            string senderScreenname = split[2];
            bool whisper = split[3] == "T";
            string message = split[6];

            BuddyInfo sender = BuddyCache.Get(senderScreenname) ?? new BuddyInfo(senderScreenname);
            ChatRoom room = this.chatRooms[roomID];

            if (this.ChatMessageReceived != null)
            {
                await this.ChatMessageReceived.Invoke(this, new ChatMessage
                {
                    RoomID = room,
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
            string senderScreenname = split[3];
            string message = split[4];

            BuddyInfo sender = BuddyCache.Get(senderScreenname) ?? new BuddyInfo(senderScreenname);

            if (this.ChatInviteReceived != null)
            {
                await this.ChatInviteReceived.Invoke(this, new ChatInviteEventArgs
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
            bool autoResponse = split[2] == "T";
            string message = split[9];

            BuddyInfo sender = BuddyCache.Get(username) ?? new BuddyInfo(username);

            if (this.IMReceived != null)
            {
                await this.IMReceived.Invoke(this, new InstantMessage(sender, message, autoResponse));
            }
        }

        /// <summary>
        /// Handles CHAT_JOIN responses from the server.
        /// </summary>
        /// <param name="data">Raw TOC data</param>
        private async Task ParseChatJoin(string data)
        {
            string[] split = data.Split(':', 4);
            string roomID = split[1];
            string roomName = split[2];

            ChatRoom room = new ChatRoom(roomName, roomID);
            this.chatRooms[roomID] = room;

            if (this.ChatJoined != null)
            {
                await this.ChatJoined.Invoke(this, room);
            }
        }

        /// <summary>
        /// Handles CHAT_BUDDY_UPDATE responses from the server.
        /// </summary>
        /// <param name="data">Raw TOC data</param>
        private async Task ParseChatUpdateBuddy(string data)
        {
            string[] split = data.Split(':');
            string roomID = split[1];
            bool isOnline = split[2] == "T";
            string[] buddyScreennames = split[3..];
            BuddyInfo[] buddies = new BuddyInfo[buddyScreennames.Length];

            for (int i = 0; i < buddyScreennames.Length; i++)
            {
                buddies[i] = BuddyCache.Get(buddyScreennames[i]) ?? new BuddyInfo(buddyScreennames[i]);
            }

            ChatRoom room = this.chatRooms[roomID];

            if (isOnline)
            {
                foreach (BuddyInfo buddy in buddies)
                {
                    room.Users.Add(buddy);
                }
            }
            else
            {
                foreach (BuddyInfo buddy in buddies)
                {
                    room.Users.RemoveWhere(x => x.Screenname == buddy.Screenname);
                }
            }

            if (this.ChatBuddyUpdate != null)
            {
                await this.ChatBuddyUpdate.Invoke(this, new ChatBuddyUpdate()
                {
                    Room = room,
                    IsOnline = isOnline,
                    Buddies = buddies
                });
            }
        }

        private async Task ParseChatLeft(string data)
        {
            string[] split = data.Split(':', 2);
            string chatLeft = split[1];
            ChatRoom room = this.chatRooms[chatLeft];
            this.chatRooms.Remove(chatLeft, out _);

            if (this.ChatLeft != null)
            {
                await this.ChatLeft.Invoke(this, room);
            }
        }

        /// <summary>
        /// Handles SIGN_ON responses from the server.
        /// </summary>
        /// <param name="data">Raw TOC data</param>
        private async Task ParseSignOn(string data)
        {
            string[] split = data.Split(':', 2);
            string version = split[1];

            if (this.VersionReceived != null)
            {
                await this.VersionReceived.Invoke(this, version);
            }
        }

        private async Task ParseClientEvent(string data)
        {
            string[] split = data.Split(':', 3);
            string sender = split[1];

            BuddyInfo info = BuddyCache.Get(sender) ?? new BuddyInfo(sender);

            ClientEventType ev = (ClientEventType)int.Parse(split[2]);

            if (this.ClientEvent != null)
            {
                await this.ClientEvent.Invoke(this, new ClientEvent(info, ev));
            }
        }

        private async Task ParseNick(string data)
        {
            string[] split = data.Split(':', 2);
            string nick = split[1];

            this.Format = nick;

            if (this.NickReceived != null)
            {
                await this.NickReceived.Invoke(this, nick);
            }
        }

        private async Task ParseEviled(string data)
        {
            string[] split = data.Split(':', 3);
            int newEvil = int.Parse(split[1]);
            string? evilerSN = split.Length > 2 ? split[2] : null;

            BuddyInfo? eviler = null;
            if (evilerSN != null)
            {
                eviler = BuddyCache.Get(evilerSN) ?? new BuddyInfo(evilerSN);
                BuddyCache.AddOrUpdate(eviler);
            }

            if (this.EvilReceived != null)
            {
                await this.EvilReceived.Invoke(this, new EvilEventArgs(newEvil, eviler));
            }
        }

        private async Task ParseGoToURL(string data)
        {
            string[] split = data.Split(':', 3);
            string windowName = split[1];
            string profile = split[2];
            string profileURL = $"http://{this.settings.Hostname}:{this.settings.Port}/{profile}";

            if (this.ProfileReceived != null)
            {
                await this.ProfileReceived.Invoke(this, new GoToURL(profileURL, windowName));
            }
        }

        private async Task ParseAdminNickStatus(string data)
        {
            string[] split = data.Split(':', 2);
            string status = split[1];

            if (this.NameFormatSuccess != null)
            {
                await this.NameFormatSuccess.Invoke(this, status == "0");
            }
        }

        private async Task ParseAdminPasswordStatus(string data)
        {
            string[] split = data.Split(':', 2);
            string status = split[1];

            if (this.PasswordChangeSuccess != null)
            {
                await this.PasswordChangeSuccess.Invoke(this, status == "0");
            }
        }

        private async Task ParseError(string data)
        {

            string[] split = data.Split(':');
            string error = split[1];
            string[] remaining = split.Length > 2 ? split[2..] : Array.Empty<string>();

            if (this.ErrorReceived != null)
            {
                await this.ErrorReceived.Invoke(this, new ErrorEventArgs(error, remaining));
            }
        }
    }
}