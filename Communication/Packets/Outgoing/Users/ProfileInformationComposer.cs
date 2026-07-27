using System;
using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Level;
using Plus.HabboHotel.Roleplay.Skill;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Users
{
    internal class ProfileInformationComposer : MessageComposer
    {
        public Habbo TargetHabbo { get; }
        public List<Group> Groups { get; }
        public int FriendCount { get; }
        public Habbo MyHabbo { get; }
        public Group Gang { get; }
        public Group Corporation { get; }

        public ProfileInformationComposer(Habbo habbo, GameClient session, List<Group> groups, int friendCount)
            : base(ServerPacketHeader.ProfileInformationMessageComposer)
        {
            TargetHabbo = habbo;
            Groups = groups;
            FriendCount = friendCount;
            MyHabbo = session.GetHabbo();
            Gang = session.GetHabbo().GetStats().FavouriteGroupId != 0 && PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(session.GetHabbo().GetStats().FavouriteGroupId, out Group gang) ? gang : null;
            Corporation = groups.FirstOrDefault(g => g != null && GroupManager.IsWorkableKind(g.Kind));
        }

        public override void Compose(ServerPacket packet)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(TargetHabbo.AccountCreated);

            packet.WriteInteger(TargetHabbo.Id);
            packet.WriteString(TargetHabbo.Username);
            packet.WriteString(TargetHabbo.Look);
            packet.WriteString(TargetHabbo.Motto);
            packet.WriteString(origin.ToString("dd/MM/yyyy"));
            packet.WriteInteger(TargetHabbo.GetStats().AchievementPoints);
            packet.WriteInteger(FriendCount);
            packet.WriteBoolean(TargetHabbo.Id != MyHabbo.Id && MyHabbo.GetMessenger().FriendshipExists(TargetHabbo.Id));
            packet.WriteBoolean(TargetHabbo.Id != MyHabbo.Id && !MyHabbo.GetMessenger().FriendshipExists(TargetHabbo.Id) && MyHabbo.GetMessenger().RequestExists(TargetHabbo.Id));
            packet.WriteBoolean((PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(TargetHabbo.Id)) != null);

            packet.WriteInteger(Groups.Count);
            foreach (Group group in Groups) {
                packet.WriteInteger(group.Id);
                packet.WriteString(group.Name);
                packet.WriteString(group.Badge);
                packet.WriteString(PlusEnvironment.GetGame().GetGroupManager().GetColourCode(group.Colour1, true));
                packet.WriteString(PlusEnvironment.GetGame().GetGroupManager().GetColourCode(group.Colour2, false));
                packet.WriteBoolean(TargetHabbo.GetStats().FavouriteGroupId == group.Id);
                packet.WriteInteger(group.CreatorId);
                packet.WriteBoolean(group.CreatorId == TargetHabbo.Id);
                packet.WriteInteger((int)group.Kind);
            }
            packet.WriteInteger(Convert.ToInt32(PlusEnvironment.GetUnixTimestamp() - TargetHabbo.LastOnline));
            packet.WriteBoolean(true);
            packet.WriteBoolean(PlusEnvironment.GetGame().GetShiftManager().IsWorking(TargetHabbo.Id));
            packet.WriteString("");
            packet.WriteString(Corporation != null && Corporation.TryGetRoleData(TargetHabbo.Id, out GroupRole corpRole) ? corpRole.Name : "");
            packet.WriteString(Gang?.Name ?? "No gang");
            packet.WriteInteger(TargetHabbo.GetRpStats().Knockouts);
            packet.WriteInteger(TargetHabbo.GetRpStats().Deaths);
            packet.WriteInteger(TargetHabbo.GetRpStats().Arrested);
            packet.WriteTrueDouble(TargetHabbo.GetRpStats().Deaths > 0 ? Math.Round((double)TargetHabbo.GetRpStats().Knockouts / TargetHabbo.GetRpStats().Deaths, 2) : TargetHabbo.GetRpStats().Knockouts);
            packet.WriteInteger(TargetHabbo.GetRpStats().Attacks);
            packet.WriteInteger(TargetHabbo.GetRpStats().Attacked);
            packet.WriteInteger(TargetHabbo.GetRpStats().DamageDealt);
            packet.WriteInteger(TargetHabbo.GetRpStats().DamageTaken);
            packet.WriteInteger(TargetHabbo.GetRpStats().GetAvailablePoints());
            packet.WriteInteger(TargetHabbo.GetRpStats().Strength);
            packet.WriteInteger(TargetHabbo.GetRpStats().Knowledge);

            var equippedSkills = new List<Skill>();
            foreach (var s in TargetHabbo.GetRpSkills().Skills) {
                if (!s.Equipped) continue;
                if (PlusEnvironment.GetSkillManager().TryGetSkill(s.SkillId, out Skill skill))
                    equippedSkills.Add(skill);
            }

            packet.WriteInteger(equippedSkills.Count);
            foreach (var skill in equippedSkills) {
                packet.WriteString(skill.BadgeCode);
                packet.WriteInteger(skill.Id);
                packet.WriteInteger(TargetHabbo.GetRpSkills().GetLevel(skill.Id));
            }
            var (Level, NextLevelXP, XPRemaining) = LevelManager.GetXPInfo(TargetHabbo.GetRpStats().Experience);
            packet.WriteInteger(Level);
            packet.WriteInteger(TargetHabbo.GetRpStats().Experience);
            packet.WriteInteger(NextLevelXP);
            if (Corporation != null) {
                packet.WriteInteger(Corporation.Id); // job group id
                packet.WriteString(Corporation.Name); // name
                packet.WriteString(Corporation.Badge); // badge
                packet.WriteInteger((int)Corporation.Kind); // type
                var (weeklyShifts, totalShifts) = PlusEnvironment.GetGame().GetShiftManager().GetShiftCounts(Corporation.Id, TargetHabbo.Id);
                packet.WriteInteger(weeklyShifts); // weekly shifts (Mon-Sun, current IRL week)
                packet.WriteInteger(totalShifts); // all shifts (any time)
            } else {
                packet.WriteInteger(0); // no corporation
            }
        }
    }
}