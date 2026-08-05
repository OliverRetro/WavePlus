using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Skill;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Roleplay;
using System;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class AttributeEditCommand : IChatCommand
    {
        public string PermissionRequired => "command_master_edit_rp";

        public string Parameters => "%user% %attr% %amount%";

        public string Description => "Use a custom bubble to chat with.";

        // Staff repair tool — editing stats (including someone's health) can't be state-gated.
        public bool UsableWhileDead => true;

        public bool UsableWhileCuffed => true;

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length <= 3) {
                session.SendWhisper("Usage of command is :medit <username> <statistic> <amount>. ");
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            if (targetClient == null) {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            string updateVariable = @params[2];
            switch (updateVariable.ToLower()) {
                case "health":
                case "hp": {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_master_edit_rp")) {
                            session.SendWhisper("Oops, you do not have the 'command_update_catalog' permission.", 1);
                            break;
                        }

                        int health = Convert.ToInt32(@params[3]);
                        if (health > 125 || health < 0) {
                            session.SendWhisper("Health must be between 0 and 125.");
                            break;
                        }

                        Habbo target = targetClient.GetHabbo();
                        target.GetRpStats().Health = health;

                        RoomUser targetUser = target.CurrentRoom?.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
                        if (health <= 0 && !target.GetRpStats().IsDead && targetUser != null)
                            UserRpStats.Kill(target, targetUser);
                        else if (health > 0 && target.GetRpStats().IsDead)
                            PlusEnvironment.GetHospitalManager().CancelDeath(target);

                        target.TrySendUserStatsUpdate(true);
                        room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*sets " + target.Username + "'s Health to " + @params[3] + "*", 0, 23, isRpAction: true));
                        break;
                    }

                case "shield": {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_master_edit_rp")) {
                            session.SendWhisper("Oops, you do not have the 'command_master_edit_rp' permission.");
                            break;
                        }

                        if (Convert.ToInt32(@params[3]) > 500 || Convert.ToInt32(@params[3]) < 0) {
                            session.SendWhisper("Shield must be between 0 and 500.");
                            break;
                        }

                        targetClient.GetHabbo().GetRpStats().Shield = +Convert.ToInt32(@params[3]);
                        room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*sets " + targetClient.GetHabbo().Username + "'s Shield to " + @params[3] + "*", 0, 23, isRpAction: true));
                        break;
                    }
                case "energy": {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_master_edit_rp")) {
                            session.SendWhisper("Oops, you do not have the 'command_master_edit_rp' permission.");
                            break;
                        }

                        if (Convert.ToInt32(@params[3]) > 250 || Convert.ToInt32(@params[3]) < 0) {
                            session.SendWhisper("Energy must be between 0 and 250.");
                            break;
                        }

                        targetClient.GetHabbo().GetRpStats().Energy = +Convert.ToInt32(@params[3]);
                        room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*sets " + targetClient.GetHabbo().Username + "'s Energy to " + @params[3] + "*", 0, 23, isRpAction: true));
                        break;
                    }
                case "hunger":
                case "hungry": {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_master_edit_rp")) {
                            session.SendWhisper("Oops, you do not have the 'command_master_edit_rp' permission.");
                            break;
                        }

                        if (Convert.ToInt32(@params[3]) > 100 || Convert.ToInt32(@params[3]) < 0) {
                            session.SendWhisper("Hunger must be between 0 and 100.");
                            break;
                        }

                        targetClient.GetHabbo().GetRpStats().Hunger = +Convert.ToInt32(@params[3]);
                        room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*sets " + targetClient.GetHabbo().Username + "'s Hunger to " + @params[3] + "*", 0, 23, isRpAction: true));
                        break;
                    }
                case "xp":
                case "experience":
                case "level": {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_master_edit_rp")) {
                            session.SendWhisper("Oops, you do not have the 'command_master_edit_rp' permission.");
                            break;
                        }

                        targetClient.GetHabbo().GetRpStats().Experience = Convert.ToInt32(@params[3]);
                        room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*sets " + targetClient.GetHabbo().Username + "'s Experience to " + @params[3] + "*", 0, 23, isRpAction: true));
                        break;
                    }

                case "skill": {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_master_edit_rp")) {
                            session.SendWhisper("Oops, you do not have the 'command_master_edit_rp' permission.");
                            break;
                        }

                        int skillId = Convert.ToInt32(@params[3]);
                        int progressAmount = Convert.ToInt32(@params[4]);

                        if (skillId > 8 || skillId < 1) {
                            session.SendWhisper("Skill must be between 1-8.");
                            break;
                        }

                        if (progressAmount > 10000 || progressAmount < 1) {
                            session.SendWhisper("Skill progress must be between 1-6967.");
                            break;
                        }

                        targetClient.GetHabbo().GetRpSkills().ProgressSkill(skillId, progressAmount);
                        Skill skill = PlusEnvironment.GetSkillManager().GetSkillById(skillId);
                        room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*added " + progressAmount + " " + skill.Name + " skill progress to " + targetClient.GetHabbo().Username + "*", 0, 23, isRpAction: true));
                        break;
                    }
            }
        }
    }
}