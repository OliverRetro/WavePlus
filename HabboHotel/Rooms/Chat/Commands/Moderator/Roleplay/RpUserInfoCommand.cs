using Plus.Communication.Packets.Outgoing.Quests;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Skill;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Plus.HabboHotel.Roleplay.Level;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Roleplay
{
    internal class RpuserRpInfoCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_user_info";

        public string Parameters => "%username%";

        public string Description => "View another users RP information.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            session.SendPacket(new AnotherStatsComposer(session));
            if (@params.Length == 1) {
                session.SendWhisper("Please enter the username of the user you wish to view.");
                return;
            }

            string username = @params[1];

            using WavePlusContext db = PlusEnvironment.GetDbContext();

            var userData = db.Users
                .Where(u => u.Username == username)
                .Select(u => new { u.Id, u.Username })
                .FirstOrDefault();

            if (userData == null) {
                session.SendNotification("Oops, there is no user in the database with that username (" + username + ")!");
                return;
            }

            int userId = userData.Id;

            var userRpInfo = db.UserRpStatistics
                .Where(s => s.UserId == userId)
                .Select(s => new { s.Health, s.Shield, s.Energy, s.Hunger, s.Experience, s.Knockouts, s.Deaths })
                .FirstOrDefault();

            if (userRpInfo == null) {
                db.Database.ExecuteSqlInterpolated($"INSERT INTO `user_rp_statistics` (`user_id`) VALUES ({userId})");

                userRpInfo = db.UserRpStatistics
                    .Where(s => s.UserId == userId)
                    .Select(s => new { s.Health, s.Shield, s.Energy, s.Hunger, s.Experience, s.Knockouts, s.Deaths })
                    .FirstOrDefault();
            }

            var userSkills = db.UserRpSkills
                .Where(s => s.UserId == userId)
                .Select(s => new { s.SkillId, s.Progress, s.Equipped })
                .ToList();

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username);

            StringBuilder habboInfo = new();
            habboInfo.Append("<b>Generic Info:</b>\r");
            if (targetClient != null) {
                habboInfo.Append("Health: " + targetClient.GetHabbo().GetRpStats().Health + "\r");
                habboInfo.Append("Shield: " + targetClient.GetHabbo().GetRpStats().Shield + "\r");
                habboInfo.Append("Energy: " + targetClient.GetHabbo().GetRpStats().Energy + "\r");
                habboInfo.Append("Hunger: " + targetClient.GetHabbo().GetRpStats().Hunger + "\r");
                var (Level, NextLevelXP, XPRemaining) = LevelManager.GetXPInfo(targetClient.GetHabbo().GetRpStats().Experience);
                habboInfo.Append("Experience: " + targetClient.GetHabbo().GetRpStats().Experience + " [LVL: <b>" + Level + "</b>]\r");
                habboInfo.Append("Kills: " + targetClient.GetHabbo().GetRpStats().Knockouts + "\r");
                habboInfo.Append("Deaths: " + targetClient.GetHabbo().GetRpStats().Deaths + "\r");
                habboInfo.Append("Aggression Timer: " + targetClient.GetHabbo().GetRpStats().Aggression + " seconds\r");
                habboInfo.Append("Bank Balance: " + (targetClient.GetHabbo().GetBankAccount()?.Balance.ToString() ?? "No account") + "\r");
                habboInfo.Append("Equipped Weapon: <b>" + (targetClient.GetHabbo().GetRpStats().ActiveWeapon()?.WeaponData?.Name ?? "Nothing") + "</b>\r\r");

                habboInfo.Append("<b>Skills:</b>\r");

                foreach (UserSkillData skillData in targetClient.GetHabbo().GetRpSkills().Skills) {
                    Skill skill = PlusEnvironment.GetSkillManager().GetSkillById(skillData.SkillId);

                    if (skill == null)
                        continue;

                    int level = targetClient.GetHabbo().GetRpSkills().GetLevel(skill.Id);
                    bool equipped = targetClient.GetHabbo().GetRpSkills().IsEquipped(skill.Id);

                    habboInfo.Append($"{skill.Name}: Level {level} {(equipped ? "[Equipped]" : "")}\r");
                }
            } else {
                habboInfo.Append("Username: " + userData.Username + "" + " (ID: <b>" + userData.Id + "</b>\r\r");
                habboInfo.Append("Online Status: " + (targetClient != null ? "True" : "False") + "\r\r");
                habboInfo.Append("Health: " + userRpInfo.Health + "\r");
                habboInfo.Append("Shield: " + userRpInfo.Shield + "\r");
                habboInfo.Append("Energy: " + userRpInfo.Energy + "\r");
                habboInfo.Append("Hunger: " + userRpInfo.Hunger + "\r");
                habboInfo.Append("Experience: " + userRpInfo.Experience + "\r");
                habboInfo.Append("Kills: " + userRpInfo.Knockouts + "\r");
                habboInfo.Append("Deaths: " + userRpInfo.Deaths + "\r\r");
                habboInfo.Append("<b>Skills:</b>\r");

                if (userSkills != null) {
                    foreach (var row in userSkills) {
                        int skillId = row.SkillId;
                        int progress = row.Progress;
                        bool equipped = row.Equipped;

                        Skill skill = PlusEnvironment.GetSkillManager().GetSkillById(skillId);

                        if (skill == null)
                            continue;

                        int level = 0;

                        foreach (SkillLevel lvl in skill.Levels.Values.OrderBy(x => x.Level)) {
                            if (progress < lvl.RequiredProgress)
                                break;

                            level = lvl.Level;
                        }

                        habboInfo.Append($"{skill.Name}: Level {level} {(equipped ? "[Equipped]" : "")}\r");
                    }
                }
            }

            session.SendNotification(habboInfo.ToString());
        }
    }
}