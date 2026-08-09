using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    internal class GetRoomEntryDataEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
                return;

            if (session.GetHabbo().RoomEntryFinalized) {
                session.GetHabbo().RoomEntryFinalized = false;
                return;
            }

            Finalize(session);
        }

        public static void Finalize(GameClient session)
        {

            if (session == null || session.GetHabbo() == null)
                return;

            Room room = session.GetHabbo().CurrentRoom;
            if (room == null)
                return;

            if (!room.GetRoomUserManager().AddAvatarToRoom(session)) {
                room.GetRoomUserManager().RemoveUserFromRoom(session, false);
                return; //TODO: Remove?
            }

            room.SendObjects(session, room);

            session.SendPacket(new RoomEntryInfoComposer(room.RoomId, room.CheckRights(session, true)));
            session.SendPacket(new RoomVisualizationSettingsComposer(room.WallThickness, room.FloorThickness, PlusEnvironment.EnumToBool(room.HideWall.ToString())));

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user != null && session.GetHabbo().PetId == 0) {
                room.SendPacket(new UserChangeComposer(user, false));
            }

            session.SendPacket(new RoomEventComposer(room, room.Promotion));

            session.GetHabbo().GetMessenger()?.OnStatusChanged(true);

            if (session.GetHabbo().GetStats().QuestId > 0)
                PlusEnvironment.GetGame().GetQuestManager().QuestReminder(session, session.GetHabbo().GetStats().QuestId);

            room.GetWired()?.TriggerEvent(WiredBoxType.TriggerRoomEnter, session.GetHabbo());

            PlusEnvironment.GetHospitalManager().HandleRoomEntry(session.GetHabbo());
            PlusEnvironment.GetJailManager().HandleRoomEntry(session.GetHabbo());
            PlusEnvironment.GetPoliceManager().HandleRoomEntry(session.GetHabbo());
            TazerService.TryRechargeAtStation(session.GetHabbo());

            if (session.GetHabbo().GetRpStats()?.IsDead != true)
                session.GetHabbo().ApplyHomeRoomData();

            if (session.GetHabbo().ClickThrough)
                session.SendPacket(new GuideSessionPartnerIsPlayingComposer(true));

            if (PlusEnvironment.GetUnixTimestamp() < session.GetHabbo().FloodTime && session.GetHabbo().FloodTime != 0)
                session.SendPacket(new FloodControlComposer((int)session.GetHabbo().FloodTime - (int)PlusEnvironment.GetUnixTimestamp()));
        }
    }
}