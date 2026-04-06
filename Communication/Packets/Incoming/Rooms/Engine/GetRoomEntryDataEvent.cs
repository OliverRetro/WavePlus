using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rooms;
using System;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    internal class GetRoomEntryDataEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Console.WriteLine("getroomentry was called");
            if (session == null || session.GetHabbo() == null)
                return;

            Room room = session.GetHabbo().CurrentRoom;
            if (room == null)
                return;

            int previousRoomId = session.GetHabbo().CurrentRoomId;
            Console.WriteLine($"[GetRoomEntryData] CurrentRoomId before removal: {previousRoomId}, InRoom: {session.GetHabbo().InRoom}");
            if (session.GetHabbo().InRoom)
            {
                if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room oldRoom))
                {
                    Console.WriteLine($"[GetRoomEntryData] Found old room {oldRoom.RoomId}, removing user");
                    if (oldRoom.GetRoomUserManager() != null)
                    {
                        Console.WriteLine("run a milly");
                        oldRoom.GetRoomUserManager().RemoveUserFromRoom(session, false);
                    }
                } else {
                    Console.WriteLine($"[GetRoomEntryData] Could not find old room with ID {session.GetHabbo().CurrentRoomId}");
                }
            } else
            {
                Console.WriteLine("not in a room");
            }
            session.GetHabbo().CurrentRoomId = previousRoomId;
            Console.WriteLine($"[GetRoomEntryData] CurrentRoomId restored to: {session.GetHabbo().CurrentRoomId}");

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

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username);
            if (user != null && session.GetHabbo().PetId == 0)
            {
                room.SendPacket(new UserChangeComposer(user, false));
            }

            session.SendPacket(new RoomEventComposer(room, room.Promotion));

            if (room.GetWired() != null)
                room.GetWired().TriggerEvent(WiredBoxType.TriggerRoomEnter, session.GetHabbo());

            if (PlusEnvironment.GetUnixTimestamp() < session.GetHabbo().FloodTime && session.GetHabbo().FloodTime != 0)
                session.SendPacket(new FloodControlComposer((int) session.GetHabbo().FloodTime - (int) PlusEnvironment.GetUnixTimestamp()));
        }
    }
}