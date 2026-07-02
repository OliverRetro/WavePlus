using System.Linq;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator
{
    internal class UpdateCommand : IChatCommand
    {
        public string PermissionRequired => "command_update";

        public string Parameters => "%variable%";

        public string Description => "Reload a specific part of the hotel.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1) {
                session.SendWhisper("You must inculde a thing to update, e.g. :update catalog");
                return;
            }

            string updateVariable = @params[1];
            switch (updateVariable.ToLower())
            {
                case "cata":
                case "catalog":
                case "catalogue":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_catalog")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_catalog' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetCatalog().Init(PlusEnvironment.GetGame().GetItemManager());
                    PlusEnvironment.GetGame().GetClientManager().SendPacket(new CatalogUpdatedComposer());
                    session.SendWhisper("Catalogue successfully updated.");
                    break;
                }

                case "items":
                case "furni":
                case "furniture":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_furni")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_furni' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetItemManager().Init();
                    session.SendWhisper("Items successfully updated.");
                    break;
                }

                case "models":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_models")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_models' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetRoomManager().LoadModels();
                    session.SendWhisper("Room models successfully updated.");
                    break;
                }

                case "promotions":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_promotions")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_promotions' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetLandingManager().Init();
                    session.SendWhisper("Landing view promotions successfully updated.");
                    break;
                }

                case "youtube":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_youtube")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_youtube' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetTelevisionManager().Init();
                    session.SendWhisper("Youtube televisions playlist successfully updated.");
                    break;
                }

                case "filter":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_filter")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_filter' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetChatManager().GetFilter().Init();
                    session.SendWhisper("Filter definitions successfully updated.");
                    break;
                }

                case "navigator":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_navigator")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_navigator' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetNavigator().Init();
                    session.SendWhisper("Navigator items successfully updated.");
                    break;
                }

                case "ranks":
                case "rights":
                case "permissions":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_rights")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_rights' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetPermissionManager().Init();

                    foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                    {
                        if (client == null || client.GetHabbo() == null || client.GetHabbo().GetPermissions() == null)
                            continue;

                        client.GetHabbo().GetPermissions().Init(client.GetHabbo());
                    }

                    session.SendWhisper("Rank definitions successfully updated.");
                    break;
                }

                case "config":
                case "settings":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_configuration")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_configuration' permission.");
                        break;
                    }

                    PlusEnvironment.GetSettingsManager().Init();
                    session.SendWhisper("Server configuration successfully updated.");
                    break;
                }

                case "bans":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_bans")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_bans' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetModerationManager().ReCacheBans();
                    session.SendWhisper("Ban cache re-loaded.");
                    break;
                }

                case "quests":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_quests")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_quests' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetQuestManager().Init();
                    session.SendWhisper("Quest definitions successfully updated.");
                    break;
                }

                case "achievements":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_achievements")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_achievements' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetAchievementManager().Init();
                    session.SendWhisper("Achievement definitions bans successfully updated.");
                    break;
                }

                case "moderation":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_moderation")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_moderation' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetModerationManager().Init();
                    PlusEnvironment.GetGame().GetClientManager().ModAlert("Moderation presets have been updated. Please reload the client to view the new presets.");

                    session.SendWhisper("Moderation configuration successfully updated.");
                    break;
                }

                case "vouchers":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_vouchers")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_vouchers' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetCatalog().GetVoucherManager().Init();
                    session.SendWhisper("Catalogue vouche cache successfully updated.");
                    break;
                }

                case "gc":
                case "games":
                case "gamecenter":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_game_center")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_game_center' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetGameDataManager().Init();
                    session.SendWhisper("Game Center cache successfully updated.");
                    break;
                }

                case "pet_locale":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_pet_locale")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_pet_locale' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetChatManager().GetPetLocale().Init();
                    session.SendWhisper("Pet locale cache successfully updated.");
                    break;
                }

                case "locale":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_locale")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_locale' permission.");
                        break;
                    }

                    PlusEnvironment.GetLanguageManager().Init();
                    session.SendWhisper("Locale cache successfully updated.");
                    break;
                }

                case "mutant":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_anti_mutant")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_anti_mutant' permission.");
                        break;
                    }

                    PlusEnvironment.GetFigureManager().Init();
                    session.SendWhisper("FigureData manager successfully reloaded.");
                    break;
                }

                case "bots":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_bots")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_bots' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetBotManager().Init();
                    session.SendWhisper("Bot managaer successfully reloaded.");
                    break;
                }

                case "rewards":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_rewards")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_rewards' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetRewardManager().Init();
                    session.SendWhisper("Rewards managaer successfully reloaded.");
                    break;
                }

                case "chat_styles":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_chat_styles")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_chat_styles' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetChatManager().GetChatStyles().Init();
                    session.SendWhisper("Chat Styles successfully reloaded.");
                    break;
                }

                case "badge_definitions":
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand("command_update_badge_definitions")) {
                        session.SendWhisper("Oops, you do not have the 'command_update_badge_definitions' permission.");
                        break;
                    }

                    PlusEnvironment.GetGame().GetBadgeManager().Init();
                    session.SendWhisper("Badge definitions successfully reloaded.");
                    break;
                }

                default:
                    session.SendWhisper("'" + updateVariable + "' is not a valid thing to reload.");
                    break;
            }
        }
    }
}