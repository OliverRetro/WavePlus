using Plus.Communication.Packets.Outgoing.Roleplay.Gang;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Roleplay.Gang
{
    internal class GangMemberActionEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Habbo viewer = session?.GetHabbo();
            if (viewer == null)
                return;

            int targetId = packet.PopInt();
            int action = packet.PopInt();

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGangForUser(viewer.Id, out Group gang) || gang == null)
                return;

            // action 3 = leave the gang yourself. The owner can't leave (must delete instead).
            if (action == 3) {
                if (gang.CreatorId == viewer.Id) {
                    session.SendPacket(new RPGangNoticeComposer("As the owner you must delete the gang, not leave it."));
                    return;
                }

                if (!gang.IsMember(viewer.Id)) {
                    session.SendPacket(new RPGangNoticeComposer("You're not a member of this gang."));
                    return;
                }

                // DeleteMember clears the member's gang motto tag (see Group.DeleteMember).
                gang.DeleteMember(viewer.Id);
                LiveFeedService.LiveFeed(LiveFeedService.Name(viewer.Username, "lightblue") + " left <b>" + gang.Name + "</b>");
                session.SendPacket(new RPGangNoticeComposer("You've left " + gang.Name + "."));
                session.SendPacket(new RPGangDataComposer(session));
                return;
            }

            bool isOwner = gang.CreatorId == viewer.Id;
            int permId = action switch
            {
                0 => GangDefinition.PermPromote,
                1 => GangDefinition.PermDemote,
                2 => GangDefinition.PermKick,
                _ => -1
            };

            if (permId < 0)
                return;

            if (!isOwner && !gang.HasPermission(viewer.Id, permId)) {
                session.SendPacket(new RPGangNoticeComposer("You don't have permission to do that."));
                return;
            }

            if (!gang.IsMember(targetId)) {
                session.SendPacket(new RPGangNoticeComposer("That user is no longer a member."));
                return;
            }

            if (targetId == gang.CreatorId) {
                session.SendPacket(new RPGangNoticeComposer("You can't do that to the owner."));
                return;
            }

            if (!GroupHierarchy.CanActOn(session, gang, targetId)) {
                session.SendPacket(new RPGangNoticeComposer("You can't act on someone at your level or above."));
                return;
            }

            Habbo target = PlusEnvironment.GetHabboById(targetId);
            string targetName = target?.Username ?? "That member";

            string message;
            switch (action) {
                case 0: {
                        if (!gang.TryPeekPromotion(targetId, out GroupRole nextRole)) {
                            session.SendPacket(new RPGangNoticeComposer("They're already at the top role."));
                            return;
                        }

                        if (!GroupHierarchy.CanAssignLevel(session, gang, nextRole.Level)) {
                            session.SendPacket(new RPGangNoticeComposer("You can't promote anyone into a role at or above your own."));
                            return;
                        }

                        if (!gang.TryPromoteMember(targetId, out nextRole)) {
                            session.SendPacket(new RPGangNoticeComposer("They're already at the top role."));
                            return;
                        }

                        message = $"{targetName} was promoted to {nextRole.Name}.";
                        LiveFeedService.LiveFeed("<b>" + LiveFeedService.Name(viewer.Username, "green") + "</b> promoted <b>" + LiveFeedService.Name(targetName, "lightblue") + "</b> to <b>" + nextRole.Name + "</b>");
                        break;
                    }
                case 1: {
                        if (!gang.TryDemoteMember(targetId, out GroupRole nextRole)) {
                            session.SendPacket(new RPGangNoticeComposer("They're already at the lowest role."));
                            return;
                        }

                        message = $"{targetName} was demoted to {nextRole.Name}.";
                        LiveFeedService.LiveFeed("<b>" + LiveFeedService.Name(viewer.Username, "green") + "</b> demoted <b>" + LiveFeedService.Name(targetName, "lightblue") + "</b> to <b>" + nextRole.Name + "</b>");
                        break;
                    }
                default: {
                        // DeleteMember clears the member's gang motto tag (see Group.DeleteMember).
                        gang.DeleteMember(targetId);
                        message = $"{targetName} was removed from {gang.Name}.";
                        LiveFeedService.LiveFeed(LiveFeedService.Name(viewer.Username, "green") + " removed " + LiveFeedService.Name(targetName, "lightblue") + " from <b>" + gang.Name + "</b>");
                        target?.GetClient()?.SendWhisper("You've been removed from '" + gang.Name + "'.", 1);
                        break;
                    }
            }

            session.SendPacket(new RPGangNoticeComposer(message));
            session.SendPacket(new RPGangDataComposer(session));

            // Refresh the target's gang window too, if they have it open.
            target?.GetClient()?.SendPacket(new RPGangDataComposer(target.GetClient()));
        }
    }
}