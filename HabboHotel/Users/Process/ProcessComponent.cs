using log4net;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Database.EF;
using System;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace Plus.HabboHotel.Users.Process
{
    internal sealed class ProcessComponent
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessComponent));

        private Habbo _player;

        private Timer _timer;

        private bool _timerRunning;

        private bool _disabled;

        private readonly AutoResetEvent _resetEvent = new(true);

        private const int RuntimeInSec = 60;

        public bool Init(Habbo player)
        {
            if (player == null)
                return false;
            if (_player != null)
                return false;

            _player = player;
            _timer = new Timer(Run, null, RuntimeInSec * 1000, RuntimeInSec * 1000);
            return true;
        }

        public void Run(object state)
        {
            try {
                if (_disabled)
                    return;

                if (_timerRunning) {
                    Log.Warn("<Player " + _player.Id + "> Server can't keep up, Player timer is lagging behind.");
                    return;
                }

                _resetEvent.Reset();

                // BEGIN CODE

                #region Muted Checks

                if (_player.TimeMuted > 0)
                    _player.TimeMuted -= 60;

                #endregion

                #region Console Checks

                if (_player.MessengerSpamTime > 0)
                    _player.MessengerSpamTime -= 60;
                if (_player.MessengerSpamTime <= 0)
                    _player.MessengerSpamCount = 0;

                #endregion

                _player.TimeAfk += 1;

                #region Respect checking

                if (_player.GetStats().RespectsTimestamp != DateTime.Today.ToString("MM/dd")) {
                    _player.GetStats().RespectsTimestamp = DateTime.Today.ToString("MM/dd");
                    int dailyPoints = (_player.Rank == 1 && _player.VipRank == 0 ? 10 : _player.VipRank == 1 ? 15 : 20);
                    string respectsTimestamp = DateTime.Today.ToString("MM/dd");
                    int playerId = _player.Id;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                        db.UserStats.Where(s => s.Id == playerId).ExecuteUpdate(s => s
                            .SetProperty(x => x.DailyRespectPoints, dailyPoints)
                            .SetProperty(x => x.DailyPetRespectPoints, dailyPoints)
                            .SetProperty(x => x.RespectsTimestamp, respectsTimestamp));

                    _player.GetStats().DailyRespectPoints = (_player.Rank == 1 && _player.VipRank == 0 ? 10 : _player.VipRank == 1 ? 15 : 20);
                    _player.GetStats().DailyPetRespectPoints = (_player.Rank == 1 && _player.VipRank == 0 ? 10 : _player.VipRank == 1 ? 15 : 20);

                    if (_player.GetClient() != null) {
                        _player.GetClient().SendPacket(new UserObjectComposer(_player));
                    }
                }

                #endregion

                #region Reset Scripting Warnings

                if (_player.GiftPurchasingWarnings < 15)
                    _player.GiftPurchasingWarnings = 0;

                if (_player.MottoUpdateWarnings < 15)
                    _player.MottoUpdateWarnings = 0;

                if (_player.ClothingUpdateWarnings < 15)
                    _player.ClothingUpdateWarnings = 0;

                #endregion

                // Batched: with thousands of users online, one synchronous achievement write
                // per user per minute was a constant background load on MySQL.
                if (_player.GetClient() != null)
                    PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(_player.GetClient(), "ACH_AllTimeHotelPresence", 1);

                _player.CheckCreditsTimer();
                _player.CheckBankSaveTimer();

                if (_player.VipRank == 1 && !_player.IsVip)
                    _player.ExpireVip();

                _player.Effects().CheckEffectExpiry(_player);

                _player.GetRpStats()?.TickHunger(_player);

                PlusEnvironment.GetGame().GetShiftManager().Tick(_player);

                // END CODE

                // Reset the values
                _timerRunning = false;

                _resetEvent.Set();
            } catch {
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

            // Null the player so we don't reference it here anymore
            _player = null;
        }
    }
}