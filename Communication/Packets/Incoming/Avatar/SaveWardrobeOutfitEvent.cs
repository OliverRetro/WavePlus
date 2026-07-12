using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Avatar
{
    internal class SaveWardrobeOutfitEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int slotId = packet.PopInt();
            string look = packet.PopString();
            string gender = packet.PopString();

            look = PlusEnvironment.GetFigureManager().ProcessFigure(look, gender, session.GetHabbo().GetClothing().GetClothingParts, true);

            uint uid = (uint)session.GetHabbo().Id;
            uint slot = (uint)slotId;
            string genderUpper = gender.ToUpper();

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            bool exists = db.user_wardrobes.Any(w => w.user_id == uid && w.slot_id == slot);

            if (exists) {
                db.user_wardrobes
                    .Where(w => w.user_id == uid && w.slot_id == slot)
                    .ExecuteUpdate(s => s
                        .SetProperty(x => x.look, look)
                        .SetProperty(x => x.gender, genderUpper));
            } else {
                db.user_wardrobes.Add(new user_wardrobe
                {
                    user_id = uid,
                    slot_id = slot,
                    look = look,
                    gender = genderUpper
                });
                db.SaveChanges();
            }
        }
    }
}
