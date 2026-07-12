using System.Linq;
using Plus.Database.EF;

namespace Plus.Communication.Packets.Outgoing.Avatar
{
    internal class WardrobeComposer : MessageComposer
    {
        public int UserId { get; }

        public WardrobeComposer(int userId)
            : base(ServerPacketHeader.WardrobeMessageComposer)
        {
            UserId = userId;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(1);
            uint uid = (uint)UserId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var wardrobeData = db.user_wardrobes
                    .Where(w => w.user_id == uid)
                    .Select(w => new { w.slot_id, w.look, w.gender })
                    .ToList();

                packet.WriteInteger(wardrobeData.Count);
                foreach (var row in wardrobeData)
                {
                    packet.WriteInteger((int)row.slot_id);
                    packet.WriteString(row.look);
                    packet.WriteString(row.gender.ToUpper());
                }
            }
        }
    }
}