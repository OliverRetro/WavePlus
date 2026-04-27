using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Connection
{
    internal class GoToFlatEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            // GoToFlat is sent by the client after it has finished rendering the room model.
            // The actual room entry (AddAvatarToRoom + SendObjects) happens in GetRoomEntryDataEvent.
            // Calling EnterRoom() here was sending a second RoomReadyComposer on every entry,
            // which caused the client's loading state machine to reset and never complete on re-entry.
        }
    }
}