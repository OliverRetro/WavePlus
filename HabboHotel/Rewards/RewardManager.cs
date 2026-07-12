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
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var rewards = db.server_rewards.Where(r => r.enabled == "1")
                    .Select(r => new { r.id, r.reward_start, r.reward_end, r.reward_type, r.reward_data, r.message })
                    .ToList();
                foreach (var r in rewards)
                {
                    _rewards.TryAdd(r.id, new Reward(Convert.ToDouble(r.reward_start), Convert.ToDouble(r.reward_end), r.reward_type, r.reward_data, r.message));
                }

                var logs = db.server_reward_logs.Select(l => new { l.user_id, l.reward_id }).ToList();
                foreach (var l in logs)
                {
                    int id = l.user_id;
                    int rewardId = l.reward_id;

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

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.server_reward_logs.Add(new server_reward_log { user_id = id, reward_id = rewardId });
                db.SaveChanges();
            }
        }

        public void CheckRewards(GameClient session)
        {
            if (session == null || session.GetHabbo() == null)
                return;

            foreach (KeyValuePair<int, Reward> entry in _rewards)
            {
                int id = entry.Key;
                Reward reward = entry.Value;

                if (HasReward(session.GetHabbo().Id, id))
                    continue;

                if (reward.Active) {
                    switch (reward.Type)
                    {
                        case RewardType.Badge:
                        {
                            if (!session.GetHabbo().GetBadgeComponent().HasBadge(reward.RewardData))
                                session.GetHabbo().GetBadgeComponent().GiveBadge(reward.RewardData, true, session);
                            break;
                        }

                        case RewardType.Credits:
                        {
                            session.GetHabbo().Credits += Convert.ToInt32(reward.RewardData);
                            session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
                            break;
                        }

                        case RewardType.Duckets:
                        {
                            session.GetHabbo().Duckets += Convert.ToInt32(reward.RewardData);
                            session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, Convert.ToInt32(reward.RewardData)));
                            break;
                        }

                        case RewardType.Diamonds:
                        {
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
