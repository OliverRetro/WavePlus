using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Roleplay.Skill;
using Plus.HabboHotel.Users;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

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

            if (roomUser.IsWalking || roomUser.GoalX != roomUser.X || roomUser.GoalY != roomUser.Y)
            {
                session.SendWhisper("You need to stand still to toggle passive mode.", 1);
                return;
            }

            PendingToggles[habbo.Id] = 0;
            session.SendWhisper($"Stand still for {TriggerDelaySeconds} seconds to toggle passive mode.", 1);

            _ = CompleteToggleAsync(habbo.Id, session, room.Id);
        }

        private static async Task CompleteToggleAsync(int userId, GameClient session, int roomId)
        {
            await Task.Delay(TimeSpan.FromSeconds(TriggerDelaySeconds));

            if (!PendingToggles.ContainsKey(userId))
                return;

            PendingToggles.TryRemove(userId, out _);

            Habbo habbo = session?.GetHabbo();
            Room room = habbo?.CurrentRoom;
            if (habbo == null || room == null || room.Id != roomId)
                return;

            RoomUser roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(userId);
            if (roomUser == null || roomUser.IsWalking || roomUser.X != roomUser.GoalX || roomUser.Y != roomUser.GoalY)
                return;

            var stats = habbo.GetRpStats();
            if (stats == null)
                return;

            stats.PassiveMode = !stats.PassiveMode;
            if (stats.PassiveMode)
            {
                roomUser.ApplyEffect(1);
                session.GetHabbo().GetRpWeapons().SetActiveWeaponId(0);
            }

            session.SendWhisper($"Passive mode is now {(stats.PassiveMode ? "enabled" : "disabled") }.", 1);
        }

        public static void CancelPendingToggle(GameClient client)
        {
            if (client == null || client.GetHabbo() == null)
                return;

            if (!PendingToggles.ContainsKey(client.GetHabbo().Id))
                return;

            client.SendWhisper(("Passive mode toggling has been canceled."), 1);
            PendingToggles.TryRemove(client.GetHabbo().Id, out _);
        }
    }
}
