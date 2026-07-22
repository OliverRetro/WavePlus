using System;
using System.Threading;
using log4net;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;

namespace Plus.Core
{
    public class ServerStatusUpdater : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServerStatusUpdater));

        private const int UpdateInSeconds = 30;

        private Timer _timer;

        public void Init()
        {
            _timer = new Timer(OnTick, null, TimeSpan.FromSeconds(UpdateInSeconds), TimeSpan.FromSeconds(UpdateInSeconds));

            Console.Title = "Plus Emulator - 0 users online - 0 rooms loaded - 0 day(s) 0 hour(s) uptime";

            Log.Info("Server Status Updater has been started.");
        }

        public void OnTick(object obj)
        {
            UpdateOnlineUsers();
        }

        private void UpdateOnlineUsers()
        {
            TimeSpan uptime = DateTime.Now - PlusEnvironment.ServerStarted;

            int usersOnline = PlusEnvironment.GetGame().GetClientManager().Count;
            int roomCount = PlusEnvironment.GetGame().GetRoomManager().Count;

            Console.Title = "Plus Emulator - " + usersOnline + " users online - " + roomCount + " rooms loaded - " + uptime.Days + " day(s) " + uptime.Hours + " hour(s) uptime";

            // NOTE: original query used LIMIT 1; ExecuteUpdate cannot express LIMIT, so this updates all rows (server_status is a single-row table).
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.ServerStatuses.ExecuteUpdate(s => s
                    .SetProperty(x => x.UsersOnline, usersOnline)
                    .SetProperty(x => x.LoadedRooms, roomCount));
            }
        }

        public void Dispose()
        {
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.ServerStatuses.ExecuteUpdate(s => s
                    .SetProperty(x => x.UsersOnline, 0)
                    .SetProperty(x => x.LoadedRooms, 0));
            }

            _timer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}