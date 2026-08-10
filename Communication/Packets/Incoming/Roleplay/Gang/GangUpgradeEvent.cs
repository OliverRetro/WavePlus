using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Roleplay.Gang;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Roleplay.Gang
{
    internal class GangUpgradeEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Habbo viewer = session?.GetHabbo();
            if (viewer == null)
                return;

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGangForUser(viewer.Id, out Group gang) || gang == null)
                return;

            if (gang.CreatorId != viewer.Id) {
                session.SendPacket(new RPGangNoticeComposer("Only the owner can upgrade the gang."));
                return;
            }

            GroupKind? next = GangDefinition.NextKind(gang.Kind);
            if (!next.HasValue) {
                session.SendPacket(new RPGangNoticeComposer("Your gang is already at the highest tier."));
                return;
            }

            GroupKind target = next.Value;
            int killsRequired = GangDefinition.KillsRequired(target);
            int kills = viewer.GetRpStats()?.Knockouts ?? 0;
            if (kills < killsRequired) {
                session.SendPacket(new RPGangNoticeComposer($"You need {killsRequired} kills to upgrade to a {GangDefinition.DisplayName(target)} (you have {kills})."));
                return;
            }

            int cost = GangDefinition.CreationCost(target);
            if (viewer.Credits < cost) {
                session.SendPacket(new RPGangNoticeComposer($"You need {cost} credits to upgrade to a {GangDefinition.DisplayName(target)}."));
                return;
            }

            viewer.Credits -= cost;
            session.SendPacket(new CreditBalanceComposer(viewer.Credits));

            gang.Kind = target;
            gang.MarkDirty();

            // Refresh every member's gang motto tag ([GANG]/[MOB]/[CARTEL]) and their
            // open gang window, so the new tier shows everywhere immediately.
            List<int> memberIds = gang.GetAllMembers;
            foreach (int memberId in memberIds) {
                GangMottoService.ApplyGangMotto(memberId, gang);

                Habbo member = PlusEnvironment.GetHabboById(memberId);
                member?.GetClient()?.SendPacket(new RPGangDataComposer(member.GetClient()));
            }

            LiveFeedService.LiveFeed("<b>" + LiveFeedService.Name(gang.Name, "green") + "</b> upgraded to a <b>" + GangDefinition.DisplayName(target) + "</b>!");
            session.SendPacket(new RPGangNoticeComposer(1, $"{gang.Name} is now a {GangDefinition.DisplayName(target)}.",
                "Gang Upgraded", $"{gang.Name} is now a {GangDefinition.DisplayName(target)}.", gang.Colour1, gang.Colour2));
        }
    }
}