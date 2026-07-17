using DotNetty.Transport.Channels;
using MySqlX.XDevAPI;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Core;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using System;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.GameClients
{
    /// <summary>
    /// Delays a user's real logout by a grace window. When a socket drops (or the user logs out),
    /// instead of tearing the session down immediately we keep the avatar in the room as a "ghost":
    /// still fully present and attackable/killable, but with no live client behind it. If nobody
    /// reconnects before the window expires we run the real teardown; if the user reconnects first
    /// the ghost is finalized up-front (persisting the cached state) so the fresh login loads it.
    ///
    /// This stops players from instantly disconnecting to dodge combat, arrest, robbery, etc.
    /// </summary>
    public class DisconnectDelayManager
    {
        /// <summary>How long (seconds) a disconnected user lingers before the real teardown runs.</summary>
        public const int GraceSeconds = 10;

        /// <summary>
        /// Window (seconds) within which we won't re-announce a disconnect for the same user. Wider
        /// than the grace window so a reconnect-flap on browser close (auto-reconnect finalizing one
        /// ghost then the real close spawning another, seconds apart) only shouts once.
        /// </summary>
        private const int AnnounceCooldownSeconds = 45;

        private sealed class PendingDisconnect
        {
            public GameClient Client;
            public IChannelId ChannelId;
            public double ExpiryUnix;
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

            try
            {
                if (PlusEnvironment.GetPoliceManager()?.IsEscorted(habbo.Id) == true)
                {
                    PlusEnvironment.GetPoliceManager().StopEscort(habbo.Id);
                    PlusEnvironment.GetRpCrimeManager()?.Commit(habbo, "logout", habbo.CurrentRoom);
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }

            try
            {
                double now = PlusEnvironment.GetUnixTimestamp();
                bool announcedRecently = _lastAnnounce.TryGetValue(habbo.Id, out double last) && (now - last) < AnnounceCooldownSeconds;

                Room room = habbo.CurrentRoom;
                RoomUser roomUser = room?.GetRoomUserManager()?.GetRoomUserByHabbo(habbo.Id);
                if (roomUser != null && !announcedRecently)
                {
                    roomUser.ApplyEffect(13);
                    _lastAnnounce[habbo.Id] = now;
                    room.SendPacket(new ShoutComposer(roomUser.VirtualId, "*disconnecting in " + GraceSeconds + " seconds*", 0, habbo.CustomBubbleId, isRpAction: true));
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }

            return true;
        }

        public void FinalizeNow(int userId)
        {
            if (!_pending.TryRemove(userId, out PendingDisconnect pending))
                return;

            try
            {
                PlusEnvironment.GetGame().GetClientManager().CompleteGhostDisconnect(pending.Client, pending.ChannelId);
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public void OnCycle()
        {
            if (_pending.IsEmpty)
                return;

            double now = PlusEnvironment.GetUnixTimestamp();
            foreach (var kv in _pending)
            {
                if (kv.Value.ExpiryUnix <= now)
                    FinalizeNow(kv.Key);
            }
        }
    }
}
