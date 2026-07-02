using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database.Interfaces;
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
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `server_rewards` WHERE enabled = '1'");
                DataTable dTable = dbClient.GetTable();
                if (dTable != null) {
                    foreach (DataRow dRow in dTable.Rows)
                    {
                        _rewards.TryAdd((int) dRow["id"], new Reward(Convert.ToDouble(dRow["reward_start"]), Convert.ToDouble(dRow["reward_end"]), Convert.ToString(dRow["reward_type"]), Convert.ToString(dRow["reward_data"]), Convert.ToString(dRow["message"])));
                    }
                }

                dbClient.SetQuery("SELECT * FROM `server_reward_logs`");
                dTable = dbClient.GetTable();
                if (dTable != null) {
                    foreach (DataRow dRow in dTable.Rows)
                    {
                        int id = (int) dRow["user_id"];
                        int rewardId = (int) dRow["reward_id"];

                        if (!_rewardLogs.ContainsKey(id))
                            _rewardLogs.TryAdd(id, new List<int>());

                        if (!_rewardLogs[id].Contains(rewardId))
                            _rewardLogs[id].Add(rewardId);
                    }
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

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `server_reward_logs` VALUES ('', @userId, @rewardId)");
                dbClient.AddParameter("userId", id);
                dbClient.AddParameter("rewardId", rewardId);
                dbClient.RunQuery();
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