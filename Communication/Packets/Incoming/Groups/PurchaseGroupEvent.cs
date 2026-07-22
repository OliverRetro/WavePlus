using System;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Groups
{
    internal class PurchaseGroupEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            string name = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());
            string description = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());

            int roomId = packet.PopInt();
            int mainColour = packet.PopInt();
            int secondaryColour = packet.PopInt();

            int count = packet.PopInt();

            int groupCost = Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("catalog.group.purchase.cost"));

            if (session.GetHabbo().Credits < groupCost) {
                session.SendPacket(new BroadcastMessageAlertComposer("A group costs " + groupCost + " credits! You only have " + session.GetHabbo().Credits + "!"));

                return;
            }

            if (!RoomFactory.TryGetData(roomId, out RoomData room))
                return;

            if (room == null || room.OwnerId != session.GetHabbo().Id || room.Group != null)
                return;

            string badge = string.Empty;

            int baseValue = 1;

            while (baseValue < count) {
                // Safety check
                if (packet == null)
                    return;

                int id = packet.PopInt();
                int color = packet.PopInt();
                int position = packet.PopInt();

                badge += BadgePartUtility.WorkBadgeParts(baseValue == 1, id.ToString(), color.ToString(), position.ToString());

                baseValue += 3;
            }

            if (!PlusEnvironment.GetGame().GetGroupManager().TryCreateGroup(session.GetHabbo(), name, description, roomId, badge, mainColour, secondaryColour, out Group group)) {
                session.SendNotification("An error occured whilst trying to create this group.");
                return;
            }

            // Deduct credits AFTER successful creation
            session.GetHabbo().Credits -= groupCost;

            session.SendPacket(
                new CreditBalanceComposer(session.GetHabbo().Credits));

            session.SendPacket(new PurchaseOkComposer());

            room.Group = group;

            if (session.GetHabbo().CurrentRoomId != room.Id)
                session.SendPacket(new RoomForwardComposer(room.Id));

            session.SendPacket(new NewGroupInfoComposer(roomId, group.Id));
        }
    }
}