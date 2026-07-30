using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Plus.Core;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Cache.Process
{
    internal sealed class ProcessComponent
    {
        private Timer _timer;

        private bool _timerRunning;

        private bool _disabled;

        private readonly AutoResetEvent _resetEvent = new(true);

        private const int RuntimeInSec = 1200;

        public void Init()
        {
            _timer = new Timer(Run, null, RuntimeInSec * 1000, RuntimeInSec * 1000);
        }

        public void Run(object state)
        {
            try {
                if (_disabled)
                    return;

                if (_timerRunning) {
                    return;
                }

                _resetEvent.Reset();

                // BEGIN CODE
                List<UserCache> cacheList = PlusEnvironment.GetGame().GetCacheManager().GetUserCache().ToList();
                if (cacheList.Count > 0) {
                    foreach (UserCache cache in cacheList) {
                        try {
                            if (cache == null)
                                continue;

                            if (cache.IsExpired())
                                PlusEnvironment.GetGame().GetCacheManager().TryRemoveUser(cache.Id, out _);
                        } catch (Exception e) {
                            ExceptionLogger.LogException(e);
                        }
                    }
                }

                List<Habbo> cachedUsers = PlusEnvironment.GetUsersCached().ToList();
                if (cachedUsers.Count > 0) {
                    foreach (Habbo data in cachedUsers) {
                        try {
                            if (data == null)
                                continue;

                            Habbo temp = null;

                            if (data.CacheExpired())
                                PlusEnvironment.RemoveFromCache(data.Id, out temp);

                            temp?.Dispose();
                        } catch (Exception e) {
                            ExceptionLogger.LogException(e);
                        }
                    }
                }
                // END CODE

                // Reset the values
                _timerRunning = false;

                _resetEvent.Set();
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }
        }

        public void Dispose()
        {
            // Wait until any processing is complete first.
            try {
                _resetEvent.WaitOne(TimeSpan.FromMinutes(5));
            } catch {
            } // give up

            // Set the timer to disabled
            _disabled = true;

            // Dispose the timer to disable it.
            try {
                _timer?.Dispose();
            } catch {
            }

            // Remove reference to the timer.
            _timer = null;
        }
    }
}