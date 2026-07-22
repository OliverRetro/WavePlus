using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Users.Inventory.Bots;

namespace Plus.Communication.Packets.Outgoing.Inventory.Bots
{
    internal class BotInventoryComposer : MessageComposer
    {
        public ICollection<Bot> Bots { get; }

        public BotInventoryComposer(ICollection<Bot> bots)
            : base(ServerPacketHeader.BotInventoryMessageComposer)
        {
            Bots = bots;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Bots.Count);
            foreach (Bot bot in Bots.ToList()) {
                packet.WriteInteger(bot.Id);
                packet.WriteString(bot.Name);
                packet.WriteString(bot.Motto);
                packet.WriteString(bot.Gender);
                packet.WriteString(bot.Figure);
            }
        }
    }
}