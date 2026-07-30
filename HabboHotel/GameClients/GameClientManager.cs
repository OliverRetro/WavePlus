using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DotNetty.Transport.Channels;
using log4net;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Core;
using Plus.Database.EF;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.HabboHotel.GameClients
{
    public class GameClientManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GameClientManager));

        private readonly ConcurrentDictionary<IChannelId, GameClient> _clients;
        private readonly ConcurrentDictionary<int, GameClient> _userIdRegister;
        private readonly ConcurrentDictionary<string, GameClient> _usernameRegister;

        private readonly Queue _timedOutConnections;

        private readonly Stopwatch _clientPingStopwatch;

        public GameClientManager()
        {
            _clients = new ConcurrentDictionary<IChannelId, GameClient>();
            _userIdRegister = new ConcurrentDictionary<int, GameClient>();
            _usernameRegister = new ConcurrentDictionary<string, GameClient>();

            _timedOutConnections = new Queue();

            _clientPingStopwatch = new Stopwatch();
            _clientPingStopwatch.Start();
        }

        public void OnCycle()
        {
            TestClientConnections();
            HandleTimeouts();
            PlusEnvironment.GetDisconnectDelayManager()?.OnCycle();
        }

        public GameClient GetClientByUserId(int userId)
        {
            return _userIdRegister.TryGetValue(userId, out GameClient client) ? client : null;
        }

        public GameClient GetClientByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            return _usernameRegister.TryGetValue(username.ToLower(), out GameClient client) ? client : null;
        }

        public bool TryGetClient(IChannelId clientId, out GameClient client)
        {
            return _clients.TryGetValue(clientId, out client);
        }

        public bool UpdateClientUsername(GameClient client, string oldUsername, string newUsername)
        {
            if (client == null || !_usernameRegister.ContainsKey(oldUsername.ToLower()))
                return false;

            _usernameRegister.TryRemove(oldUsername.ToLower(), out client);
            _usernameRegister.TryAdd(newUsername.ToLower(), client);
            return true;
        }

        public string GetNameById(int id)
        {
            GameClient client = GetClientByUserId(id);

            if (client != null)
                return client.GetHabbo().Username;

            string username;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                username = db.Users.Where(u => u.Id == id).Select(u => u.Username).FirstOrDefault();
            }

            return username;
        }

        public IEnumerable<GameClient> GetClientsById(Dictionary<int, MessengerBuddy>.KeyCollection users)
        {
            foreach (int id in users) {
                GameClient client = GetClientByUserId(id);
                if (client != null)
                    yield return client;
            }
        }

        public void StaffAlert(MessageComposer message, int exclude = 0)
        {
            foreach (GameClient client in GetClients.ToList()) {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (client.GetHabbo().Rank < 2 || client.GetHabbo().Id == exclude)
                    continue;

                client.SendPacket(message);
            }
        }

        public void ModAlert(string message)
        {
            foreach (GameClient client in GetClients.ToList()) {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (client.GetHabbo().GetPermissions().HasRight("mod_tool") &&
                    !client.GetHabbo().GetPermissions().HasRight("staff_ignore_mod_alert")) {
                    try {
                        client.SendWhisper(message, 5);
                    } catch {
                    }
                }
            }
        }

        public void DoAdvertisingReport(GameClient reporter, GameClient target)
        {
            if (reporter == null || target == null || reporter.GetHabbo() == null || target.GetHabbo() == null)
                return;

            StringBuilder builder = new();
            builder.Append("New report submitted!\r\r");
            builder.Append("Reporter: " + reporter.GetHabbo().Username + "\r");
            builder.Append("Reported User: " + target.GetHabbo().Username + "\r\r");
            builder.Append(target.GetHabbo().Username + "s last 10 messages:\r\r");

            uint targetId = (uint)target.GetHabbo().Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                int number = 11;
                foreach (string message in db.Chatlogs.Where(c => c.UserId == targetId).OrderByDescending(c => c.Id).Take(10).Select(c => c.Message).ToList()) {
                    number -= 1;
                    builder.Append(number + ": " + message + "\r");
                }
            }

            foreach (GameClient client in GetClients.ToList()) {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (client.GetHabbo().GetPermissions().HasRight("mod_tool") && !client.GetHabbo().GetPermissions()
                    .HasRight("staff_ignore_advertisement_reports"))
                    client.SendPacket(new MotdNotificationComposer(builder.ToString()));
            }
        }

        public void SendPacket(MessageComposer packet, string fuse = "")
        {
            foreach (GameClient client in _clients.Values) {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (!string.IsNullOrEmpty(fuse)) {
                    if (!client.GetHabbo().GetPermissions().HasRight(fuse))
                        continue;
                }

                client.SendPacket(packet);
            }
        }

        public void CreateAndStartClient(IChannelHandlerContext connection)
        {
            GameClient client = new(connection);
            if (_clients.TryAdd(connection.Channel.Id, client)) {
                //Hmmmmm?
            } else
                connection.CloseAsync();
        }

        public void DisposeConnection(IChannelId clientId)
        {
            if (!TryGetClient(clientId, out GameClient client))
                return;

            // A logged-in user enters the disconnect grace window rather than being torn down now
            // (so they can't dodge combat/arrest by dropping). Keep the client object alive in the
            // grace registry — only drop the dead channel mapping here; the real teardown runs when
            // the window expires (or up-front on reconnect) via CompleteGhostDisconnect.
            bool ghosted = client != null && client.Disconnect();
            if (ghosted) {
                _clients.TryRemove(clientId, out _);
                return;
            }

            client?.Dispose();
            _clients.TryRemove(clientId, out client);
        }

        public void CompleteGhostDisconnect(GameClient client, IChannelId channelId)
        {
            if (client == null)
                return;

            // Capture the room up-front: the heavy disconnect save below (RP stats, bank, crime
            // flush, user row) can throw, and if it does before Habbo.Dispose runs the ghost's
            // avatar would linger in the room forever. We remove it explicitly as a safety net.
            Plus.HabboHotel.Users.Habbo habbo = client.GetHabbo();
            Plus.HabboHotel.Rooms.Room room = habbo?.CurrentRoom;

            client.Disconnect(immediate: true);

            // If the normal teardown already pulled the user from the room this is a no-op
            // (GetRoomUserByHabbo returns null); otherwise force the removal now. Runs BEFORE
            // Dispose, which nulls the client's Habbo reference.
            try {
                if (room != null && habbo != null && room.GetRoomUserManager()?.GetRoomUserByHabbo(habbo.Id) != null)
                    room.GetRoomUserManager().RemoveUserFromRoom(client, false);
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }

            client.Dispose();

            if (channelId != null)
                _clients.TryRemove(channelId, out _);
        }

        public void DropChannel(IChannelId channelId)
        {
            if (channelId != null)
                _clients.TryRemove(channelId, out _);
        }

        public void LogClonesOut(int userId)
        {
            GameClient client = GetClientByUserId(userId);
            client?.Disconnect(immediate: true);
        }

        public void RegisterClient(GameClient client, int userId, string username)
        {
            if (client == null || string.IsNullOrWhiteSpace(username))
                return;

            _usernameRegister[username.ToLower()] = client;
            _userIdRegister[userId] = client;
        }

        public void UnregisterClient(int userId, string username)
        {
            _userIdRegister.TryRemove(userId, out GameClient _);
            _usernameRegister.TryRemove(username.ToLower(), out GameClient _);
        }

        public void CloseAll()
        {
            Log.Info("<<- SERVER SHUTDOWN ->> Running disconnection saves for every online user...");

            foreach (GameClient client in GetClients.ToList()) {
                var habbo = client?.GetHabbo();
                if (habbo == null)
                    continue;

                try {
                    // Run the full disconnection save (RP stats, items, weapons, skills, bank,
                    // shift end, crime-log flush, user row) — the same path a normal logout takes.
                    habbo.OnDisconnect();
                    Log.Info(habbo.Username + " has been disconnected.");
                } catch (Exception e) {
                    ExceptionLogger.LogException(e);
                }
            }

            Log.Info("Done saving all online users!");
            Log.Info("Closing server connections...");
            try {
                foreach (GameClient client in GetClients.ToList()) {
                    if (client == null)
                        continue;

                    try {
                        client.Dispose();
                    } catch {
                    }
                }

                Console.Clear();
                Log.Info("<<- SERVER SHUTDOWN ->> CLOSING CONNECTIONS");
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }

            if (_clients.Count > 0)
                _clients.Clear();

            Log.Info("Connections closed!");
        }

        private void TestClientConnections()
        {
            if (_clientPingStopwatch.ElapsedMilliseconds >= 30000) {
                _clientPingStopwatch.Restart();

                List<GameClient> toPing = new();

                foreach (GameClient client in _clients.Values) {
                    if (client.PingCount < 6) {
                        client.PingCount++;

                        toPing.Add(client);
                    } else {
                        lock (_timedOutConnections.SyncRoot) {
                            _timedOutConnections.Enqueue(client);
                        }
                    }
                }

                DateTime start = DateTime.Now;

                foreach (GameClient client in toPing) {
                    try {
                        // Time the probe: the reply (PingEvent) turns it into a latency sample.
                        client.KeepAlivePingAt = System.Diagnostics.Stopwatch.GetTimestamp();
                        client.SendPacket(new PongComposer());
                    } catch {
                        lock (_timedOutConnections.SyncRoot) {
                            _timedOutConnections.Enqueue(client);
                        }
                    }
                }
            }
        }

        private void HandleTimeouts()
        {
            if (_timedOutConnections.Count > 0) {
                lock (_timedOutConnections.SyncRoot) {
                    while (_timedOutConnections.Count > 0) {
                        GameClient client = null;

                        if (_timedOutConnections.Count > 0)
                            client = (GameClient)_timedOutConnections.Dequeue();

                        client?.Disconnect();
                    }
                }
            }
        }

        public int Count => _clients.Count;

        public ICollection<GameClient> GetClients => _clients.Values;
    }
}