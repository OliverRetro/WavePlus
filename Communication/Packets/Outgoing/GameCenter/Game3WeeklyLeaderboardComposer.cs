using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Games;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.GameCenter
{
    public class Game3WeeklyLeaderboardComposer : MessageComposer
    {
        public GameData GameData { get; }
        public ICollection<Habbo> Habbos { get; }

        public Game3WeeklyLeaderboardComposer(GameData gameData, ICollection<Habbo> habbos)
            : base(ServerPacketHeader.Game3WeeklyLeaderboardMessageComposer)
        {
            GameData = gameData;
            Habbos = habbos;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(2014);
            packet.WriteInteger(41);
            packet.WriteInteger(0);
            packet.WriteInteger(1);
            packet.WriteInteger(1581);

            //Used to generate the ranking numbers.
            int num = 0;

            packet.WriteInteger(Habbos.Count); //Count
            foreach (Habbo habbo in Habbos.ToList()) {
                num++;
                packet.WriteInteger(habbo.Id); //Id
                packet.WriteInteger(habbo.FastFoodScore); //Score
                packet.WriteInteger(num); //Rank
                packet.WriteString(habbo.Username); //Username
                packet.WriteString(habbo.Look); //Figure
                packet.WriteString(habbo.Gender.ToLower()); //Gender .ToLower()
            }

            packet.WriteInteger(0); //
            packet.WriteInteger(GameData.Id); //Game Id?
        }
    }
}