using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Quests
{
    public class QuestListComposer : MessageComposer
    {
        public Habbo Habbo { get; }
        public bool Send { get; }
        public Dictionary<string, Quest> UserQuests { get; }

        public QuestListComposer(GameClient session, bool send, Dictionary<string, Quest> userQuests)
            : base(ServerPacketHeader.QuestListMessageComposer)
        {
            Habbo = session.GetHabbo();
            Send = send;
            UserQuests = userQuests;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(UserQuests.Count);

            // Active ones first
            foreach (var userQuest in UserQuests) {
                if (userQuest.Value == null)
                    continue;

                SerializeQuest(packet, Habbo, userQuest.Value, userQuest.Key);
            }

            // Dead ones last
            foreach (var userQuest in UserQuests) {
                if (userQuest.Value != null)
                    continue;

                SerializeQuest(packet, Habbo, userQuest.Value, userQuest.Key);
            }

            packet.WriteBoolean(Send);
        }

        public static void SerializeQuest(ServerPacket message, Habbo habbo, Quest quest, string category)
        {
            if (message == null || habbo == null)
                return;

            int amountInCat = PlusEnvironment.GetGame().GetQuestManager().GetAmountOfQuestsInCategory(category);
            int number = quest == null ? amountInCat : quest.Number - 1;
            int userProgress = quest == null ? 0 : habbo.GetQuestProgress(quest.Id);

            if (quest != null && quest.IsCompleted(userProgress))
                number++;

            message.WriteString(category);
            message.WriteInteger(quest == null ? 0 : ((quest.Category.Contains("xmas2012")) ? 0 : number)); // Quest progress in this cat
            message.WriteInteger(quest == null ? 0 : (quest.Category.Contains("xmas2012")) ? 0 : amountInCat); // Total quests in this cat
            message.WriteInteger(quest == null ? 3 : quest.RewardType); // Reward type (1 = Snowflakes, 2 = Love hearts, 3 = Pixels, 4 = Seashells, everything else is pixels
            message.WriteInteger(quest == null ? 0 : quest.Id); // Quest id
            message.WriteBoolean(quest == null ? false : habbo.GetStats().QuestId == quest.Id); // Quest started
            message.WriteString(quest == null ? string.Empty : quest.ActionName);
            message.WriteString(quest == null ? string.Empty : quest.DataBit);
            message.WriteInteger(quest == null ? 0 : quest.Reward);
            message.WriteString(quest == null ? string.Empty : quest.Name);
            message.WriteInteger(userProgress); // Current progress
            message.WriteInteger(quest == null ? 0 : quest.GoalData); // Target progress
            message.WriteInteger(quest == null ? 0 : quest.TimeUnlock); // "Next quest available countdown" in seconds
            message.WriteString("");
            message.WriteString("");
            message.WriteBoolean(true);
        }
    }
}