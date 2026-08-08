using System.Collections.Generic;
using System.Text;
using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Links;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class LinkWhitelistCommand : IChatCommand
    {
        public string PermissionRequired => "command_link_whitelist";

        public string Parameters => "%action% %pattern% %type%";

        public string Description => "Manage the clickable chat link whitelist (list/add/remove/reload).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            LinkWhitelistManager manager = PlusEnvironment.GetGame().GetChatManager().GetLinkWhitelist();

            if (@params.Length < 2) {
                session.SendWhisper("Usage: :linkwhitelist list | add <pattern> [domain|wildcard|prefix] [faviconUrl] | remove <pattern> | reload");
                return;
            }

            switch (@params[1].ToLower()) {
                case "list": {
                        IReadOnlyList<LinkWhitelistManager.Entry> entries = manager.GetEntries();
                        if (entries.Count == 0) {
                            session.SendWhisper("The link whitelist is empty.");
                            return;
                        }

                        StringBuilder sb = new();
                        sb.Append("Link whitelist (").Append(entries.Count).Append("):");
                        foreach (LinkWhitelistManager.Entry entry in entries) {
                            sb.Append('\n').Append("• ").Append(entry.Pattern).Append("  [").Append(entry.MatchType).Append(']');
                            if (!string.IsNullOrEmpty(entry.Favicon))
                                sb.Append("  icon: ").Append(entry.Favicon);
                        }

                        session.SendWhisper(sb.ToString());
                        break;
                    }

                case "add": {
                        if (@params.Length < 3) {
                            session.SendWhisper("Usage: :linkwhitelist add <pattern> [domain|wildcard|prefix] [faviconUrl]");
                            return;
                        }

                        string pattern = @params[2];
                        string matchType = @params.Length >= 4 ? @params[3] : "domain";
                        string favicon = @params.Length >= 5 ? @params[4] : null;

                        if (manager.Add(pattern, matchType, favicon)) {
                            RepushAll();
                            session.SendWhisper($"Added '{pattern}' ({matchType}) to the link whitelist.");
                        } else {
                            session.SendWhisper($"'{pattern}' ({matchType}) is already whitelisted.");
                        }
                        break;
                    }

                case "remove": {
                        if (@params.Length < 3) {
                            session.SendWhisper("Usage: :linkwhitelist remove <pattern>");
                            return;
                        }

                        string pattern = @params[2];
                        if (manager.Remove(pattern)) {
                            RepushAll();
                            session.SendWhisper($"Removed '{pattern}' from the link whitelist.");
                        } else {
                            session.SendWhisper($"'{pattern}' was not found in the link whitelist.");
                        }
                        break;
                    }

                case "reload": {
                        manager.Init();
                        RepushAll();
                        session.SendWhisper("Link whitelist reloaded and pushed to all online clients.");
                        break;
                    }

                default:
                    session.SendWhisper("Usage: :linkwhitelist list | add <pattern> [domain|wildcard|prefix] | remove <pattern> | reload");
                    break;
            }
        }

        // update :-)
        private static void RepushAll()
        {
            foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients) {
                if (client?.GetHabbo() == null)
                    continue;

                client.SendPacket(new ChatLinkConfigComposer(client));
            }
        }
    }
}