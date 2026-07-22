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
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var wardrobeData = db.UserWardrobes
                    .Where(w => w.UserId == uid)
                    .Select(w => new { w.SlotId, w.Look, w.Gender })
                    .ToList();

                packet.WriteInteger(wardrobeData.Count);
                foreach (var row in wardrobeData) {
                    packet.WriteInteger((int)row.SlotId);
                    packet.WriteString(row.Look);
                    packet.WriteString(row.Gender.ToUpper());
                }
            }
        }
    }
}