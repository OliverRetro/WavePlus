using System;
using System.Drawing;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Inventory.Bots;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users.Inventory.Bots;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Bots
{
    internal class PickUpBotEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            int botId = packet.PopInt();
            if (botId == 0)
                return;

            Room room = session.GetHabbo().CurrentRoom;
            if (room == null)
                return;

            if (!room.GetRoomUserManager().TryGetBot(botId, out RoomUser botUser))
                return;

            if (session.GetHabbo().Id != botUser.BotData.OwnerId && !session.GetHabbo().GetPermissions().HasRight("bot_place_any_override")) {
                session.SendWhisper("You can only pick up your own bots!");
                return;
            }

            uint bId = (uint)botId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Bots.Where(b => b.Id == bId).ExecuteUpdate(s => s.SetProperty(b => b.RoomId, 0u));
            }

            room.GetGameMap().RemoveUserFromMap(botUser, new Point(botUser.X, botUser.Y));

            session.GetHabbo().GetInventoryComponent().TryAddBot(new Bot(Convert.ToInt32(botUser.BotData.Id), Convert.ToInt32(botUser.BotData.OwnerId), botUser.BotData.Name, botUser.BotData.Motto, botUser.BotData.Look, botUser.BotData.Gender));
            session.SendPacket(new BotInventoryComposer(session.GetHabbo().GetInventoryComponent().GetBots()));
            room.GetRoomUserManager().RemoveBot(botUser.VirtualId, false);
        }
    }
}