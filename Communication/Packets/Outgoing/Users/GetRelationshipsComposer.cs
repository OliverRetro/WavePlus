using System.Linq;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Relationships;

namespace Plus.Communication.Packets.Outgoing.Users
{
    internal class GetRelationshipsComposer : MessageComposer
    {
        public Habbo Habbo { get; }

        public GetRelationshipsComposer(Habbo habbo)
            : base(ServerPacketHeader.GetRelationshipsMessageComposer)
        {
            Habbo = habbo;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Habbo.Id);
            packet.WriteInteger(Habbo.Relationships.Count); // Count
            foreach (Relationship relationship in Habbo.Relationships.Values)
            {
                UserCache relation = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(relationship.UserId);
                if (relation == null)
                {
                    packet.WriteInteger(0);
                    packet.WriteInteger(0);
                    packet.WriteInteger(0); // Their ID
                    packet.WriteString("Placeholder");
                    packet.WriteString("hr-115-42.hd-190-1.ch-215-62.lg-285-91.sh-290-62");
                }
                else
                {
                    packet.WriteInteger(relationship.Type);
                    packet.WriteInteger(Habbo.Relationships.Count(x => x.Value.Type == relationship.Type));
                    packet.WriteInteger(relationship.UserId); // Their ID
                    packet.WriteString(relation.Username);
                    packet.WriteString(relation.Look);
                }
            }
        }
    }
}