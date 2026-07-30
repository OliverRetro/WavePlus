using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.Crafting
{
    public static class CraftingContext
    {
        public const string CraftPermission = "craft_item";

        public static ICraftingSource Resolve(GameClient session, int objectId)
        {
            Habbo habbo = session?.GetHabbo();
            Room room = habbo?.CurrentRoom;
            if (habbo == null || room == null)
                return null;

            Item item = room.GetRoomItemHandler().GetItem(objectId);
            if (item?.GetBaseItem() == null)
                return null;

            switch (item.GetBaseItem().InteractionType) {
                case InteractionType.Crafting:
                    return new PersonalCraftingSource(habbo);

                case InteractionType.CraftingCorporation:
                    Group group = room.Group;
                    if (group == null) {
                        session.SendWhisper("This crafting table isn't tied to a corporation.", 1);
                        return null;
                    }

                    if (!group.IsOwnerOrHasPermission(habbo.Id, CraftPermission)) {
                        session.SendWhisper("You don't have permission to craft here.", 1);
                        return null;
                    }

                    return new CorporationCraftingSource(room.Id, group.Id);

                default:
                    return null;
            }
        }
    }
}