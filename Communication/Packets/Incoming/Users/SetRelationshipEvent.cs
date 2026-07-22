using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboHotel.Users.Relationships;

namespace Plus.Communication.Packets.Incoming.Users
{
    internal class SetRelationshipEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
                return;

            int user = packet.PopInt();
            int type = packet.PopInt();

            if (!session.GetHabbo().GetMessenger().FriendshipExists(user)) {
                session.SendPacket(new BroadcastMessageAlertComposer("Oops, you can only set a relationship where a friendship exists."));
                return;
            }

            if (type < 0 || type > 3) {
                session.SendPacket(new BroadcastMessageAlertComposer("Oops, you've chosen an invalid relationship type."));
                return;
            }

            if (session.GetHabbo().Relationships.Count > 2000) {
                session.SendPacket(new BroadcastMessageAlertComposer("Sorry, you're limited to a total of 2000 relationships."));
                return;
            }

            int myId = session.GetHabbo().Id;
            using WavePlusContext db = PlusEnvironment.GetDbContext();

            if (type == 0) {
                db.UserRelationships.Where(r => r.UserId == myId && r.Target == user).ExecuteDelete();

                if (session.GetHabbo().Relationships.ContainsKey(user))
                    session.GetHabbo().Relationships.Remove(user);
            } else {
                int id = db.UserRelationships.Where(r => r.UserId == myId && r.Target == user).Select(r => r.Id).FirstOrDefault();

                if (id > 0) {
                    db.UserRelationships.Where(r => r.UserId == myId && r.Target == user).ExecuteDelete();

                    if (session.GetHabbo().Relationships.ContainsKey(id))
                        session.GetHabbo().Relationships.Remove(id);
                }

                var entity = new Database.EF.Entities.UserRelationshipEntity { UserId = myId, Target = user, Type = type.ToString() };
                db.UserRelationships.Add(entity);
                db.SaveChanges();
                int newId = entity.Id;

                if (!session.GetHabbo().Relationships.ContainsKey(user))
                    session.GetHabbo().Relationships.Add(user, new Relationship(newId, user, type));
            }

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(user);
            if (client != null)
                session.GetHabbo().GetMessenger().UpdateFriend(user, client, true);
            else {
                Habbo habbo = PlusEnvironment.GetHabboById(user);
                if (habbo != null) {
                    if (session.GetHabbo().GetMessenger().TryGetFriend(user, out MessengerBuddy buddy))
                        session.SendPacket(new FriendListUpdateComposer(session.GetHabbo(), buddy));
                }
            }
        }
    }
}