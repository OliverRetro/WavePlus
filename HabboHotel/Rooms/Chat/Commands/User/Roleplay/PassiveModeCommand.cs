using Plus.Communication.Packets.Outgoing.Overlay;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Users;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    internal class PassiveModeCommand : IChatCommand
    {
        private static readonly ConcurrentDictionary<int, byte> PendingToggles = new();
        private const int TriggerDelaySeconds = 10;

        public string PermissionRequired => "command_rp_passive_mode";

        public string Parameters => "";

        public string Description => "Toggle passive mode.";
        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session == null || room == null)
                return;

            Habbo habbo = session.GetHabbo();
            var _rpstats = habbo.GetRpStats();
            RoomUser roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (_rpstats == null || roomUser == null)
                return;

            if (session.GetHabbo().GetRpStats().Aggression > 0) {
                session.SendWhisper("You cannot toggle passive mode while you have aggression.", 1);
                return;
            }

            if (roomUser.IsWalking || roomUser.Statusses.ContainsKey("mv")) {
                session.SendWhisper("You need to stand still to toggle passive mode.", 1);
                return;
            }

            PendingToggles[habbo.Id] = 0;
            WebOverlay.SendInteractionTimer(session, "Toggling passive mode in %seconds% seconds", TriggerDelaySeconds);
            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*begins swap between passive mode*", 0, 4, isRpAction: true));

            _ = CompleteToggleAsync(habbo.Id, session, room.Id);
        }

        private static async Task CompleteToggleAsync(int userId, GameClient session, int roomId)
        {
            await Task.Delay(TimeSpan.FromSeconds(TriggerDelaySeconds + RpInteractionTimer.CompletionGraceSeconds));

            if (!PendingToggles.ContainsKey(userId))
                return;

            PendingToggles.TryRemove(userId, out _);

            Habbo habbo = session?.GetHabbo();
            Room room = habbo?.CurrentRoom;
            if (habbo == null || room == null || room.Id != roomId) {
                WebOverlay.CancelInteractionTimer(session);
                return;
            }

            RoomUser roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(userId);
            if (roomUser == null || roomUser.IsWalking || roomUser.X != roomUser.GoalX || roomUser.Y != roomUser.GoalY) {
                // Walked off mid-countdown. CancelPendingToggle only fires on a deliberate move,
                // so this path has to clear the bar itself or it would sit there at 0.
                WebOverlay.CancelInteractionTimer(session);
                return;
            }

            var stats = habbo.GetRpStats();
            if (stats == null) {
                WebOverlay.CancelInteractionTimer(session);
                return;
            }

            stats.PassiveMode = !stats.PassiveMode;
            if (stats.PassiveMode)
                Plus.HabboHotel.Roleplay.RpItem.Weapon.RpWeaponService.Disarm(habbo);

            // Resolve the effect (passive shows 1057, disabling clears it — unless staff/weapon).
            RpEffectService.Refresh(habbo);

            // Passive changes which items are usable, so the open inventory has to re-grey.
            Plus.Communication.Packets.Incoming.WebOverlayCallbackEvent.RefreshInventory(session);

            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, $"*passive mode is now {(stats.PassiveMode ? "enabled" : "disabled")}*", 0, 4, isRpAction: true));
        }

        public static void CancelPendingToggle(GameClient client)
        {
            if (client == null || client.GetHabbo() == null)
                return;

            if (!PendingToggles.ContainsKey(client.GetHabbo().Id))
                return;

            client.SendWhisper(("Passive mode toggling has been canceled."), 1);
            WebOverlay.CancelInteractionTimer(client);
            PendingToggles.TryRemove(client.GetHabbo().Id, out _);
        }
    }
}