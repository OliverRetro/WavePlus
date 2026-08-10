using Plus.Communication.Packets.Outgoing.Roleplay.Gang;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Roleplay.Gang
{
    internal class GangSaveColorsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Habbo viewer = session?.GetHabbo();
            if (viewer == null)
                return;

            int primary = packet.PopInt();   // colour1: id into the group colorsA palette
            int secondary = packet.PopInt(); // colour2: id into the group colorsB palette

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGangForUser(viewer.Id, out Group gang) || gang == null)
                return;

            bool canEdit = gang.CreatorId == viewer.Id || gang.HasPermission(viewer.Id, GangDefinition.PermEditGroup);
            if (!canEdit) {
                session.SendPacket(new RPGangNoticeComposer("You don't have permission to edit the gang colors."));
                return;
            }

            gang.Colour1 = primary;
            gang.Colour2 = secondary;
            gang.MarkDirty();

            session.SendPacket(new RPGangNoticeComposer("Gang colors saved."));
            session.SendPacket(new RPGangDataComposer(session));
        }
    }
}