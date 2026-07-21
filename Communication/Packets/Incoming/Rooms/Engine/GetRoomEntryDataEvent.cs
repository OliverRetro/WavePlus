using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    internal class GetRoomEntryDataEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
                return;

            Room room = session.GetHabbo().CurrentRoom;
            if (room == null)
                return;

            if (!room.GetRoomUserManager().AddAvatarToRoom(session))
            {
                room.GetRoomUserManager().RemoveUserFromRoom(session, false);
                return; //TODO: Remove?
            }

            room.SendObjects(session, room);

            if (session.GetHabbo().GetMessenger() != null)
                session.GetHabbo().GetMessenger().OnStatusChanged(true);

            if (session.GetHabbo().GetStats().QuestId > 0)
                PlusEnvironment.GetGame().GetQuestManager().QuestReminder(session, session.GetHabbo().GetStats().QuestId);

            session.SendPacket(new RoomEntryInfoComposer(room.RoomId, room.CheckRights(session, true)));
            session.SendPacket(new RoomVisualizationSettingsComposer(room.WallThickness, room.FloorThickness, PlusEnvironment.EnumToBool(room.HideWall.ToString())));

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user != null && session.GetHabbo().PetId == 0)
            {
                room.SendPacket(new UserChangeComposer(user, false));
            }

            session.SendPacket(new RoomEventComposer(room, room.Promotion));

            if (room.GetWired() != null)
                room.GetWired().TriggerEvent(WiredBoxType.TriggerRoomEnter, session.GetHabbo());

            PlusEnvironment.GetHospitalManager().HandleRoomEntry(session.GetHabbo());
            PlusEnvironment.GetJailManager().HandleRoomEntry(session.GetHabbo());
            PlusEnvironment.GetPoliceManager().HandleRoomEntry(session.GetHabbo());
            Plus.HabboHotel.Roleplay.RpItem.Weapon.TazerService.TryRechargeAtStation(session.GetHabbo());

            if (session.GetHabbo().GetRpStats()?.IsDead != true)
                session.GetHabbo().ApplyHomeRoomData();

            // Re-assert the user's persisted click-through preference on every room entry —
            // the client resets it per room, so a login/goto would otherwise drop the setting.
            if (session.GetHabbo().ClickThrough)
                session.SendPacket(new GuideSessionPartnerIsPlayingComposer(true));

            PlusEnvironment.GetGame().GetBotManager().TryHandleTaxiArrival(session);

            if (PlusEnvironment.GetUnixTimestamp() < session.GetHabbo().FloodTime && session.GetHabbo().FloodTime != 0)
                session.SendPacket(new FloodControlComposer((int)session.GetHabbo().FloodTime - (int)PlusEnvironment.GetUnixTimestamp()));
        }
    }
}
