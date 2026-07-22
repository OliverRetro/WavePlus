using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rewards
{
    public class RewardManager
    {
        private readonly ConcurrentDictionary<int, Reward> _rewards;
        private readonly ConcurrentDictionary<int, List<int>> _rewardLogs;

        public RewardManager()
        {
            _rewards = new ConcurrentDictionary<int, Reward>();
            _rewardLogs = new ConcurrentDictionary<int, List<int>>();
        }

        public void Init()
        {
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var rewards = db.ServerRewards.Where(r => r.Enabled == "1")
                    .Select(r => new { r.Id, r.RewardStart, r.RewardEnd, r.RewardType, r.RewardData, r.Message })
                    .ToList();
                foreach (var r in rewards) {
                    _rewards.TryAdd(r.Id, new Reward(Convert.ToDouble(r.RewardStart), Convert.ToDouble(r.RewardEnd), r.RewardType, r.RewardData, r.Message));
                }

                var logs = db.ServerRewardLogs.Select(l => new { l.UserId, l.RewardId }).ToList();
                foreach (var l in logs) {
                    int id = l.UserId;
                    int rewardId = l.RewardId;

                    if (!_rewardLogs.ContainsKey(id))
                        _rewardLogs.TryAdd(id, new List<int>());

                    if (!_rewardLogs[id].Contains(rewardId))
                        _rewardLogs[id].Add(rewardId);
                }
            }
        }

        public bool HasReward(int id, int rewardId)
        {
            return _rewardLogs.ContainsKey(id) && _rewardLogs[id].Contains(rewardId);
        }

        public void LogReward(int id, int rewardId)
        {
            if (!_rewardLogs.ContainsKey(id))
                _rewardLogs.TryAdd(id, new List<int>());

            if (!_rewardLogs[id].Contains(rewardId))
                _rewardLogs[id].Add(rewardId);

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.ServerRewardLogs.Add(new ServerRewardLogEntity { UserId = id, RewardId = rewardId });
                db.SaveChanges();
            }
        }

        public void CheckRewards(GameClient session)
        {
            if (session == null || session.GetHabbo() == null)
                return;

            foreach (KeyValuePair<int, Reward> entry in _rewards) {
                int id = entry.Key;
                Reward reward = entry.Value;

                if (HasReward(session.GetHabbo().Id, id))
                    continue;

                if (reward.Active) {
                    switch (reward.Type) {
                        case RewardType.Badge: {
                                if (!session.GetHabbo().GetBadgeComponent().HasBadge(reward.RewardData))
                                    session.GetHabbo().GetBadgeComponent().GiveBadge(reward.RewardData, true, session);
                                break;
                            }

                        case RewardType.Credits: {
                                session.GetHabbo().Credits += Convert.ToInt32(reward.RewardData);
                                session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
                                break;
                            }

                        case RewardType.Duckets: {
                                session.GetHabbo().Duckets += Convert.ToInt32(reward.RewardData);
                                session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, Convert.ToInt32(reward.RewardData)));
                                break;
                            }

                        case RewardType.Diamonds: {
                                session.GetHabbo().Diamonds += Convert.ToInt32(reward.RewardData);
                                session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Diamonds, Convert.ToInt32(reward.RewardData), 5));
                                break;
                            }
                    }

                    if (!string.IsNullOrEmpty(reward.Message))
                        session.SendNotification(reward.Message);

                    LogReward(session.GetHabbo().Id, id);
                } else
                    continue;
            }
        }
    }
}