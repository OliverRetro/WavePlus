using System.Linq;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.Moodlight;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Data.Moodlight;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Moodlight
{
    internal class GetMoodlightConfigEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                return;

            if (!room.CheckRights(session, true))
                return;

            if (room.MoodlightData == null)
            {
                foreach (Item item in room.GetRoomItemHandler().GetWall.ToList())
                {
                    if (item.GetBaseItem().InteractionType == InteractionType.Moodlight)
                        room.MoodlightData = new MoodlightData(item.Id);
                }
            }

            if (room.MoodlightData == null)
                return;

            session.SendPacket(new MoodlightConfigComposer(room.MoodlightData));
        }
    }
}