using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using System;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class AttributeEditCommand : IChatCommand
    {
        public string PermissionRequired => "command_master_edit_rp";

        public string Parameters => "%user% %attr% %amount%";

        public string Description => "Use a custom bubble to chat with.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length <= 3)
            {
                session.SendWhisper("Usage of command is :medit <username> <statistic> <amount>. ");
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            string updateVariable = @params[2];
            switch (updateVariable.ToLower())
            {
                case "health":
                case "hp":
                    {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_master_edit_rp"))
                        {
                            session.SendWhisper("Oops, you do not have the 'command_update_catalog' permission.", 1);
                            break;
                        }

                        if (Convert.ToInt32(@params[3]) > 125 || Convert.ToInt32(@params[3]) < 0)
                        {
                            session.SendWhisper("Health must be between 0 and 125.");
                            break;
                        }

                        targetClient.GetHabbo().GetRpStats().Health = + Convert.ToInt32(@params[3]);
                        session.SendPacket(new ShoutComposer(session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*Sets " + targetClient.GetHabbo().Username + "\'s Health to " + @params[3] + "*", 0, 23));
                        break;
                    }

                case "shield":
                    {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_master_edit_rp"))
                        {
                            session.SendWhisper("Oops, you do not have the 'command_master_edit_rp' permission.");
                            break;
                        }

                        if (Convert.ToInt32(@params[3]) > 500 || Convert.ToInt32(@params[3]) < 0)
                        {
                            session.SendWhisper("Shield must be between 0 and 500.");
                            break;
                        }

                        targetClient.GetHabbo().GetRpStats().Shield = + Convert.ToInt32(@params[3]);
                        session.SendPacket(new ShoutComposer(session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*Sets " + targetClient.GetHabbo().Username + "\'s Shield to " + @params[3] + "*", 0, 23));
                        break;
                    }
                case "energy":
                    {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_master_edit_rp"))
                        {
                            session.SendWhisper("Oops, you do not have the 'command_master_edit_rp' permission.");
                            break;
                        }

                        if (Convert.ToInt32(@params[3]) > 250 || Convert.ToInt32(@params[3]) < 0)
                        {
                            session.SendWhisper("Energy must be between 0 and 250.");
                            break;
                        }

                        targetClient.GetHabbo().GetRpStats().Energy = + Convert.ToInt32(@params[3]);
                        session.SendPacket(new ShoutComposer(session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*Sets " + targetClient.GetHabbo().Username + "\'s Energy to " + @params[3] + "*", 0, 23));
                        break;
                    }
                case "hunger":
                case "hungry":
                    {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_master_edit_rp"))
                        {
                            session.SendWhisper("Oops, you do not have the 'command_master_edit_rp' permission.");
                            break;
                        }
                        
                        if(Convert.ToInt32(@params[3]) > 100 || Convert.ToInt32(@params[3]) < 0)
                        {
                            session.SendWhisper("Hunger must be between 0 and 100.");
                            break;
                        }

                        targetClient.GetHabbo().GetRpStats().Hunger = + Convert.ToInt32(@params[3]);
                        session.SendPacket(new ShoutComposer(session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*Sets " + targetClient.GetHabbo().Username + "\'s Hunger to " + @params[3] + "*", 0, 23));
                        break;
                    }
                case "xp":
                case "experience":
                case "level":
                    {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_master_edit_rp"))
                        {
                            session.SendWhisper("Oops, you do not have the 'command_master_edit_rp' permission.");
                            break;
                        }

                        targetClient.GetHabbo().GetRpStats().Experience = + Convert.ToInt32(@params[3]);
                        session.SendPacket(new ShoutComposer(session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*Sets " + targetClient.GetHabbo().Username + "\'s Experience to " + @params[3] + "*", 0, 23));
                        break;
                    }
            }
        }
    }
}