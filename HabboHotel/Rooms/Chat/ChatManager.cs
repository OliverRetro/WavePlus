using log4net;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Rooms.Chat.Emotions;
using Plus.HabboHotel.Rooms.Chat.Filter;
using Plus.HabboHotel.Rooms.Chat.Links;
using Plus.HabboHotel.Rooms.Chat.Logs;
using Plus.HabboHotel.Rooms.Chat.Pets.Commands;
using Plus.HabboHotel.Rooms.Chat.Pets.Locale;
using Plus.HabboHotel.Rooms.Chat.Styles;

namespace Plus.HabboHotel.Rooms.Chat
{
    public sealed class ChatManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ChatManager));

        /// <summary>
        /// Chat Emoticons.
        /// </summary>
        private readonly ChatEmotionsManager _emotions;

        /// <summary>
        /// Chatlog Manager
        /// </summary>
        private readonly ChatLogManager _logs;

        /// <summary>
        /// Filter Manager.
        /// </summary>
        private readonly WordFilterManager _filter;

        /// <summary>
        /// Commands.
        /// </summary>
        private readonly CommandManager _commands;

        /// <summary>
        /// Pet Commands.
        /// </summary>
        private readonly PetCommandManager _petCommands;

        /// <summary>
        /// Pet Locale.
        /// </summary>
        private readonly PetLocale _petLocale;

        /// <summary>
        /// Chat styles.
        /// </summary>
        private readonly ChatStyleManager _chatStyles;

        /// <summary>
        /// Clickable-link whitelist.
        /// </summary>
        private readonly LinkWhitelistManager _linkWhitelist;

        /// <summary>
        /// Initializes a new instance of the ChatManager class.
        /// </summary>
        public ChatManager()
        {
            _emotions = new ChatEmotionsManager();
            _logs = new ChatLogManager();

            _filter = new WordFilterManager();
            _filter.Init();

            _commands = new CommandManager(":");
            _petCommands = new PetCommandManager();
            _petLocale = new PetLocale();

            _chatStyles = new ChatStyleManager();
            _chatStyles.Init();

            _linkWhitelist = new LinkWhitelistManager();
            _linkWhitelist.Init();

            Log.Info("Chat Manager -> LOADED");
        }

        public ChatEmotionsManager GetEmotions()
        {
            return _emotions;
        }

        public ChatLogManager GetLogs()
        {
            return _logs;
        }

        public WordFilterManager GetFilter()
        {
            return _filter;
        }

        public CommandManager GetCommands()
        {
            return _commands;
        }

        public PetCommandManager GetPetCommands()
        {
            return _petCommands;
        }

        public PetLocale GetPetLocale()
        {
            return _petLocale;
        }

        public ChatStyleManager GetChatStyles()
        {
            return _chatStyles;
        }

        public LinkWhitelistManager GetLinkWhitelist()
        {
            return _linkWhitelist;
        }
    }
}