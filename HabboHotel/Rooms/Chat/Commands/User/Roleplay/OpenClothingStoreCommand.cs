using Plus.Communication.Packets.Outgoing.Roleplay.Clothing;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Clothing;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    internal class OpenClothingStoreCommand : IChatCommand
    {
        public string PermissionRequired => "command_clothingstore";
        public string Parameters => "";
        public string Description => "Open clothing store.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            IReadOnlyList<RpClothingCategory> categories =
                PlusEnvironment.GetRpClothingStoreManager().GetCategoriesForRoom(session.GetHabbo().CurrentRoomId);

            session.SendPacket(new ClothingStoreComposer(categories));
        }
    }
}