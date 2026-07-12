using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Skill;
using Plus.HabboHotel.Users;
using System;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

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

            var userData = db.users
                .Where(u => u.username == username)
                .Select(u => new { u.id, u.username })
                .FirstOrDefault();

            if (userData == null) {
                session.SendNotification("Oops, there is no user in the database with that username (" + username + ")!");
                return;
            }

            int userId = userData.id;

            var userRpInfo = db.user_rp_statistics
                .Where(s => s.user_id == userId)
                .Select(s => new { s.health, s.shield, s.energy, s.hunger, s.experience, s.knockouts, s.deaths })
                .FirstOrDefault();

            if (userRpInfo == null) {
                db.Database.ExecuteSqlInterpolated($"INSERT INTO `user_rp_statistics` (`user_id`) VALUES ({userId})");

                userRpInfo = db.user_rp_statistics
                    .Where(s => s.user_id == userId)
                    .Select(s => new { s.health, s.shield, s.energy, s.hunger, s.experience, s.knockouts, s.deaths })
                    .FirstOrDefault();
            }

            var userSkills = db.user_rp_skills
                .Where(s => s.user_id == userId)
                .Select(s => new { s.skill_id, s.progress, s.equipped })
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

                foreach (UserSkillData skillData in targetClient.GetHabbo().GetRpSkills().Skills)
                {
                    Skill skill = PlusEnvironment.GetSkillManager().GetSkillById(skillData.SkillId);

                    if (skill == null)
                        continue;

                    int level = targetClient.GetHabbo().GetRpSkills().GetLevel(skill.Id);
                    bool equipped = targetClient.GetHabbo().GetRpSkills().IsEquipped(skill.Id);

                    habboInfo.Append($"{skill.Name}: Level {level} {(equipped ? "[Equipped]" : "")}\r");
                }
            } else {
                habboInfo.Append("Username: " + userData.username + "" + " (ID: <b>" + userData.id + "</b>\r\r");
                habboInfo.Append("Online Status: " + (targetClient != null ? "True" : "False") + "\r\r");
                habboInfo.Append("Health: " + userRpInfo.health + "\r");
                habboInfo.Append("Shield: " + userRpInfo.shield + "\r");
                habboInfo.Append("Energy: " + userRpInfo.energy + "\r");
                habboInfo.Append("Hunger: " + userRpInfo.hunger + "\r");
                habboInfo.Append("Experience: " + userRpInfo.experience + "\r");
                habboInfo.Append("Kills: " + userRpInfo.knockouts + "\r");
                habboInfo.Append("Deaths: " + userRpInfo.deaths + "\r\r");
                habboInfo.Append("<b>Skills:</b>\r");

                if (userSkills != null) {
                    foreach (var row in userSkills)
                    {
                        int skillId = row.skill_id;
                        int progress = row.progress;
                        bool equipped = row.equipped;

                        Skill skill = PlusEnvironment.GetSkillManager().GetSkillById(skillId);

                        if (skill == null)
                            continue;

                        int level = 0;

                        foreach (SkillLevel lvl in skill.Levels.Values.OrderBy(x => x.Level))
                        {
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