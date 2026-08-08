using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Links;

namespace Plus.Communication.Packets.Outgoing.Roleplay
{
    internal class ChatLinkConfigComposer : MessageComposer
    {
        private readonly bool _warnBeforeOpen;

        public ChatLinkConfigComposer(GameClient session)
            : base(ServerPacketHeader.ChatLinkConfigMessageComposer)
        {
            _warnBeforeOpen = session?.GetHabbo()?.GetRpSettings()?.LinkWarning ?? true;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteBoolean(_warnBeforeOpen);

            IReadOnlyList<LinkWhitelistManager.Entry> entries =
                PlusEnvironment.GetGame().GetChatManager().GetLinkWhitelist().GetEntries();

            packet.WriteInteger(entries.Count);
            foreach (LinkWhitelistManager.Entry entry in entries) {
                packet.WriteString(entry.Pattern);
                packet.WriteString(entry.MatchType);
                packet.WriteString(entry.Favicon);
            }
        }
    }
}