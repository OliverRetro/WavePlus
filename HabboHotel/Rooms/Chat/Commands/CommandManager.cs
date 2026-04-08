using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rooms.Chat.Commands.Administrator;
using Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay;
using Plus.HabboHotel.Rooms.Chat.Commands.Events;
using Plus.HabboHotel.Rooms.Chat.Commands.Moderator;
using Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun;
using Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Roleplay;
using Plus.HabboHotel.Rooms.Chat.Commands.User;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Fun;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plus.HabboHotel.Rooms.Chat.Commands
{
    public class CommandManager
    {
        /// <summary>
        /// Command Prefix only applies to custom commands.
        /// </summary>
        private readonly string _prefix = ":";

        /// <summary>
        /// Commands registered for use.
        /// </summary>
        private readonly Dictionary<string, IChatCommand> _commands;

        /// <summary>
        /// The default initializer for the CommandManager
        /// </summary>
        public CommandManager(string prefix)
        {
            _prefix = prefix;
            _commands = new Dictionary<string, IChatCommand>();

            RegisterVip();
            RegisterUser();
            RegisterEvents();
            RegisterModerator();
            RegisterAdministrator();
        }

        /// <summary>
        /// Request the text to parse and check for commands that need to be executed.
        /// </summary>
        /// <param name="session">Session calling this method.</param>
        /// <param name="message">The message to parse.</param>
        /// <returns>True if parsed or false if not.</returns>
        public bool Parse(GameClient session, string message)
        {
            if (session == null || session.GetHabbo() == null || session.GetHabbo().CurrentRoom == null)
                return false;

            if (!message.StartsWith(_prefix))
                return false;

            if (message == _prefix + "commands")
            {
                StringBuilder list = new();
                list.Append("This is the list of commands you have available:\n");
                foreach (var cmdList in _commands.ToList())
                {
                    if (!string.IsNullOrEmpty(cmdList.Value.PermissionRequired))
                    {
                        if (!session.GetHabbo().GetPermissions().HasCommand(cmdList.Value.PermissionRequired))
                            continue;
                    }

                    list.Append(":" + cmdList.Key + " " + cmdList.Value.Parameters + " - " + cmdList.Value.Description + "\n");
                }

                session.SendPacket(new MotdNotificationComposer(list.ToString()));
                return true;
            }

            message = message.Substring(1);
            string[] split = message.Split(' ');

            if (split.Length == 0)
                return false;

            if (_commands.TryGetValue(split[0].ToLower(), out IChatCommand cmd))
            {
                if (session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                    LogCommand(session.GetHabbo().Id, message, session.GetHabbo().MachineId);

                if (!string.IsNullOrEmpty(cmd.PermissionRequired))
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand(cmd.PermissionRequired))
                        return false;
                }

                session.GetHabbo().ChatCommand = cmd;
                session.GetHabbo().CurrentRoom.GetWired().TriggerEvent(WiredBoxType.TriggerUserSaysCommand, session.GetHabbo(), this);

                cmd.Execute(session, session.GetHabbo().CurrentRoom, split);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Registers the VIP set of commands.
        /// </summary>
        private void RegisterVip()
        {
            Register("spull", new SuperPullCommand());
        }

        /// <summary>
        /// Registers the Events set of commands.
        /// </summary>
        private void RegisterEvents()
        {
            Register("eha", new EventAlertCommand());
            Register("eventalert", new EventAlertCommand());
        }

        /// <summary>
        /// Registers the default set of commands.
        /// </summary>
        private void RegisterUser()
        {
            Register("about", new InfoCommand());
            Register("pickall", new PickAllCommand());
            Register("ejectall", new EjectAllCommand());
            Register("lay", new LayCommand());
            Register("sit", new SitCommand());
            Register("stand", new StandCommand());
            Register("mutepets", new MutePetsCommand());
            Register("mutebots", new MuteBotsCommand());

            Register("mimic", new MimicCommand());
            Register("dance", new DanceCommand());
            Register("push", new PushCommand());
            Register("pull", new PullCommand());
            Register("enable", new EnableCommand());
            Register("follow", new FollowCommand());
            Register("faceless", new FacelessCommand());
            Register("moonwalk", new MoonwalkCommand());

            Register("unload", new UnloadCommand());
            Register("regenmaps", new RegenMaps());
            Register("emptyitems", new EmptyItemsCommand());
            Register("setmax", new SetMaxCommand());
            Register("setspeed", new SetSpeedCommand());
            Register("disablediagonal", new DisableDiagonalCommand());
            Register("flagme", new FlagMeCommand());

            Register("stats", new StatsCommand());
            Register("kickpets", new KickPetsCommand());
            Register("kickbots", new KickBotsCommand());

            Register("room", new RoomCommand());
            Register("dnd", new DndCommand());
            Register("disablegifts", new DisableGiftsCommand());
            Register("convertcredits", new ConvertCreditsCommand());
            Register("disablewhispers", new DisableWhispersCommand());
            Register("disablemimic", new DisableMimicCommand());

            Register("pet", new PetCommand());
            Register("spush", new SuperPushCommand());
            Register("superpush", new SuperPushCommand());
        }

        /// <summary>
        /// Registers the moderator set of commands.
        /// </summary>
        private void RegisterModerator()
        {
            // Roleplay
            Register("rpstats", new RpuserRpInfoCommand());
            Register("roomheal", new RoomHealCommand());
            Register("globalheal", new GlobalHealCommand());

            // rest
            Register("ban", new BanCommand());
            Register("mip", new MipCommand());
            Register("ipban", new IpBanCommand());

            Register("ui", new UserInfoCommand());
            Register("userinfo", new UserInfoCommand());
            Register("sa", new StaffAlertCommand());
            Register("roomunmute", new RoomUnmuteCommand());
            Register("roommute", new RoomMuteCommand());
            Register("roombadge", new RoomBadgeCommand());
            Register("roomalert", new RoomAlertCommand());
            Register("roomkick", new RoomKickCommand());
            Register("mute", new MuteCommand());
            Register("smute", new MuteCommand());
            Register("unmute", new UnmuteCommand());
            Register("massbadge", new MassBadgeCommand());
            Register("kick", new KickCommand());
            Register("skick", new KickCommand());
            Register("ha", new HotelAlertCommand());
            Register("hotelalert", new HotelAlertCommand());
            Register("hal", new HalCommand());
            Register("give", new GiveCommand());
            Register("givebadge", new GiveBadgeCommand());
            Register("dc", new DisconnectCommand());
            Register("kill", new DisconnectCommand());
            Register("Disconnect", new DisconnectCommand());
            Register("alert", new AlertCommand());
            Register("tradeban", new TradeBanCommand());

            Register("teleport", new TeleportCommand());
            Register("summon", new SummonCommand());
            Register("override", new OverrideCommand());
            Register("massenable", new MassEnableCommand());
            Register("massdance", new MassDanceCommand());
            Register("freeze", new FreezeCommand());
            Register("unfreeze", new UnFreezeCommand());
            Register("fastwalk", new FastwalkCommand());
            Register("superfastwalk", new SuperFastwalkCommand());
            Register("coords", new CoordsCommand());
            Register("alleyesonme", new AllEyesOnMeCommand());
            Register("allaroundme", new AllAroundMeCommand());
            Register("forcesit", new ForceSitCommand());

            Register("ignorewhispers", new IgnoreWhispersCommand());
            Register("forced_effects", new DisableForcedFxCommand());

            Register("makesay", new MakeSayCommand());
            Register("flaguser", new FlagUserCommand());
        }

        /// <summary>
        /// Registers the administrator set of commands.
        /// </summary>
        private void RegisterAdministrator()
        {
            // Roleplay
            Register("set", new AttributeEditCommand());

            Register("bubble", new BubbleCommand());
            Register("update", new UpdateCommand());
            Register("deletegroup", new DeleteGroupCommand());
            Register("carry", new CarryCommand());
            Register("goto", new GotoCommand());
        }

        /// <summary>
        /// Registers a Chat Command.
        /// </summary>
        /// <param name="commandText">Text to type for this command.</param>
        /// <param name="command">The command to execute.</param>
        public void Register(string commandText, IChatCommand command)
        {
            _commands.Add(commandText, command);
        }

        public static string MergeParams(string[] @params, int start)
        {
            var merged = new StringBuilder();
            for (int i = start; i < @params.Length; i++)
            {
                if (i > start)
                    merged.Append(" ");
                merged.Append(@params[i]);
            }

            return merged.ToString();
        }

        public void LogCommand(int userId, string data, string machineId)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `logs_client_staff` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                dbClient.AddParameter("UserId", userId);
                dbClient.AddParameter("Data", data);
                dbClient.AddParameter("MachineId", machineId);
                dbClient.AddParameter("Timestamp", PlusEnvironment.GetUnixTimestamp());
                dbClient.RunQuery();
            }
        }

        public bool TryGetCommand(string command, out IChatCommand chatCommand)
        {
            return _commands.TryGetValue(command, out chatCommand);
        }
    }
}