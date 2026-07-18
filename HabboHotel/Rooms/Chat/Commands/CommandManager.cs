using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rooms.Chat.Commands.Administrator;
using Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay;
using Plus.HabboHotel.Rooms.Chat.Commands.Events;
using Plus.HabboHotel.Rooms.Chat.Commands.Moderator;
using Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun;
using Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Roleplay;
using Plus.HabboHotel.Rooms.Chat.Commands.User;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Fun;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Banking;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Police;
using System;
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
        /// Every known command instance, keyed by its class name (e.g. "GotoCommand").
        /// Populated at construction. This is the master list; it does NOT make a command usable
        /// on its own — the actual chat triggers come from the DB (see <see cref="ApplyDatabaseAliases"/>).
        /// </summary>
        private readonly Dictionary<string, IChatCommand> _registry = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The live trigger table: chat alias -> command instance. Built entirely from the DB
        /// (permissions_commands.keys) by <see cref="ApplyDatabaseAliases"/>. A command with no keys
        /// row simply isn't reachable until keys are added in the database.
        /// </summary>
        private readonly Dictionary<string, IChatCommand> _commands = new();

        /// <summary>
        /// The default initializer for the CommandManager
        /// </summary>
        public CommandManager(string prefix)
        {
            _prefix = prefix;

            RegisterRoleplay();
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
                    if (!string.IsNullOrEmpty(cmdList.Value.PermissionRequired) || !string.IsNullOrEmpty(cmdList.Value.GroupPermissionRequired))
                    {
                        if (!CanExecuteCommand(session, cmdList.Key.ToLower(), cmdList.Value))
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

                if (!string.IsNullOrEmpty(cmd.PermissionRequired) || !string.IsNullOrEmpty(cmd.GroupPermissionRequired))
                {
                    if (!CanExecuteCommand(session, split[0].ToLower(), cmd))
                        return false;
                }

                session.GetHabbo().ChatCommand = cmd;
                session.GetHabbo().CurrentRoom.GetWired().TriggerEvent(WiredBoxType.TriggerUserSaysCommand, session.GetHabbo(), this);

                cmd.Execute(session, session.GetHabbo().CurrentRoom, split);
                return true;
            }

            return false;
        }

        private void RegisterRoleplay()
        {
            Register(new LiveFeedTestCommand());
            Register(new StopWorkCommand());
            Register(new StartWorkCommand());
            Register(new RobUserCommand());
            Register(new GangChatCommand());
            Register(new AttributeEditCommand());
            Register(new BankCommand());
            Register(new RpuserRpInfoCommand());
            Register(new RoomHealCommand());
            Register(new GlobalHealCommand());
            Register(new TempRpInventoryCommand());
            Register(new EquipWeaponCommand());
            Register(new ToggleWeaponSkinCommand());
            Register(new UnlockEverythingCommand());
            Register(new AddStockCommand());
            Register(new StockCommand());
            Register(new DemoteCommand());
            Register(new PromoteCommand());
            Register(new FireCommand());
            Register(new PassiveModeCommand());
            Register(new WantedListCommand());
            Register(new CuffCommand());
            Register(new UncuffCommand());
            Register(new EscortCommand());
            Register(new StopEscortCommand());
            Register(new ArrestCommand());
            Register(new ChargeCommand());
            Register(new PardonCommand());
            Register(new HireCommand());
            Register(new SuperHireCommand());
            Register(new GroupPermissionsCommand());
            Register(new HitCommand());
            Register(new LockTargetCommand());
            Register(new UnlockTargetCommand());
            Register(new ToggleSkillCommand());
            Register(new BuildHeightCommand());
            Register(new SuicideCommand());
            Register(new SetTaxiPointCommand());
            Register(new BalanceCommand());
            Register(new DepositCommand());
            Register(new WithdrawCommand());
            Register(new OpenAccountCommand());
        }

        /// <summary>
        /// Registers the VIP set of commands.
        /// </summary>
        private void RegisterVip()
        {
            Register(new SuperPullCommand());
        }

        private static bool CanExecuteCommand(GameClient session, string commandKey, IChatCommand command)
        {
            if (session?.GetHabbo() == null || command == null)
                return false;

            if (!string.IsNullOrEmpty(command.PermissionRequired) && session.GetHabbo().GetPermissions().HasCommand(command.PermissionRequired))
                return true;

            if (string.IsNullOrEmpty(command.GroupPermissionRequired))
                return false;

            Group group = session.GetHabbo().CurrentRoom?.Group;
            return group != null && group.IsOwnerOrHasPermission(session.GetHabbo().Id, command.GroupPermissionRequired);
        }

        /// <summary>
        /// Registers the Events set of commands.
        /// </summary>
        private void RegisterEvents()
        {
            Register(new EventAlertCommand());
        }

        /// <summary>
        /// Registers the default set of commands.
        /// </summary>
        private void RegisterUser()
        {
            Register(new InfoCommand());
            Register(new PickAllCommand());
            Register(new EjectAllCommand());
            Register(new LayCommand());
            Register(new SitCommand());
            Register(new StandCommand());
            Register(new MutePetsCommand());
            Register(new MuteBotsCommand());

            Register(new MimicCommand());
            Register(new DanceCommand());
            Register(new PushCommand());
            Register(new PullCommand());
            Register(new EnableCommand());
            Register(new FollowCommand());
            Register(new FacelessCommand());
            Register(new MoonwalkCommand());

            Register(new UnloadCommand());
            Register(new RegenMaps());
            Register(new EmptyItemsCommand());
            Register(new SetMaxCommand());
            Register(new SetSpeedCommand());
            Register(new DisableDiagonalCommand());
            Register(new ClickThroughCommand());
            Register(new FlagMeCommand());

            Register(new StatsCommand());
            Register(new KickPetsCommand());
            Register(new KickBotsCommand());

            Register(new RoomCommand());
            Register(new DndCommand());
            Register(new DisableGiftsCommand());
            Register(new ConvertCreditsCommand());
            Register(new DisableWhispersCommand());
            Register(new DisableMimicCommand());

            Register(new PetCommand());
            Register(new SuperPushCommand());
            Register(new TaxiCommand());
        }

        /// <summary>
        /// Registers the moderator set of commands.
        /// </summary>
        private void RegisterModerator()
        {
            Register(new BanCommand());
            Register(new MipCommand());
            Register(new IpBanCommand());

            Register(new UserInfoCommand());
            Register(new StaffAlertCommand());
            Register(new RoomUnmuteCommand());
            Register(new RoomMuteCommand());
            Register(new RoomBadgeCommand());
            Register(new RoomAlertCommand());
            Register(new RoomKickCommand());
            Register(new MuteCommand());
            Register(new UnmuteCommand());
            Register(new MassBadgeCommand());
            Register(new KickCommand());
            Register(new HotelAlertCommand());
            Register(new HalCommand());
            Register(new GiveCommand());
            Register(new GiveBadgeCommand());
            Register(new DisconnectCommand());
            Register(new AlertCommand());
            Register(new TradeBanCommand());

            Register(new TeleportCommand());
            Register(new SummonCommand());
            Register(new OverrideCommand());
            Register(new MassEnableCommand());
            Register(new MassDanceCommand());
            Register(new FreezeCommand());
            Register(new UnFreezeCommand());
            Register(new FastwalkCommand());
            Register(new SuperFastwalkCommand());
            Register(new CoordsCommand());
            Register(new AllEyesOnMeCommand());
            Register(new AllAroundMeCommand());
            Register(new ForceSitCommand());

            Register(new IgnoreWhispersCommand());
            Register(new DisableForcedFxCommand());

            Register(new MakeSayCommand());
            Register(new FlagUserCommand());

            Register(new UseItemCommand());
            Register(new InvCommand());
            Register(new DisbandItemCommand());
            Register(new GiveItemCommand());
            Register(new GiveWeaponCommand());
            Register(new RpUpdateCommand());
        }

        /// <summary>
        /// Registers the administrator set of commands.
        /// </summary>
        private void RegisterAdministrator()
        {
            Register(new AddChatBubbleCommand());
            Register(new BubbleCommand());
            Register(new UpdateCommand());
            Register(new DeleteGroupCommand());
            Register(new CarryCommand());
            Register(new GotoCommand());
            Register(new SendToCommand());
        }

        /// <summary>
        /// Adds a command instance to the master registry (keyed by its class name). Chat triggers
        /// for it are supplied separately from the database — see <see cref="ApplyDatabaseAliases"/>.
        /// </summary>
        public void Register(IChatCommand command)
        {
            if (command == null)
                return;

            _registry[command.GetType().Name] = command;
        }

        /// <summary>
        /// Binds a single chat alias to a command instance in the live trigger table.
        /// </summary>
        private void Bind(string alias, IChatCommand command)
        {
            if (string.IsNullOrWhiteSpace(alias) || command == null)
                return;

            // Assignment (not Add) so a duplicate alias can't crash boot; last one wins.
            _commands[alias.Trim().ToLower()] = command;
        }

        /// <summary>
        /// (Re)builds the live trigger table from the database. Each permissions_commands row's
        /// `keys` value (a ';'-separated alias list) is bound to the command it names. A row is
        /// matched to a command by EITHER the command's class name (e.g. "CuffCommand" — required to
        /// disambiguate the several commands that share one permission fuse) OR the command's
        /// permission fuse (e.g. "command_goto" — convenient for the common one-command-per-fuse case).
        ///
        /// Keys are the ONLY way to trigger a command, so this must run at boot after the permission
        /// manager has loaded. A command with no keys row is intentionally unreachable until keys are
        /// added in the DB. Safe to call repeatedly (it clears and rebuilds).
        /// </summary>
        public void ApplyDatabaseAliases()
        {
            _commands.Clear();

            // Fuse -> instance, for rows that reference a command by its (unique) permission fuse.
            var byFuse = new Dictionary<string, IChatCommand>(StringComparer.OrdinalIgnoreCase);
            foreach (IChatCommand cmd in _registry.Values)
            {
                if (!string.IsNullOrEmpty(cmd.PermissionRequired) && !byFuse.ContainsKey(cmd.PermissionRequired))
                    byFuse[cmd.PermissionRequired] = cmd;
            }

            try
            {
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                foreach (var row in db.PermissionsCommands.Select(c => new { c.Command, c.Keys }).ToList())
                {
                    if (string.IsNullOrWhiteSpace(row.Keys) || string.IsNullOrWhiteSpace(row.Command))
                        continue;

                    // Class name wins (unambiguous); fall back to the permission fuse.
                    if (!_registry.TryGetValue(row.Command, out IChatCommand cmd) &&
                        !byFuse.TryGetValue(row.Command, out cmd))
                        continue;

                    foreach (string alias in row.Keys.Split(';'))
                        Bind(alias, cmd);
                }
            }
            catch (Exception e)
            {
                // Most likely the permissions_commands.keys column hasn't been added yet. Without it
                // no commands bind — log loudly so the missing migration is obvious.
                Core.ExceptionLogger.LogException(e);
            }
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
            Core.Persistence.LogBuffer.LogCommand(userId, data, machineId);
        }

        public bool TryGetCommand(string command, out IChatCommand chatCommand)
        {
            return _commands.TryGetValue(command, out chatCommand);
        }
    }
}
