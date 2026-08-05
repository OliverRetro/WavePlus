using DotNetty.Transport.Channels;
using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Core;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using System;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.GameClients
{
    public class DisconnectDelayManager
    {
        public const int GraceSeconds = 10;

        private const int AnnounceCooldownSeconds = 45;

        private const int DissolveLeadSeconds = 3;

        private sealed class PendingDisconnect
        {
            public GameClient Client;
            public IChannelId ChannelId;
            public double ExpiryUnix;
            public bool DissolveSent;
        }

        private readonly ConcurrentDictionary<int, PendingDisconnect> _pending = new();

        private readonly ConcurrentDictionary<int, double> _lastAnnounce = new();
        public bool IsGhost(int userId) => _pending.ContainsKey(userId);

        public bool Begin(GameClient client, IChannelId channelId)
        {
            Habbo habbo = client?.GetHabbo();
            if (habbo == null)
                return false;

            PendingDisconnect pending = new()
            {
                Client = client,
                ChannelId = channelId,
                ExpiryUnix = PlusEnvironment.GetUnixTimestamp() + GraceSeconds
            };

            if (!_pending.TryAdd(habbo.Id, pending))
                return true;

            try {
                if (PlusEnvironment.GetPoliceManager()?.IsEscorted(habbo.Id) == true) {
                    PlusEnvironment.GetPoliceManager().StopEscort(habbo.Id);
                    PlusEnvironment.GetRpCrimeManager()?.Commit(habbo, "logout", habbo.CurrentRoom);
                }
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }

            try {
                double now = PlusEnvironment.GetUnixTimestamp();
                bool announcedRecently = _lastAnnounce.TryGetValue(habbo.Id, out double last) && (now - last) < AnnounceCooldownSeconds;

                Room room = habbo.CurrentRoom;
                RoomUser roomUser = room?.GetRoomUserManager()?.GetRoomUserByHabbo(habbo.Id);
                if (roomUser != null && !announcedRecently) {
                    roomUser.ApplyEffect(13);
                    _lastAnnounce[habbo.Id] = now;
                    room.SendPacket(new ShoutComposer(roomUser.VirtualId, "*disconnecting in " + GraceSeconds + " seconds*", 0, habbo.CustomBubbleId, isRpAction: true));
                }
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }

            return true;
        }

        public void FinalizeNow(int userId)
        {
            if (!_pending.TryRemove(userId, out PendingDisconnect pending))
                return;

            try {
                PlusEnvironment.GetGame().GetClientManager().CompleteGhostDisconnect(pending.Client, pending.ChannelId);
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }
        }

        private void TrySendDissolve(int userId)
        {
            try {
                if (!_pending.TryGetValue(userId, out PendingDisconnect pending))
                    return;

                Habbo habbo = pending.Client?.GetHabbo();
                Room room = habbo?.CurrentRoom;
                RoomUser roomUser = room?.GetRoomUserManager()?.GetRoomUserByHabbo(habbo.Id);
                if (roomUser != null)
                    room.SendPacket(new TeleportDissolveComposer(roomUser.VirtualId, rebuild: false));
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }
        }

        public void OnCycle()
        {
            double now = PlusEnvironment.GetUnixTimestamp();

            // Prune stale announce cooldowns — the map otherwise grows one entry per user who
            // ever ghost-disconnected, for the lifetime of the process.
            if (!_lastAnnounce.IsEmpty) {
                foreach (var kv in _lastAnnounce) {
                    if (now - kv.Value > AnnounceCooldownSeconds * 4)
                        _lastAnnounce.TryRemove(kv.Key, out _);
                }
            }

            if (_pending.IsEmpty)
                return;

            foreach (var kv in _pending) {
                PendingDisconnect pending = kv.Value;

                if (pending.ExpiryUnix <= now) {
                    FinalizeNow(kv.Key);
                    continue;
                }

                if (!pending.DissolveSent && (pending.ExpiryUnix - now) <= DissolveLeadSeconds) {
                    pending.DissolveSent = true;
                    TrySendDissolve(kv.Key);
                }
            }
        }
    }
}