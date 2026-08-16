using DotNetty.Transport.Channels;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.BuildersClub;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Inventory.Achievements;
using Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Sound;
using Plus.Core;
using Plus.Database.EF;
using Plus.HabboHotel.Permissions;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Subscriptions;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger.FriendBar;
using Plus.HabboHotel.Users.UserData;
using Plus.Network.Codec;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Plus.HabboHotel.GameClients
{
    public class GameClient
    {
        private Habbo _habbo;
        public string MachineId;
        public string IpAddress = string.Empty;
        private bool _disconnected;
        private bool _ghost;
        private readonly IChannelHandlerContext _channel;
        public int PingCount { get; set; }
        public long PingRequestedAt { get; set; }

        public long KeepAlivePingAt { get; set; }

        public double LatencyMs { get; private set; }

        public void RecordLatencySample(double sampleMs)
        {
            // A negative or absurd sample means the reply didn't belong to the probe we timed
            // (the client also pings unprompted) — better no data than bad data.
            if (sampleMs <= 0 || sampleMs > 2000)
                return;

            LatencyMs = LatencyMs <= 0 ? sampleMs : (LatencyMs * 0.7) + (sampleMs * 0.3);
        }

        public GameClient(IChannelHandlerContext context)
        {
            _channel = context;
        }

        public bool TryAuthenticate(string authTicket)
        {
            try {
                UserData userData = UserDataFactory.GetUserData(authTicket, out byte errorCode);
                if (errorCode == 1 || errorCode == 2) {
                    Disconnect();
                    return false;
                }

                #region Ban Checking

                //Let's have a quick search for a ban before we successfully authenticate..
                if (PlusEnvironment.GetGame().GetModerationManager().IsIpBanned(IpAddress, out _)) {
                    Disconnect();
                    return false;
                }

                if (userData.User != null) {
                    if (PlusEnvironment.GetGame().GetModerationManager().IsUserBanned(userData.UserId, out _)) {
                        Disconnect();
                        return false;
                    }
                }

                #endregion

                if (userData.User == null) //Possible NPE
                {
                    return false;
                }

                PlusEnvironment.GetGame().GetClientManager().RegisterClient(this, userData.UserId, userData.User.Username);
                _habbo = userData.User;
                if (_habbo != null) {
                    userData.User.Init(this, userData);

                    SendPacket(new AuthenticationOkComposer());
                    SendPacket(new AvatarEffectsComposer(_habbo.Effects().GetAllEffects));
                    SendPacket(new NavigatorSettingsComposer(_habbo.GetReconnectRoomId()));
                    SendPacket(new FavouritesComposer(userData.User.FavoriteRooms));
                    SendPacket(new FigureSetIdsComposer(_habbo.GetClothing().GetClothingParts));
                    SendPacket(new UserRightsComposer(_habbo.Rank));
                    SendPacket(new AvailabilityStatusComposer());
                    SendPacket(new AchievementScoreComposer(_habbo.GetStats().AchievementPoints));
                    SendPacket(new BuildersClubMembershipComposer());
                    SendPacket(new CfhTopicsInitComposer(PlusEnvironment.GetGame().GetModerationManager().UserActionPresets));
                    _habbo.TrySendUserStatsUpdate(true);

                    SendPacket(new BadgeDefinitionsComposer(PlusEnvironment.GetGame().GetAchievementManager().Achievements));
                    SendPacket(new SoundSettingsComposer(_habbo.ClientVolume, _habbo.ChatPreference, _habbo.AllowMessengerInvites, _habbo.FocusPreference, FriendBarStateUtility.GetInt(_habbo.FriendBarState)));
                    //SendMessage(new TalentTrackLevelComposer());

                    if (GetHabbo().GetMessenger() != null)
                        GetHabbo().GetMessenger().OnStatusChanged(true);

                    if (!string.IsNullOrEmpty(MachineId)) {
                        if (_habbo.MachineId != MachineId) {
                            int habboId = _habbo.Id;
                            string machineId = MachineId;
                            using WavePlusContext db = PlusEnvironment.GetDbContext();
                            db.Users.Where(u => u.Id == habboId).ExecuteUpdate(s => s.SetProperty(u => u.MachineId, machineId));
                        }

                        _habbo.MachineId = MachineId;
                    }

                    // Record the address this session actually connected from (X-Real-IP), so a later
                    // mip/ip ban targets a real IP rather than a stale registration address.
                    if (!string.IsNullOrEmpty(IpAddress)) {
                        int habboId = _habbo.Id;
                        string ip = IpAddress;
                        using WavePlusContext db = PlusEnvironment.GetDbContext();
                        db.Users.Where(u => u.Id == habboId).ExecuteUpdate(s => s.SetProperty(u => u.IpLast, ip));
                    }

                    if (PlusEnvironment.GetGame().GetPermissionManager().TryGetGroup(_habbo.Rank, out PermissionGroup group)) {
                        if (!string.IsNullOrEmpty(group.Badge))
                            if (!_habbo.GetBadgeComponent().HasBadge(group.Badge))
                                _habbo.GetBadgeComponent().GiveBadge(group.Badge, true, this);
                    }

                    if (PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(_habbo.VipRank, out SubscriptionData subData)) {
                        if (!string.IsNullOrEmpty(subData.Badge)) {
                            if (!_habbo.GetBadgeComponent().HasBadge(subData.Badge))
                                _habbo.GetBadgeComponent().GiveBadge(subData.Badge, true, this);
                        }
                    }

                    if (!PlusEnvironment.GetGame().GetCacheManager().ContainsUser(_habbo.Id))
                        PlusEnvironment.GetGame().GetCacheManager().GenerateUser(_habbo.Id);

                    _habbo.Look = PlusEnvironment.GetFigureManager().ProcessFigure(_habbo.Look, _habbo.Gender, _habbo.GetClothing().GetClothingParts, _habbo.IsVip, _habbo.GetPermissions().HasRight("clothing_no_validation"));
                    _habbo.InitProcess();

                    if (userData.User.GetPermissions().HasRight("mod_tickets")) {
                        SendPacket(new ModeratorInitComposer(
                            PlusEnvironment.GetGame().GetModerationManager().UserMessagePresets,
                            PlusEnvironment.GetGame().GetModerationManager().RoomMessagePresets,
                            PlusEnvironment.GetGame().GetModerationManager().GetTickets));
                    }

                    if (PlusEnvironment.GetSettingsManager().TryGetValue("user.login.message.enabled") == "1")
                        SendPacket(new MotdNotificationComposer(PlusEnvironment.GetLanguageManager().TryGetValue("user.login.message")));

                    PlusEnvironment.GetGame().GetRewardManager().CheckRewards(this);

                    // Evaluate "on enter hotel" achievements (registration duration, RP level, chat
                    // styles owned, happy hour).
                    Plus.HabboHotel.Roleplay.Utilities.RpAchievementHooks.OnEnterHotel(this);
                    return true;
                }
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }

            return false;
        }

        public void SendWhisper(string message, int colour = 0)
        {
            if (GetHabbo() == null || GetHabbo().CurrentRoom == null)
                return;

            RoomUser user = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().Username);
            if (user == null)
                return;

            SendPacket(new WhisperComposer(user.VirtualId, message, 0, (colour == 0 ? user.LastBubble : colour)));
        }

        public void SendNotification(string message)
        {
            SendPacket(new BroadcastMessageAlertComposer(message));
        }

        public void SendPacket(MessageComposer message)
        {
            // A ghost (grace-window) client has no live socket — silently drop its packets.
            if (_ghost)
                return;

            _channel.WriteAndFlushAsync(message);
        }

        public void SendPackets(List<MessageComposer> messages)
        {
            if (_ghost || messages == null || messages.Count == 0)
                return;

            foreach (MessageComposer message in messages) {
                if (message != null)
                    _channel.WriteAsync(message);
            }

            _channel.Flush();
        }

        public async void SendPacketsAsync(List<MessageComposer> messages)
        {
            foreach (MessageComposer message in messages) {
                await _channel.WriteAsync(message);
            }

            _channel.Flush();
        }

        public Habbo GetHabbo()
        {
            return _habbo;
        }

        public bool Disconnect(bool immediate = false)
        {
            if (_disconnected)
                return false;

            if (!immediate && _habbo != null &&
                PlusEnvironment.GetDisconnectDelayManager() != null &&
                PlusEnvironment.GetDisconnectDelayManager().Begin(this, _channel?.Channel?.Id)) {
                _ghost = true;
                PlusEnvironment.GetGame().GetClientManager().DropChannel(_channel?.Channel?.Id);
                return true;
            }

            _disconnected = true;

            try {
                if (GetHabbo() != null) {
                    try {
                        using WavePlusContext db = PlusEnvironment.GetDbContext();
                        db.Database.ExecuteSql(GetHabbo().GetQueryString);
                    } catch (Exception e) {
                        ExceptionLogger.LogException(e);
                    }

                    GetHabbo()?.OnDisconnect();
                }
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }

            _channel?.CloseAsync();
            return false;
        }

        public void Dispose()
        {
            if (GetHabbo() != null)
                GetHabbo().OnDisconnect();

            MachineId = string.Empty;
            _disconnected = true;
            _habbo = null;
            _channel.DisconnectAsync();
        }

        public void EnableEncryption(byte[] sharedKey)
        {
            _channel.Channel.Pipeline.AddFirst("gameCrypto", new EncryptionDecoder(sharedKey));
        }
    }
}