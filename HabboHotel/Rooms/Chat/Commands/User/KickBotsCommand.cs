using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Inventory.Bots;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Inventory.Bots;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class KickBotsCommand : IChatCommand
    {
        public string PermissionRequired => "command_kickbots";

        public string Parameters => "";

        public string Description => "Kick all of the bots from the room.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (!room.CheckRights(session, true)) {
                session.SendWhisper("Oops, only the room owner can run this command!");
                return;
            }

            foreach (RoomUser user in room.GetRoomUserManager().GetUserList().ToList()) {
                if (user == null || user.IsPet || !user.IsBot)
                    continue;

                RoomUser botUser = null;
                if (!room.GetRoomUserManager().TryGetBot(user.BotData.Id, out botUser))
                    return;

                uint botId = (uint)user.BotData.Id;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    db.Bots.Where(b => b.Id == botId).ExecuteUpdate(s => s.SetProperty(b => b.RoomId, 0u));
                }

                session.GetHabbo().GetInventoryComponent().TryAddBot(new Bot(Convert.ToInt32(botUser.BotData.Id), Convert.ToInt32(botUser.BotData.OwnerId), botUser.BotData.Name, botUser.BotData.Motto, botUser.BotData.Look, botUser.BotData.Gender));
                session.SendPacket(new BotInventoryComposer(session.GetHabbo().GetInventoryComponent().GetBots()));
                room.GetRoomUserManager().RemoveBot(botUser.VirtualId, false);
            }

            session.SendWhisper("Success, removed all bots.");
        }
    }
}