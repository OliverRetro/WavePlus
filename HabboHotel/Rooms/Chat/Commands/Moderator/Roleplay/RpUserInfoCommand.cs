using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Skill;
using System;
using System.Data;
using System.Text;
using System.Linq;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Roleplay
{
    internal class RpuserRpInfoCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_user_info";

        public string Parameters => "%username%";

        public string Description => "View another users RP information.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("Please enter the username of the user you wish to view.");
                return;
            }

            DataRow userData = null;
            DataRow userRpInfo = null;
            DataTable userSkills = null;
            string username = @params[1];

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`username`,`mail`,`rank`,`motto`,`credits`,`activity_points`,`vip_points`,`gotw_points`,`online`,`rank_vip` FROM users WHERE `username` = @Username LIMIT 1");
                dbClient.AddParameter("Username", username);
                userData = dbClient.GetRow();
            }

            if (userData == null)
            {
                session.SendNotification("Oops, there is no user in the database with that username (" + username + ")!");
                return;
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `user_rp_statistics` WHERE `user_id` = '" + Convert.ToInt32(userData["id"]) + "' LIMIT 1");
                userRpInfo = dbClient.GetRow();
                if (userRpInfo == null)
                {
                    dbClient.RunQuery("INSERT INTO `user_rp_statistics` (`user_id`) VALUES ('" + Convert.ToInt32(userData["id"]) + "')");

                    dbClient.SetQuery("SELECT * FROM `user_rp_statistics` WHERE `user_id` = '" + Convert.ToInt32(userData["id"]) + "' LIMIT 1");
                    userRpInfo = dbClient.GetRow();
                }
            }


            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `user_rp_skills` WHERE `user_id` = @uid");
                dbClient.AddParameter("uid", Convert.ToInt32(userData["id"]));
                userSkills = dbClient.GetTable();
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username);

            StringBuilder habboInfo = new();
            habboInfo.Append("<b>Generic Info:</b>\r");
            if (targetClient != null)
            {
                habboInfo.Append("Health: " + targetClient.GetHabbo().GetRpStats().Health + "\r");
                habboInfo.Append("Shield: " + targetClient.GetHabbo().GetRpStats().Shield + "\r");
                habboInfo.Append("Energy: " + targetClient.GetHabbo().GetRpStats().Energy + "\r");
                habboInfo.Append("Hunger: " + targetClient.GetHabbo().GetRpStats().Hunger + "\r");
                habboInfo.Append("Experience: " + targetClient.GetHabbo().GetRpStats().Experience + " [LVL: <b>" + LevelManager.GetLevelFromXp(targetClient.GetHabbo().GetRpStats().Experience) + "</b>]\r");
                habboInfo.Append("Kills: " + targetClient.GetHabbo().GetRpStats().Knockouts + "\r");
                habboInfo.Append("Deaths: " + targetClient.GetHabbo().GetRpStats().Deaths + "\r\r");

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
                habboInfo.Append("Username: " + Convert.ToString(userData["username"]) + "" + " (ID: <b>" + Convert.ToInt32(userData["id"]) + "</b>\r\r");
                habboInfo.Append("Online Status: " + (targetClient != null ? "True" : "False") + "\r\r");
                habboInfo.Append("Health: " + Convert.ToInt32(userRpInfo["health"]) + "\r");
                habboInfo.Append("Shield: " + Convert.ToInt32(userRpInfo["shield"]) + "\r");
                habboInfo.Append("Energy: " + Convert.ToInt32(userRpInfo["energy"]) + "\r");
                habboInfo.Append("Hunger: " + Convert.ToInt32(userRpInfo["hunger"]) + "\r");
                habboInfo.Append("Experience: " + Convert.ToInt32(userRpInfo["experience"]) + "\r");
                habboInfo.Append("Kills: " + Convert.ToInt32(userRpInfo["knockouts"]) + "\r");
                habboInfo.Append("Deaths: " + Convert.ToInt32(userRpInfo["deaths"]) + "\r\r");
                habboInfo.Append("<b>Skills:</b>\r");

                if (userSkills != null)
                {
                    foreach (DataRow row in userSkills.Rows)
                    {
                        int skillId = Convert.ToInt32(row["skill_id"]);
                        int progress = Convert.ToInt32(row["progress"]);
                        bool equipped = Convert.ToBoolean(row["equipped"]);

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