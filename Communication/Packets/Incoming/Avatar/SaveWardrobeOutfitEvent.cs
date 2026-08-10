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

            look = PlusEnvironment.GetFigureManager().ProcessFigure(look, gender, session.GetHabbo().GetClothing().GetClothingParts, session.GetHabbo().IsVip, false, session.GetHabbo().Look);

            uint uid = (uint)session.GetHabbo().Id;
            uint slot = (uint)slotId;
            string genderUpper = gender.ToUpper();

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            bool exists = db.UserWardrobes.Any(w => w.UserId == uid && w.SlotId == slot);

            if (exists) {
                db.UserWardrobes
                    .Where(w => w.UserId == uid && w.SlotId == slot)
                    .ExecuteUpdate(s => s
                        .SetProperty(x => x.Look, look)
                        .SetProperty(x => x.Gender, genderUpper));
            } else {
                db.UserWardrobes.Add(new UserWardrobeEntity
                {
                    UserId = uid,
                    SlotId = slot,
                    Look = look,
                    Gender = genderUpper
                });
                db.SaveChanges();
            }
        }
    }
}