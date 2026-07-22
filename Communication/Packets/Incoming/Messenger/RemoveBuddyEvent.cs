using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Messenger
{
    internal class RemoveBuddyEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
                return;

            int amount = packet.PopInt();
            if (amount > 100)
                amount = 100;
            else if (amount < 0)
                return;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                for (int i = 0; i < amount; i++) {
                    int id = packet.PopInt();

                    if (session.GetHabbo().Relationships.Count(x => x.Value.UserId == id) > 0) {
                        int selfId = session.GetHabbo().Id;
                        int targetId = id;
                        // NOTE: preserves legacy precedence: (user_id=self AND target=id) OR (target=self AND user_id=id)
                        db.UserRelationships.Where(r => (r.UserId == selfId && r.Target == targetId) || (r.Target == selfId && r.UserId == targetId)).ExecuteDelete();
                    }

                    if (session.GetHabbo().Relationships.ContainsKey(id))
                        session.GetHabbo().Relationships.Remove(id);

                    GameClient target = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(id);
                    if (target != null) {
                        if (target.GetHabbo().Relationships.ContainsKey(session.GetHabbo().Id))
                            target.GetHabbo().Relationships.Remove(session.GetHabbo().Id);
                    }

                    session.GetHabbo().GetMessenger().DestroyFriendship(id);
                }
            }
        }
    }
}