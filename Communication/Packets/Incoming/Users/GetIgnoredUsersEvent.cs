using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Users
{
    internal class GetIgnoredUsersEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            List<string> ignoredUsers = new();

            foreach (int userId in new List<int>(session.GetHabbo().GetIgnores().IgnoredUserIds()))
            {
                Habbo player = PlusEnvironment.GetHabboById(userId);
                if (player != null)
                {
                    if (!ignoredUsers.Contains(player.Username))
                        ignoredUsers.Add(player.Username);
                }
            }

            session.SendPacket(new IgnoredUsersComposer(ignoredUsers));
        }
    }
}