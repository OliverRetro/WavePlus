using System;
using System.Threading;
using System.Threading.Tasks;
using Plus.Communication.Packets;
using Plus.Core;
using Plus.Core.Persistence;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.Badges;
using Plus.HabboHotel.Bots;
using Plus.HabboHotel.Cache;
using Plus.HabboHotel.Camera;
using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Games;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Televisions;
using Plus.HabboHotel.LandingView;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Navigator;
using Plus.HabboHotel.Permissions;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rewards;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat;
using Plus.HabboHotel.Subscriptions;
using Plus.HabboHotel.Roleplay.Corporation;
using Plus.HabboHotel.Roleplay.Macros;
using Plus.HabboHotel.Talents;

namespace Plus.HabboHotel
{
    public class Game
    {
        private readonly PacketManager _packetManager;
        private readonly GameClientManager _clientManager;
        private readonly ModerationManager _moderationManager;
        private readonly ItemDataManager _itemDataManager;
        private readonly CatalogManager _catalogManager;
        private readonly TelevisionManager _televisionManager; //TODO: Initialize from the item manager.
        private readonly NavigatorManager _navigatorManager;
        private readonly RoomManager _roomManager;
        private readonly ChatManager _chatManager;
        private readonly GroupManager _groupManager;
        private readonly QuestManager _questManager;
        private readonly AchievementManager _achievementManager;
        private readonly TalentTrackManager _talentTrackManager;
        private readonly LandingViewManager _landingViewManager; //TODO: Rename class
        private readonly GameDataManager _gameDataManager;
        private readonly ServerStatusUpdater _globalUpdater;
        private readonly BotManager _botManager;
        private readonly CacheManager _cacheManager;
        private readonly RewardManager _rewardManager;
        private readonly BadgeManager _badgeManager;
        private readonly PermissionManager _permissionManager;
        private readonly SubscriptionManager _subscriptionManager;
        private readonly ShiftManager _shiftManager;
        private readonly MacroManager _macroManager;
        private readonly CameraManager _cameraManager;
        private readonly PersistenceScheduler _persistenceScheduler;

        private bool _cycleEnded;
        private bool _cycleActive;
        private Task _gameCycle;
        private PeriodicTimer _cycleTimer;
        private long _nextHospitalCycleMs;
        private long _nextJailCycleMs;
        private long _nextPoliceCycleMs;
        private long _nextTrashCycleMs;
        private long _nextAchievementFlushMs;

        // How often the loop POLLS. RoomManager.CycleIntervalMs (50ms) is the grid steps actually
        // dispatch on; this only decides how late past that grid a tick can fire. Movement is
        // paced by that grid, and the client animates a tile over exactly the grid interval, so
        // tick lateness is directly visible as walking stutter. Polling AT the grid interval is
        // the worst case: the poll phase beats against the grid phase and every tick lands a
        // different 0-50ms late. 5ms bounds the lateness to about one frame.
        private const int CyclePollMs = 5;
        private const int HospitalCycleIntervalMs = 1000;
        private const int JailCycleIntervalMs = 1000;
        private const int PoliceCycleIntervalMs = 250;
        private const int TrashCycleIntervalMs = 250;
        private const int AchievementFlushIntervalMs = 60000; // batch-push queued achievement progress once a minute

        public Game()
        {
            _packetManager = new PacketManager();
            _clientManager = new GameClientManager();

            _moderationManager = new ModerationManager();
            _moderationManager.Init();

            _itemDataManager = new ItemDataManager();
            _itemDataManager.Init();

            _catalogManager = new CatalogManager();
            _catalogManager.Init(_itemDataManager);

            _televisionManager = new TelevisionManager();
            _televisionManager.Init();

            _navigatorManager = new NavigatorManager();
            _navigatorManager.Init();

            _roomManager = new RoomManager();
            _roomManager.LoadModels();

            _chatManager = new ChatManager();

            _groupManager = new GroupManager();
            _groupManager.Init();

            _questManager = new QuestManager();
            _questManager.Init();

            _achievementManager = new AchievementManager();
            _achievementManager.Init();

            _talentTrackManager = new TalentTrackManager();
            _talentTrackManager.Init();

            _landingViewManager = new LandingViewManager();
            _landingViewManager.Init();

            _gameDataManager = new GameDataManager();
            _gameDataManager.Init();

            _globalUpdater = new ServerStatusUpdater();
            _globalUpdater.Init();

            _botManager = new BotManager();
            _botManager.Init();

            _cacheManager = new CacheManager();

            _rewardManager = new RewardManager();
            _rewardManager.Init();

            _badgeManager = new BadgeManager();
            _badgeManager.Init();

            _permissionManager = new PermissionManager();
            _permissionManager.Init();

            _chatManager.GetCommands().ApplyDatabaseAliases();

            _subscriptionManager = new SubscriptionManager();
            _subscriptionManager.Init();

            _shiftManager = new ShiftManager();

            _macroManager = new MacroManager();

            _cameraManager = new CameraManager();

            _persistenceScheduler = new PersistenceScheduler();
            PersistenceRegistry.RegisterAll(_persistenceScheduler);
        }

