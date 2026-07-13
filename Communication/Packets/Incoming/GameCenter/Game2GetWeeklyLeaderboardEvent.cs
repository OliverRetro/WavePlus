using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Games;

namespace Plus.Communication.Packets.Incoming.GameCenter
{
    internal class Game2GetWeeklyLeaderboardEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int gameId = packet.PopInt();

            if (PlusEnvironment.GetGame().GetGameDataManager().TryGetGame(gameId, out GameData _))
            {
                //Code
            }
        }
    }
}