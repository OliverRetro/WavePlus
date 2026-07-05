using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Police;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Ambulance
{
    internal class MedicHealCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_hit";
        public string Parameters => "%username%";
        public string Description => "Heal or revive an adjacent user (EMS only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            if (@params.Length < 2) {
                session.SendWhisper("Usage: :heal <username>", 1);
                return;
            }

            if (!PoliceManager.HasJobPermission(session.GetHabbo(), "rp.ambulance.corporation.id", "heal_user")) {
                session.SendWhisper("Only on-duty ambulance staff can heal users.", 1);
                return;
            }

            if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                return;

            if (!RpCommandUtil.RequireAdjacent(session, room, targetClient.GetHabbo(), "You need to be beside the patient to heal them."))
                return;

            UserRpStats medicStats = session.GetHabbo().GetRpStats();
            int energyCost = Math.Max(0, GetSettingInt("rp.ambulance.heal.energy.cost", 2));
            if (medicStats != null && medicStats.Energy < energyCost) {
                session.SendWhisper("You don't have enough energy to perform treatment.", 1);
                return;
            }

            Habbo patient = targetClient.GetHabbo();
            UserRpStats stats = patient.GetRpStats();
            if (stats == null)
                return;

            if (medicStats != null) {
                medicStats.Energy = Math.Max(0, medicStats.Energy - energyCost);
                session.GetHabbo().TrySendUserStatsUpdate(true);
            }

            if (stats.IsDead) {
                PlusEnvironment.GetHospitalManager().CancelDeath(patient);
                int reviveHealth = Math.Clamp(GetSettingInt("rp.ambulance.revive.health", 35), 1, UserRpStats.GetMaxHealth(patient));
                stats.Health = reviveHealth;
                stats.IsDead = false;
                patient.SaveRpStats();
                patient.TrySendUserStatsUpdate(true);

                targetClient.SendWhisper("You were revived by " + session.GetHabbo().Username + ".", 1);
                session.SendWhisper("You revived " + patient.Username + ".", 1);
                RpCommandUtil.RoomAction(room, session.GetHabbo(), "*" + session.GetHabbo().Username + " revives " + patient.Username + "*");
                return;
            }

            int amount = Math.Max(1, GetSettingInt("rp.ambulance.heal.amount", 25));
            int oldHealth = stats.Health;
            stats.Health = Math.Min(UserRpStats.GetMaxHealth(patient), stats.Health + amount);
            patient.SaveRpStats();
            patient.TrySendUserStatsUpdate(true);

            int healed = stats.Health - oldHealth;
            if (healed <= 0) {
                session.SendWhisper(patient.Username + " is already at full health.", 1);
                return;
            }

            targetClient.SendWhisper("You were healed for " + healed + " HP by " + session.GetHabbo().Username + ".", 1);
            session.SendWhisper("You healed " + patient.Username + " for " + healed + " HP.", 1);
            RpCommandUtil.RoomAction(room, session.GetHabbo(), "*" + session.GetHabbo().Username + " treats " + patient.Username + "'s injuries*");
        }

        private static int GetSettingInt(string key, int fallback)
        {
            return int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue(key), out int parsed) ? parsed : fallback;
        }
    }
}
