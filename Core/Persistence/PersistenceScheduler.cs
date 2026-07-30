using System;
using System.Collections.Generic;
using System.Threading;
using log4net;

namespace Plus.Core.Persistence
{
    public sealed class PersistenceScheduler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PersistenceScheduler));

        private static readonly TimeSpan UserInterval = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan WorldInterval = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan MirrorInterval = TimeSpan.FromSeconds(60);

        private readonly List<(string Name, Action Save)> _userSavers = [];
        private readonly List<(string Name, Action Save)> _worldSavers = [];
        private readonly List<(string Name, Action Save)> _mirrors = [];
        private readonly object _userLock = new();
        private readonly object _worldLock = new();
        private readonly object _mirrorLock = new();

        private Timer _userTimer;
        private Timer _worldTimer;
        private Timer _mirrorTimer;

        public void RegisterUserSaver(string name, Action save)
        {
            _userSavers.Add((name, save));
        }

        public void RegisterWorldSaver(string name, Action save)
        {
            _worldSavers.Add((name, save));
        }

        public void RegisterMirror(string name, Action mirror)
        {
            _mirrors.Add((name, mirror));
        }

        public void Start()
        {
            _userTimer = new Timer(_ => RunUserSavers(), null, UserInterval, UserInterval);
            _worldTimer = new Timer(_ => RunWorldSavers(), null, WorldInterval, WorldInterval);
            _mirrorTimer = new Timer(_ => RunMirrors(), null, MirrorInterval, MirrorInterval);
            Log.Info($"Persistence scheduler started (user flush every {UserInterval.TotalMinutes}m, world flush every {WorldInterval.TotalMinutes}m, Redis mirror every {MirrorInterval.TotalSeconds}s).");
        }

        private void RunUserSavers()
        {
            // Timer callbacks can overlap if a flush runs long; the lock serialises them.
            if (!Monitor.TryEnter(_userLock))
                return;

            try {
                RunAll(_userSavers, "user");
            } finally {
                Monitor.Exit(_userLock);
            }
        }

        private void RunWorldSavers()
        {
            if (!Monitor.TryEnter(_worldLock))
                return;

            try {
                RunAll(_worldSavers, "world");
            } finally {
                Monitor.Exit(_worldLock);
            }
        }

        private void RunMirrors()
        {
            if (!Monitor.TryEnter(_mirrorLock))
                return;

            try {
                RunAll(_mirrors, "mirror");
            } finally {
                Monitor.Exit(_mirrorLock);
            }
        }

        private static void RunAll(List<(string Name, Action Save)> savers, string label)
        {
            foreach ((string name, Action save) in savers) {
                try {
                    save();
                } catch (Exception ex) {
                    // Include the inner exception: provider wrappers (e.g. the MySQL provider's
                    // "configure your entity type accordingly") hide the real cause otherwise.
                    string detail = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
                    Log.Error($"Persistence {label} saver '{name}' failed: {detail}", ex);
                }
            }
        }

        public void FlushAll()
        {
            lock (_userLock)
                RunAll(_userSavers, "user");
            lock (_worldLock)
                RunAll(_worldSavers, "world");
        }

        public void Stop()
        {
            _userTimer?.Dispose();
            _worldTimer?.Dispose();
            _mirrorTimer?.Dispose();
            _userTimer = null;
            _worldTimer = null;
            _mirrorTimer = null;
        }
    }
}