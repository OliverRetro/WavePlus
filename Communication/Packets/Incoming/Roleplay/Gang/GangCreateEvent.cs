using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Roleplay.Gang;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Roleplay.Gang
{
    internal class GangCreateEvent : IPacketEvent
    {
        private const int MinNameLength = 3;
        private const int MaxNameLength = 24;
        private const int MaxStatusLength = 64;

        public void Parse(GameClient session, ClientPacket packet)
        {
            Habbo viewer = session?.GetHabbo();
            if (viewer == null)
                return;

            string name = (packet.PopString() ?? string.Empty).Trim();
            string status = (packet.PopString() ?? string.Empty).Trim();
            int kindValue = packet.PopInt();
            int primary = packet.PopInt();   // colour1: id into the group colorsA palette
            int secondary = packet.PopInt(); // colour2: id into the group colorsB palette

            GroupManager groups = PlusEnvironment.GetGame().GetGroupManager();

            if (groups.TryGetGangForUser(viewer.Id, out _)) {
                session.SendPacket(new RPGangNoticeComposer("You are already in a gang."));
                return;
            }

            if (!GroupManager.IsGangKind((GroupKind)kindValue)) {
                session.SendPacket(new RPGangNoticeComposer("Pick a valid organization type."));
                return;
            }

            GroupKind kind = (GroupKind)kindValue;

            if (name.Length < MinNameLength || name.Length > MaxNameLength) {
                session.SendPacket(new RPGangNoticeComposer($"The name must be {MinNameLength}-{MaxNameLength} characters."));
                return;
            }

            if (status.Length > MaxStatusLength)
                status = status.Substring(0, MaxStatusLength);

            int killsRequired = GangDefinition.KillsRequired(kind);
            int kills = viewer.GetRpStats()?.Knockouts ?? 0;
            if (kills < killsRequired) {
                session.SendPacket(new RPGangNoticeComposer($"You need {killsRequired} kills to start a {GangDefinition.DisplayName(kind)} (you have {kills})."));
                return;
            }

            int cost = GangDefinition.CreationCost(kind);
            if (viewer.Credits < cost) {
                session.SendPacket(new RPGangNoticeComposer($"You need {cost} credits to start a {GangDefinition.DisplayName(kind)}."));
                return;
            }

            if (!groups.TryCreateGang(viewer, name, status, kind, primary, secondary, out Group gang) || gang == null) {
                session.SendPacket(new RPGangNoticeComposer("Something went wrong creating your gang."));
                return;
            }

            viewer.Credits -= cost;
            session.SendPacket(new CreditBalanceComposer(viewer.Credits));

            viewer.GetStats().FavouriteGroupId = gang.Id;

            // Tag the owner's motto with the new gang (unless they're on a work shift).
            GangMottoService.ApplyGangMotto(viewer.Id, gang);

            session.SendPacket(new RPGangNoticeComposer(1, $"{name} is yours. Time to rally the crew.",
                "Gang Created", $"{name} is yours. Time to rally the crew.", primary, secondary));
            session.SendPacket(new RPGangDataComposer(session));
        }
    }
}