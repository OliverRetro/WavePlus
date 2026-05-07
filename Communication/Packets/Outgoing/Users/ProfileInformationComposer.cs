using System;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Users
{
    internal class ProfileInformationComposer : MessageComposer
    {
        public Habbo TargetHabbo { get; }
        public List<Group> Groups { get; }
        public int FriendCount { get; }
        public Habbo MyHabbo { get; }

        public ProfileInformationComposer(Habbo habbo, GameClient session, List<Group> groups, int friendCount)
            : base(ServerPacketHeader.ProfileInformationMessageComposer)
        {
            TargetHabbo = habbo;
            Groups = groups;
            FriendCount = friendCount;
            MyHabbo = session.GetHabbo();
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
            packet.WriteInteger(FriendCount); // Friend Count
            packet.WriteBoolean(TargetHabbo.Id != MyHabbo.Id && MyHabbo.GetMessenger().FriendshipExists(TargetHabbo.Id)); //  Is friend
            packet.WriteBoolean(TargetHabbo.Id != MyHabbo.Id && !MyHabbo.GetMessenger().FriendshipExists(TargetHabbo.Id) && MyHabbo.GetMessenger().RequestExists(TargetHabbo.Id)); // Sent friend request
            packet.WriteBoolean((PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(TargetHabbo.Id)) != null);

            packet.WriteInteger(Groups.Count);
            foreach (Group group in Groups)
            {
                packet.WriteInteger(group.Id);
                packet.WriteString(group.Name);
                packet.WriteString(group.Badge);
                packet.WriteString(PlusEnvironment.GetGame().GetGroupManager().GetColourCode(group.Colour1, true));
                packet.WriteString(PlusEnvironment.GetGame().GetGroupManager().GetColourCode(group.Colour2, false));
                packet.WriteBoolean(TargetHabbo.GetStats().FavouriteGroupId == group.Id); // todo favs
                packet.WriteInteger(group.CreatorId); // or OwnerId
                packet.WriteBoolean(group.CreatorId == TargetHabbo.Id);
            }

            packet.WriteInteger(Convert.ToInt32(PlusEnvironment.GetUnixTimestamp() - TargetHabbo.LastOnline)); // Last online
            packet.WriteBoolean(true); // Show the profile
            packet.WriteBoolean(true); // on duty
            packet.WriteString("Pigs"); // Job name
            packet.WriteString("Fuck da police"); // job description
            packet.WriteString("N.W.A."); // gang name
            packet.WriteInteger(TargetHabbo.GetRpStats().Knockouts);
            packet.WriteInteger(TargetHabbo.GetRpStats().Deaths);
            packet.WriteInteger(TargetHabbo.GetRpStats().Arrested);
            packet.WriteTrueDouble(TargetHabbo.GetRpStats().Deaths > 0 ? Math.Round((double)TargetHabbo.GetRpStats().Knockouts / TargetHabbo.GetRpStats().Deaths, 2) : TargetHabbo.GetRpStats().Knockouts);
            packet.WriteInteger(TargetHabbo.GetRpStats().Attacks);
            packet.WriteInteger(TargetHabbo.GetRpStats().Attacked);
            packet.WriteInteger(TargetHabbo.GetRpStats().DamageDealt);
            packet.WriteInteger(TargetHabbo.GetRpStats().DamageTaken);
        }
    }
}