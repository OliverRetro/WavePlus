using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Stock;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal class OfferCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_offerx";

        public string Parameters => "%username%";

        public string GroupPermissionRequired => "offer_item";

        public string Description => "Offer this room's stock to a user for sale.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            Habbo habbo = session?.GetHabbo();
            if (habbo == null || room == null)
                return;

            if (@params.Length < 2) {
                session.SendWhisper("Please enter the username of the person you want to make an offer to.", 1);
                return;
            }

            Group group = room.Group;
            if (group == null) {
                session.SendWhisper("This room doesn't belong to a corporation.", 1);
                return;
            }

            if (!group.IsMember(habbo.Id)) {
                session.SendWhisper("You don't work here.", 1);
                return;
            }

            RoomUser targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(@params[1]);
            if (targetUser == null || targetUser.IsBot || targetUser.GetClient()?.GetHabbo() == null) {
                session.SendWhisper("That user isn't in this room.", 1);
                return;
            }

            Habbo target = targetUser.GetClient().GetHabbo();
            if (target.Id == habbo.Id) {
                session.SendWhisper("You can't make an offer to yourself.", 1);
                return;
            }

            if (!RpOfferService.Open(targetUser.GetClient(), room.Id, RoomStockManager.RoomScope, habbo, group.Name)) {
                session.SendWhisper("This room has nothing in stock. Add some with :addroomstock.", 1);
                return;
            }

            session.SendWhisper($"Showed {target.Username} what {group.Name} has in stock.", 1);

            RoomUser self = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (self != null)
                room.SendPacket(new ShoutComposer(self.VirtualId, $"*offers {target.Username} a look at the stock*", 0, 4, isRpAction: true));
        }
    }
}