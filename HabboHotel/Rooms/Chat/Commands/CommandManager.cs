using Plus.Communication.Packets.Outgoing.Overlay;
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
using Plus.HabboHotel.Users;
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
        /// Class name -> category, filled in by <see cref="Register"/> from whichever Register*
        /// block is running. The registration blocks already partition the commands exactly the
        /// way the overlay wants to show them, so the category is derived rather than stored in
        /// the database — a command therefore cannot be added without getting a category, and
        /// the two can never drift apart.
        /// </summary>
        private readonly Dictionary<string, string> _categoryByCommand = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Category assigned to anything registered while it is set. See the constructor.</summary>
        private string _registeringCategory = CommandCategory.General;

        /// <summary>
        /// The default initializer for the CommandManager
        /// </summary>
        public CommandManager(string prefix)
        {
            _prefix = prefix;

            _registeringCategory = CommandCategory.Roleplay;
            RegisterRoleplay();

            _registeringCategory = CommandCategory.Vip;
            RegisterVip();

            // General commands are available to everyone and so only ever appear under "All".
            _registeringCategory = CommandCategory.General;
            RegisterUser();
            RegisterEvents();

            _registeringCategory = CommandCategory.Staff;
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

            if (message == _prefix + "commands") {
                SendCommandList(session);
                return true;
            }

            message = message.Substring(1);
            string[] split = message.Split(' ');

            if (split.Length == 0)
                return false;

            if (_commands.TryGetValue(split[0].ToLower(), out IChatCommand cmd)) {
                if (session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                    LogCommand(session.GetHabbo().Id, message, session.GetHabbo().MachineId);

                if (!string.IsNullOrEmpty(cmd.PermissionRequired) || !string.IsNullOrEmpty(cmd.GroupPermissionRequired)) {
                    if (!CanExecuteCommand(session, split[0].ToLower(), cmd))
                        return false;
                }

                if (IsBlockedByState(session, cmd))
                    return false;

                session.GetHabbo().ChatCommand = cmd;
                session.GetHabbo().CurrentRoom.GetWired().TriggerEvent(WiredBoxType.TriggerUserSaysCommand, session.GetHabbo(), this);

                cmd.Execute(session, session.GetHabbo().CurrentRoom, split);
                return true;
            }

            return false;
        }

        private void RegisterRoleplay()
        {
            Register(new MassChatCommand());
            Register(new GodModeCommand());
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
            Register(new AddRoomStockCommand());
            Register(new AddItemStockCommand());
            Register(new StockCommand());
            Register(new OfferCommand());
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
            Register(new SuperPromoteCommand());
            Register(new SuperDemoteCommand());
            Register(new SuperFireCommand());
            Register(new GInviteCommand());
            Register(new GroupPermissionsCommand());
            Register(new HitCommand());
            Register(new LockTargetCommand());
            Register(new UnlockTargetCommand());
            Register(new PingCommand());
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

        /// <summary>
        /// Pushes the categorised command list to the gaz-js overlay. Only commands the user can
        /// actually run are included, and a category is only offered when they qualify for it.
        /// </summary>
        public void SendCommandList(GameClient session)
        {
            Habbo habbo = session?.GetHabbo();
            if (habbo == null)
                return;

            // Aliases point at shared singletons, so grouping by instance collapses every trigger
            // for a command into one entry instead of repeating it once per key.
            Dictionary<IChatCommand, List<string>> aliases = new();
            foreach (KeyValuePair<string, IChatCommand> pair in _commands) {
                if (!aliases.TryGetValue(pair.Value, out List<string> keys))
                    aliases[pair.Value] = keys = new List<string>();

                keys.Add(pair.Key);
            }

            List<(string Name, object Payload)> commands = new();
            foreach (KeyValuePair<IChatCommand, List<string>> pair in aliases) {
                IChatCommand command = pair.Key;

                if (!_categoryByCommand.TryGetValue(command.GetType().Name, out string category))
                    category = CommandCategory.General;

                if (!CanSeeCategory(habbo, category))
                    continue;

                if ((!string.IsNullOrEmpty(command.PermissionRequired) || !string.IsNullOrEmpty(command.GroupPermissionRequired))
                    && !CanExecuteCommand(session, string.Empty, command))
                    continue;

                // Shortest alias wins as the display name (ties broken alphabetically so the
                // choice is stable between calls); the rest are listed as alternatives.
                List<string> keys = pair.Value
                    .OrderBy(x => x.Length)
                    .ThenBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                string name = _prefix + keys[0];

                commands.Add((name, new
                {
                    name,
                    aliases = keys.Skip(1).Select(x => _prefix + x).ToArray(),
                    description = command.Description ?? "",
                    usage = string.IsNullOrWhiteSpace(command.Parameters)
                        ? name
                        : name + " " + command.Parameters,
                    category,
                    access = category
                }));
            }

            WebOverlay.Send(session, "commands", new
            {
                categories = VisibleCategories(habbo),
                commands = commands
                    .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(x => x.Payload)
                    .ToArray()
            });
        }

        /// <summary>
        /// Tab order for the overlay. "All" and "Roleplay" are always offered; VIP unlocks above
        /// subscription rank 1 and Staff above rank 3.
        /// </summary>
        private static string[] VisibleCategories(Habbo habbo)
        {
            List<string> categories = new() { "All", CommandCategory.Roleplay };

            if (habbo.VipRank > 1)
                categories.Add(CommandCategory.Vip);

            if (habbo.Rank > 3)
                categories.Add(CommandCategory.Staff);

            return categories.ToArray();
        }

        private static bool CanSeeCategory(Habbo habbo, string category)
        {
            if (category == CommandCategory.Vip)
                return habbo.VipRank > 1;

            if (category == CommandCategory.Staff)
                return habbo.Rank > 3;

            return true;
        }

        private bool IsStaffCommand(IChatCommand command) =>
            command != null
            && _categoryByCommand.TryGetValue(command.GetType().Name, out string category)
            && category == CommandCategory.Staff;

        /// <summary>
        /// Death and cuffs block <b>every</b> command, not just the roleplay ones — a corpse and a
        /// suspect in handcuffs can't act at all. Two escape hatches: Staff-category commands (a
        /// moderator has to be able to moderate whatever state their character is in) and commands
        /// that opt in via <see cref="IChatCommand.UsableWhileDead"/> /
        /// <see cref="IChatCommand.UsableWhileCuffed"/>.
        ///
        /// Deliberately mirrors the item gates in <see cref="Roleplay.RpItem.Item.UserRpItems.Use"/>
        /// so the two halves of the RP surface refuse the same things for the same reasons.
        /// </summary>
        private bool IsBlockedByState(GameClient session, IChatCommand command)
        {
            Habbo habbo = session?.GetHabbo();
            if (habbo == null || command == null || IsStaffCommand(command))
                return false;

            if (!command.UsableWhileDead && habbo.GetRpStats()?.IsDead == true) {
                session.SendWhisper("You're dead — you can't do that.", 1);
                return true;
            }

            if (!command.UsableWhileCuffed && PlusEnvironment.GetPoliceManager().IsCuffed(habbo.Id)) {
                session.SendWhisper("You can't do that while cuffed.", 1);
                return true;
            }

            return false;
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
            Register(new OnlineUsersCommand());
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
            _categoryByCommand[command.GetType().Name] = _registeringCategory;
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
            foreach (IChatCommand cmd in _registry.Values) {
                if (!string.IsNullOrEmpty(cmd.PermissionRequired) && !byFuse.ContainsKey(cmd.PermissionRequired))
                    byFuse[cmd.PermissionRequired] = cmd;
            }

            try {
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                foreach (var row in db.PermissionsCommands.Select(c => new { c.Command, c.Keys }).ToList()) {
                    if (string.IsNullOrWhiteSpace(row.Keys) || string.IsNullOrWhiteSpace(row.Command))
                        continue;

                    // Class name wins (unambiguous); fall back to the permission fuse.
                    if (!_registry.TryGetValue(row.Command, out IChatCommand cmd) &&
                        !byFuse.TryGetValue(row.Command, out cmd))
                        continue;

                    foreach (string alias in row.Keys.Split(';'))
                        Bind(alias, cmd);
                }
            } catch (Exception e) {
                // Most likely the permissions_commands.keys column hasn't been added yet. Without it
                // no commands bind — log loudly so the missing migration is obvious.
                Core.ExceptionLogger.LogException(e);
            }
        }

        public static string MergeParams(string[] @params, int start)
        {
            var merged = new StringBuilder();
            for (int i = start; i < @params.Length; i++) {
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