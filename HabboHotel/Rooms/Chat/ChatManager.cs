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

        private readonly ChatEmotionsManager _emotions;

        private readonly ChatLogManager _logs;

        private readonly WordFilterManager _filter;

        private readonly CommandManager _commands;

        private readonly PetCommandManager _petCommands;

        private readonly PetLocale _petLocale;

        private readonly ChatStyleManager _chatStyles;

        private readonly LinkWhitelistManager _linkWhitelist;

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