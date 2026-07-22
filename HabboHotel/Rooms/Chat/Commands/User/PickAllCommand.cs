using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class PickAllCommand : IChatCommand
    {
        public string PermissionRequired => "command_pickall";

        public string Parameters => "";

        public string Description => "Picks up all of the furniture from your room.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (!room.CheckRights(session, true))
                return;

            room.GetRoomItemHandler().RemoveItems(session);
            room.GetGameMap().GenerateMaps();

            uint roomId = (uint)room.Id;
            int userId = session.GetHabbo().Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Items.Where(i => i.RoomId == roomId && i.UserId == userId).ExecuteUpdate(s => s.SetProperty(i => i.RoomId, 0u));
            }

            List<Item> items = room.GetRoomItemHandler().GetWallAndFloor.ToList();
            if (items.Count > 0)
                session.SendWhisper("There are still more items in this room, manually remove them or use :ejectall to eject them!");

            session.SendPacket(new FurniListUpdateComposer());
        }
    }
}