        public void StartGameLoop()
        {
            // Pin the public rooms (and their items) into memory before clients connect;
            // from here on they only touch the DB on the 15-minute failsafe and shutdown.
            Core.Cache.PublicRoomCache.LoadAll();

            _cycleActive = true;

            // A single long-running loop instead of a System.Threading.Timer: a Timer queues
            // callbacks on schedule whether or not the previous one finished, so an overrunning
            // cycle either piles up threads (corrupting the room tick's phase - OnCycle is a
            // read-modify-write on a plain field) or, with a reentrancy skip, silently drops a
            // whole poll period. A loop can do neither. PeriodicTimer on .NET 8+ uses the OS
            // high-resolution timer, so the poll really fires every CyclePollMs rather than
            // being quantised to the ~15.6ms system clock the way Thread.Sleep is.
            _cycleTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(CyclePollMs));
            _gameCycle = Task.Run(GameCycleLoop);

            _persistenceScheduler.Start();
        }

        private async Task GameCycleLoop()
        {
            try {
                while (_cycleActive && await _cycleTimer.WaitForNextTickAsync()) {
                    try {
                        RunGameCycle();
                    } catch (Exception e) {
                        ExceptionLogger.LogException(e);
                    }
                }
            } catch (OperationCanceledException) {
                // Timer disposed during shutdown.
            }

            _cycleEnded = true;
        }

        private void RunGameCycle()
        {
            _cycleEnded = false;

            long nowMs = PlusEnvironment.Now();

            PlusEnvironment.GetGame().GetRoomManager().OnCycle();
            PlusEnvironment.GetGame().GetClientManager().OnCycle();
            PlusEnvironment.GetGame().GetGroupManager().OnCycle();

            if (nowMs >= _nextHospitalCycleMs) {
                _nextHospitalCycleMs = nowMs + HospitalCycleIntervalMs;
                PlusEnvironment.GetHospitalManager().OnCycle();
            }

            if (nowMs >= _nextJailCycleMs) {
                _nextJailCycleMs = nowMs + JailCycleIntervalMs;
                PlusEnvironment.GetJailManager().OnCycle();
            }

            if (nowMs >= _nextPoliceCycleMs) {
                _nextPoliceCycleMs = nowMs + PoliceCycleIntervalMs;
                PlusEnvironment.GetPoliceManager()?.OnCycle();
            }

            if (nowMs >= _nextTrashCycleMs) {
                _nextTrashCycleMs = nowMs + TrashCycleIntervalMs;
                PlusEnvironment.GetTrashSearchManager()?.OnCycle();
                PlusEnvironment.GetCraftingManager()?.OnCycle();
                PlusEnvironment.GetRpCooldownManager()?.OnCycle();
            }

            if (nowMs >= _nextAchievementFlushMs) {
                _nextAchievementFlushMs = nowMs + AchievementFlushIntervalMs;
                PlusEnvironment.GetGame().GetAchievementManager().FlushQueuedProgress();
            }

            // Ticked every cycle (it no-ops while idle) so a slow heal reacts to incoming
            // damage almost immediately rather than on a coarse interval.
            PlusEnvironment.GetSlowHealManager()?.OnCycle();

            PlusEnvironment.GetMayhemManager()?.OnCycle();

            PlusEnvironment.GetRubbishManager()?.OnCycle();

            _cycleEnded = true;
        }

        public void StopGameLoop()
        {
            _cycleActive = false;
            _cycleTimer?.Dispose(); // wakes WaitForNextTickAsync so the loop exits promptly

            while (!_cycleEnded) {
                Thread.Sleep(CyclePollMs);
            }
        }

        public PacketManager GetPacketManager()
        {
            return _packetManager;
        }

        public GameClientManager GetClientManager()
        {
            return _clientManager;
        }

        public CatalogManager GetCatalog()
        {
            return _catalogManager;
        }

        public NavigatorManager GetNavigator()
        {
            return _navigatorManager;
        }

        public ItemDataManager GetItemManager()
        {
            return _itemDataManager;
        }

        public CameraManager GetCameraManager()
        {
            return _cameraManager;
        }

        public RoomManager GetRoomManager()
        {
            return _roomManager;
        }

        public AchievementManager GetAchievementManager()
        {
            return _achievementManager;
        }

        public TalentTrackManager GetTalentTrackManager()
        {
            return _talentTrackManager;
        }

        public ModerationManager GetModerationManager()
        {
            return _moderationManager;
        }

        public PermissionManager GetPermissionManager()
        {
            return _permissionManager;
        }

        public SubscriptionManager GetSubscriptionManager()
        {
            return _subscriptionManager;
        }

        public QuestManager GetQuestManager()
        {
            return _questManager;
        }

        public GroupManager GetGroupManager()
        {
            return _groupManager;
        }

        public LandingViewManager GetLandingManager()
        {
            return _landingViewManager;
        }

        public TelevisionManager GetTelevisionManager()
        {
            return _televisionManager;
        }

        public ChatManager GetChatManager()
        {
            return _chatManager;
        }

        public GameDataManager GetGameDataManager()
        {
            return _gameDataManager;
        }

        public BotManager GetBotManager()
        {
            return _botManager;
        }

        public CacheManager GetCacheManager()
        {
            return _cacheManager;
        }

        public RewardManager GetRewardManager()
        {
            return _rewardManager;
        }

        public BadgeManager GetBadgeManager()
        {
            return _badgeManager;
        }

        public ShiftManager GetShiftManager()
        {
            return _shiftManager;
        }

        public MacroManager GetMacroManager()
        {
            return _macroManager;
        }

        public PersistenceScheduler GetPersistenceScheduler()
        {
            return _persistenceScheduler;
        }
    }
}