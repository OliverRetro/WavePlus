using System.Collections.Generic;
using Plus.HabboHotel.Games;

namespace Plus.Communication.Packets.Outgoing.GameCenter
{
    internal class GameListComposer : MessageComposer
    {
        public ICollection<GameData> Games { get; }

        public GameListComposer(ICollection<GameData> games)
            : base(ServerPacketHeader.GameListMessageComposer)
        {
            Games = games;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(PlusEnvironment.GetGame().GetGameDataManager().GetCount()); //Game count
            foreach (GameData game in Games) {
                packet.WriteInteger(game.Id);
                packet.WriteString(game.Name);
                packet.WriteString(game.ColourOne);
                packet.WriteString(game.ColourTwo);
                packet.WriteString(game.ResourcePath);
                packet.WriteString(game.StringThree);
            }
        }
    }
}