using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DotNetty.Transport.Channels;
using log4net;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Core;
using Plus.Database.Interfaces;
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
        }

        public GameClient GetClientByUserId(int userId)
        {
            if (_userIdRegister.ContainsKey(userId))
                return _userIdRegister[userId];
            return null;
        }

        public GameClient GetClientByUsername(string username)
        {
            if (_usernameRegister.ContainsKey(username.ToLower()))
                return _usernameRegister[username.ToLower()];
            return null;
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
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT username FROM users WHERE id = @id LIMIT 1");
                dbClient.AddParameter("id", id);
                username = dbClient.GetString();
            }

            return username;
        }

        public IEnumerable<GameClient> GetClientsById(Dictionary<int, MessengerBuddy>.KeyCollection users)
        {
            foreach (int id in users)
            {
                GameClient client = GetClientByUserId(id);
                if (client != null)
                    yield return client;
            }
        }

        public void StaffAlert(MessageComposer message, int exclude = 0)
        {
            foreach (GameClient client in GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (client.GetHabbo().Rank < 2 || client.GetHabbo().Id == exclude)
                    continue;

                client.SendPacket(message);
            }
        }

        public void ModAlert(string message)
        {
            foreach (GameClient client in GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (client.GetHabbo().GetPermissions().HasRight("mod_tool") &&
                    !client.GetHabbo().GetPermissions().HasRight("staff_ignore_mod_alert"))
                {
                    try
                    {
                        client.SendWhisper(message, 5);
                    }
                    catch
                    {
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

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `message` FROM `chatlogs` WHERE `user_id` = '" + target.GetHabbo().Id +
                                  "' ORDER BY `id` DESC LIMIT 10");
                int number = 11;
                var reader = dbClient.GetTable();
                foreach (DataRow row in reader.Rows)
                {
                    number -= 1;
                    builder.Append(number + ": " + row["message"] + "\r");
                }
            }

            foreach (GameClient client in GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (client.GetHabbo().GetPermissions().HasRight("mod_tool") && !client.GetHabbo().GetPermissions()
                    .HasRight("staff_ignore_advertisement_reports"))
                    client.SendPacket(new MotdNotificationComposer(builder.ToString()));
            }
        }


        public void SendPacket(MessageComposer packet, string fuse = "")
        {
            foreach (GameClient client in _clients.Values.ToList())
            {
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

            client?.Disconnect();
            client?.Dispose();

            _clients.TryRemove(clientId, out client);
        }

        public void LogClonesOut(int userId)
        {
            GameClient client = GetClientByUserId(userId);
            client?.Disconnect();
        }

        public void RegisterClient(GameClient client, int userId, string username)
        {
            if (_usernameRegister.ContainsKey(username.ToLower()))
                _usernameRegister[username.ToLower()] = client;
            else
                _usernameRegister.TryAdd(username.ToLower(), client);

            if (_userIdRegister.ContainsKey(userId))
                _userIdRegister[userId] = client;
            else
                _userIdRegister.TryAdd(userId, client);
        }

        public void UnregisterClient(int userId, string username)
        {
            _userIdRegister.TryRemove(userId, out GameClient _);
            _usernameRegister.TryRemove(username.ToLower(), out GameClient _);
        }

        public void CloseAll()
        {
            foreach (GameClient client in GetClients.ToList())
            {
                if (client?.GetHabbo() != null) {
                    try
                    {
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery(client.GetHabbo().GetQueryString);
                        }

                        Console.Clear();
                        Log.Info("<<- SERVER SHUTDOWN ->> INVENTORY IS SAVING");
                    }
                    catch
                    {
                    }
                }
            }

            Log.Info("Done saving users inventory!");
            Log.Info("Closing server connections...");
            try
            {
                foreach (GameClient client in GetClients.ToList())
                {
                    if (client == null)
                        continue;

                    try
                    {
                        client.Dispose();
                    }
                    catch
                    {
                    }
                }

                Console.Clear();
                Log.Info("<<- SERVER SHUTDOWN ->> CLOSING CONNECTIONS");
            }
            catch (Exception e)
            {
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

                foreach (GameClient client in _clients.Values.ToList())
                {
                    if (client.PingCount < 6) {
                        client.PingCount++;

                        toPing.Add(client);
                    } else {
                        lock (_timedOutConnections.SyncRoot)
                        {
                            _timedOutConnections.Enqueue(client);
                        }
                    }
                }

                DateTime start = DateTime.Now;

                foreach (GameClient client in toPing.ToList())
                {
                    try
                    {
                        client.SendPacket(new PongComposer());
                    }
                    catch
                    {
                        lock (_timedOutConnections.SyncRoot)
                        {
                            _timedOutConnections.Enqueue(client);
                        }
                    }
                }
            }
        }

        private void HandleTimeouts()
        {
            if (_timedOutConnections.Count > 0) {
                lock (_timedOutConnections.SyncRoot)
                {
                    while (_timedOutConnections.Count > 0)
                    {
                        GameClient client = null;

                        if (_timedOutConnections.Count > 0)
                            client = (GameClient) _timedOutConnections.Dequeue();

                        client?.Disconnect();
                    }
                }
            }
        }

        public int Count => _clients.Count;

        public ICollection<GameClient> GetClients => _clients.Values;
    }
}
