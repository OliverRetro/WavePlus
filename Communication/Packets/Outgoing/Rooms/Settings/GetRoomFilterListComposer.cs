using System.Collections.Generic;

namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    internal class GetRoomFilterListComposer : MessageComposer
    {
        public List<string> WordFilterList { get; }

        public GetRoomFilterListComposer(List<string> wordFilterList)
            : base(ServerPacketHeader.GetRoomFilterListMessageComposer)
        {
            WordFilterList = wordFilterList;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(WordFilterList.Count);
            foreach (string word in WordFilterList) {
                packet.WriteString(word);
            }
        }
    }
}