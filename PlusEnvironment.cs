using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Plus.Communication.Encryption;
using Plus.Communication.Encryption.Keys;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Rcon;
using Plus.Core;
using Plus.Core.FigureData;
using Plus.Core.Language;
using Plus.Core.Settings;
using Plus.Database.EF;
using Plus.HabboHotel;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Permissions;
using Plus.HabboHotel.Roleplay.Banking;
using Plus.HabboHotel.Roleplay.Combat;
using Plus.HabboHotel.Roleplay.Cooldowns;
using Plus.HabboHotel.Roleplay.Hospital;
using Plus.HabboHotel.Roleplay.Trash;
using Plus.HabboHotel.Roleplay.Police;
using Plus.HabboHotel.Roleplay.Stock;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.UserData;
using Plus.Network;
using Plus.Utilities;
using Plus.HabboHotel.Roleplay.Mayhem;

namespace Plus
{
    public static class PlusEnvironment
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PlusEnvironment));

        public const string PrettyVersion = "Plus Emulator";
        public const string PrettyBuild = "3.4.3.0";

        private static Encoding _defaultEncoding;
        public static CultureInfo CultureInfo;

        private static Game _game;
        private static ConfigurationData _configuration;
        private static LanguageManager _languageManager;
        private static SettingsManager _settingsManager;
        private static RedisManager _redisManager;
        private static RconSocket _rcon;
        private static FigureDataManager _figureManager;
        private static NetworkBootstrap _bootstrap;

        // rp
        private static SkillManager _skillManager;
        private static CombatManager _combatManager;
        private static WeaponManager _weaponManager;
        private static RpItemManager _rpItemManager;
        private static Plus.HabboHotel.Roleplay.Clothing.RpClothingStoreManager _rpClothingStoreManager;
        private static Plus.HabboHotel.Roleplay.Crafting.CraftingManager _craftingManager;
        private static RoomStockManager _roomStockManager;
        private static Plus.HabboHotel.Roleplay.Crime.RpCrimeManager _rpCrimeManager;
        private static Plus.HabboHotel.Roleplay.Crime.JailManager _jailManager;
        private static HospitalManager _hospitalManager;
        private static SlowHealManager _slowHealManager;
        private static BankingManager _bankingManager;
        private static PoliceManager _policeManager;
        private static TrashSearchManager _trashSearchManager;
        private static RpCooldownManager _rpCooldownManager;
        private static Plus.HabboHotel.GameClients.DisconnectDelayManager _disconnectDelayManager;
        private static MayhemManager _mayhemManager;
        private static Plus.HabboHotel.Roleplay.Rubbish.RubbishManager _rubbishManager;

        // TODO: Get rid?
        public static bool Event = false;
        public static DateTime LastEvent;
        public static DateTime ServerStarted;

        private static readonly List<char> AllowedChars = new(new[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l',
            'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
            'y', 'z', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-', '.'
        });

        private static readonly ConcurrentDictionary<int, Habbo> UsersCached = new();

        public static string ClientRevision = "";

        public static void Initialize()
        {
            ServerStarted = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine();
            Console.WriteLine("                     ____  __           ________  _____  __");
            Console.WriteLine(@"                    / __ \/ /_  _______/ ____/  |/  / / / /");
            Console.WriteLine("                   / /_/ / / / / / ___/ __/ / /|_/ / / / / ");
            Console.WriteLine("                  / ____/ / /_/ (__  ) /___/ /  / / /_/ /  ");
            Console.WriteLine(@"                 /_/   /_/\__,_/____/_____/_/  /_/\____/ ");

            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("                                " + PrettyVersion + " <Build " + PrettyBuild + ">");
            Console.WriteLine("                                http://PlusIndustry.com");

            Console.WriteLine("");
            Console.Title = "Loading Plus Emulator";
            _defaultEncoding = Encoding.Default;

            Console.WriteLine("");
            Console.WriteLine("");

            CultureInfo = CultureInfo.CreateSpecificCulture("en-GB");

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo;
            Thread.CurrentThread.CurrentCulture = CultureInfo;
            Thread.CurrentThread.CurrentUICulture = CultureInfo;

            try {
                _configuration = new ConfigurationData($"Config{Path.DirectorySeparatorChar}config.ini");

                var connectionString = new MySqlConnectionStringBuilder
                {
                    ConnectionTimeout = 10,
                    Database = GetConfig().Data["db.name"],
                    DefaultCommandTimeout = 30,
                    MaximumPoolSize = uint.Parse(GetConfig().Data["db.pool.maxsize"]),
                    MinimumPoolSize = uint.Parse(GetConfig().Data["db.pool.minsize"]),
                    Password = GetConfig().Data["db.password"],
                    Pooling = true,
                    Port = uint.Parse(GetConfig().Data["db.port"]),
                    Server = GetConfig().Data["db.hostname"],
                    UserID = GetConfig().Data["db.username"],
                    AllowZeroDateTime = true,
                    ConvertZeroDateTime = true,
                    SslMode = MySqlSslMode.None
                };

                WavePlusContextFactory.Init(
                    GetConfig().Data["db.hostname"],
                    uint.Parse(GetConfig().Data["db.port"]),
                    GetConfig().Data["db.name"],
                    GetConfig().Data["db.username"],
                    GetConfig().Data["db.password"],
                    uint.Parse(GetConfig().Data["db.pool.minsize"]),
                    uint.Parse(GetConfig().Data["db.pool.maxsize"]));

                bool dbConnected;
                try {
                    using WavePlusContext db = GetDbContext();
                    dbConnected = db.Database.CanConnect();
                } catch {
                    dbConnected = false;
                }

                if (!dbConnected) {
                    Log.Error("Failed to Connect to the specified MySQL server.");
                    Console.ReadKey(true);
                    Environment.Exit(1);
                    return;
                }

                Log.Info("Connected to Database!");

                _redisManager = new RedisManager();
                _redisManager.Init();

                //Reset our statistics first.
                using (WavePlusContext db = GetDbContext()) {
                    db.Database.ExecuteSqlRaw("TRUNCATE `catalog_marketplace_data`");
                    db.Rooms.Where(r => r.UsersNow > 0).ExecuteUpdate(s => s.SetProperty(r => r.UsersNow, 0));
                    db.Users.Where(u => u.Online == "1").ExecuteUpdate(s => s.SetProperty(u => u.Online, "0"));
                    db.ServerStatuses.ExecuteUpdate(s => s.SetProperty(x => x.UsersOnline, 0).SetProperty(x => x.LoadedRooms, 0));
                }

                //Get the configuration & Game set.
                _languageManager = new LanguageManager();
                _languageManager.Init();

                _settingsManager = new SettingsManager();
                _settingsManager.Init();

                _figureManager = new FigureDataManager();
                _figureManager.Init();

                // Get new RP stuff
                _skillManager = new SkillManager();
                _skillManager.Init();

                _combatManager = new CombatManager();

                _hospitalManager = new HospitalManager();

                _slowHealManager = new SlowHealManager();

                _weaponManager = new WeaponManager();
                _weaponManager.Init();

                _rpItemManager = new RpItemManager();
                _rpItemManager.Init();

                _rpClothingStoreManager = new Plus.HabboHotel.Roleplay.Clothing.RpClothingStoreManager();
                _rpClothingStoreManager.Init();

                _craftingManager = new Plus.HabboHotel.Roleplay.Crafting.CraftingManager();
                _craftingManager.Init();

                _roomStockManager = new RoomStockManager();
                _roomStockManager.Init();

                _rpCrimeManager = new Plus.HabboHotel.Roleplay.Crime.RpCrimeManager();
                _rpCrimeManager.Init();

                _jailManager = new Plus.HabboHotel.Roleplay.Crime.JailManager();

                _bankingManager = new BankingManager();

                _policeManager = new PoliceManager();

                _disconnectDelayManager = new Plus.HabboHotel.GameClients.DisconnectDelayManager();

                _mayhemManager = new MayhemManager();

                _rubbishManager = new Plus.HabboHotel.Roleplay.Rubbish.RubbishManager();

                _trashSearchManager = new TrashSearchManager();
                _trashSearchManager.Init();

                _rpCooldownManager = new RpCooldownManager();

                //Have our encryption ready.
                HabboEncryptionV2.Initialize(new RSAKeys());

                //Make sure Rcon is connected before we allow clients to Connect.
                _rcon = new RconSocket(GetConfig().Data["rcon.tcp.bindip"], int.Parse(GetConfig().Data["rcon.tcp.port"]), GetConfig().Data["rcon.tcp.allowedaddr"].Split(Convert.ToChar(";")));

                _game = new Game();
                _game.StartGameLoop();

                //Accept connections.
                _bootstrap = new NetworkBootstrap(GetConfig().Data["game.tcp.bindip"], int.Parse(GetConfig().Data["game.tcp.port"]));
                _bootstrap.InitAsync().Wait();

                TimeSpan timeUsed = DateTime.Now - ServerStarted;

                Console.WriteLine();

                Log.Info("EMULATOR -> READY! (" + timeUsed.Seconds + " s, " + timeUsed.Milliseconds + " ms)");
            } catch (KeyNotFoundException) {
                Log.Error("Please check your configuration file - some values appear to be missing.");
                Log.Error("Press any key to shut down ...");

                Console.ReadKey(true);
                Environment.Exit(1);
            } catch (InvalidOperationException e) {
                Log.Error("Failed to initialize PlusEmulator: " + e.Message);
                Log.Error("Press any key to shut down ...");
                Console.ReadKey(true);
                Environment.Exit(1);
            } catch (Exception e) {
                Log.Error("Fatal error during startup: " + e);
                Log.Error("Press a key to exit");

                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        public static bool EnumToBool(string @enum)
        {
            return @enum == "1";
        }

        public static string BoolToEnum(bool @bool)
        {
            return @bool ? "1" : "0";
        }

        public static int GetRandomNumber(int min, int max)
        {
            return RandomNumber.GenerateNewRandom(min, max);
        }

        public static double GetUnixTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public static double GetUnixTimestampPrecise()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        }

        public static long Now()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            double unixTime = ts.TotalMilliseconds;
            return (long)unixTime;
        }

        public static long MonotonicMs() => Environment.TickCount64;

        public static string FilterFigure(string figure)
        {
            foreach (char character in figure) {
                if (!IsValid(character))
                    return "sh-3338-93.ea-1406-62.hr-831-49.ha-3331-92.hd-180-7.ch-3334-93-1408.lg-3337-92.ca-1813-62";
            }

            return figure;
        }

        private static bool IsValid(char character)
        {
            return AllowedChars.Contains(character);
        }

        public static bool IsValidAlphaNumeric(string inputStr)
        {
            inputStr = inputStr.ToLower();
            if (string.IsNullOrEmpty(inputStr)) {
                return false;
            }

            for (int i = 0; i < inputStr.Length; i++) {
                if (!IsValid(inputStr[i])) {
                    return false;
                }
            }

            return true;
        }

        public static string GetUsernameById(int userId)
        {
            string name = "Unknown User";

            GameClient client = GetGame().GetClientManager().GetClientByUserId(userId);
            if (client != null && client.GetHabbo() != null)
                return client.GetHabbo().Username;

            UserCache user = GetGame().GetCacheManager().GenerateUser(userId);
            if (user != null)
                return user.Username;

            using (WavePlusContext db = GetDbContext())
                name = db.Users.Where(u => u.Id == userId).Select(u => u.Username).FirstOrDefault();

            if (string.IsNullOrEmpty(name))
                name = "Unknown User";

            return name;
        }

        public static Habbo GetHabboById(int userId)
        {
            try {
                GameClient client = GetGame().GetClientManager().GetClientByUserId(userId);
                Habbo online = client?.GetHabbo();

                if (online != null && online.Id > 0) {
                    UsersCached.TryRemove(userId, out _);
                    return online;
                }

                if (UsersCached.TryGetValue(userId, out Habbo cached))
                    return cached;

                UserData data = UserDataFactory.GetUserData(userId);
                Habbo generated = data?.User;
                if (generated == null)
                    return null;

                generated.InitInformation(data);
                UsersCached.TryAdd(userId, generated);
                return generated;
            } catch (Exception e) {
                // Swallowing this silently is why the failure only ever surfaced as "an error
                // occurred while finding that user's profile" with nothing to go on.
                ExceptionLogger.LogException(e);
                return null;
            }
        }

        public static Habbo GetHabboByUsername(string userName)
        {
            try {
                using (WavePlusContext db = GetDbContext()) {
                    int id = db.Users.Where(u => u.Username == userName).Select(u => u.Id).FirstOrDefault();
                    if (id > 0)
                        return GetHabboById(id);
                }

                return null;
            } catch {
                return null;
            }
        }

        private static int _shuttingDown;

        public static void PerformShutDown()
        {
            if (Interlocked.CompareExchange(ref _shuttingDown, 1, 0) != 0)
                return;

            Console.Clear();
            Log.Info("Server shutting down...");
            Console.Title = "PLUS EMULATOR: SHUTTING DOWN!";

            GetGame().GetClientManager().SendPacket(new BroadcastMessageAlertComposer(GetLanguageManager().TryGetValue("server.shutdown.message")));
            GetGame().StopGameLoop();
            GetGame().GetPersistenceScheduler().Stop();
            GetGame().GetPersistenceScheduler().FlushAll(); //Final failsafe flush (user + world data)
            Thread.Sleep(2500);
            GetGame().GetPacketManager().UnregisterAll(); //Unregister the packets.
            GetGame().GetPacketManager().WaitForAllToComplete();
            GetGame().GetGroupManager().SaveAll(); //Flush any pending group role/permission edits
            GetGame().GetClientManager().CloseAll(); //Close all connections (runs per-user disconnection saves)
            _bootstrap.Shutdown().Wait();
            _bootstrap.ShutdownWorkers();
            GetGame().GetRoomManager().Dispose(); //Stop the game loop.

            using (WavePlusContext db = GetDbContext()) {
                db.Database.ExecuteSqlRaw("TRUNCATE `catalog_marketplace_data`");
                db.Users.ExecuteUpdate(s => s.SetProperty(u => u.Online, "0").SetProperty(u => u.AuthTicket, (string)null));
                db.Rooms.Where(r => r.UsersNow > 0).ExecuteUpdate(s => s.SetProperty(r => r.UsersNow, 0));
                db.ServerStatuses.ExecuteUpdate(s => s.SetProperty(x => x.UsersOnline, 0).SetProperty(x => x.LoadedRooms, 0));
            }

            Log.Info("Plus Emulator has successfully shutdown.");

            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        public static ConfigurationData GetConfig()
        {
            return _configuration;
        }

        public static RedisManager GetRedis()
        {
            return _redisManager;
        }

        public static Encoding GetDefaultEncoding()
        {
            return _defaultEncoding;
        }

        public static Game GetGame()
        {
            return _game;
        }

        public static RconSocket GetRconSocket()
        {
            return _rcon;
        }

        public static FigureDataManager GetFigureManager()
        {
            return _figureManager;
        }

        public static WavePlusContext GetDbContext()
        {
            return WavePlusContextFactory.Create();
        }

        public static LanguageManager GetLanguageManager()
        {
            return _languageManager;
        }

        public static SettingsManager GetSettingsManager()
        {
            return _settingsManager;
        }

        public static SkillManager GetSkillManager()
        {
            return _skillManager;
        }

        public static CombatManager GetCombatManager()
        {
            return _combatManager;
        }

        public static HospitalManager GetHospitalManager()
        {
            return _hospitalManager;
        }

        public static SlowHealManager GetSlowHealManager()
        {
            return _slowHealManager;
        }

        public static WeaponManager GetWeaponManager()
        {
            return _weaponManager;
        }

        public static RpItemManager GetRpItemManager()
        {
            return _rpItemManager;
        }

        public static Plus.HabboHotel.Roleplay.Clothing.RpClothingStoreManager GetRpClothingStoreManager()
        {
            return _rpClothingStoreManager;
        }

        public static Plus.HabboHotel.Roleplay.Crafting.CraftingManager GetCraftingManager()
        {
            return _craftingManager;
        }

        public static RoomStockManager GetRoomStockManager()
        {
            return _roomStockManager;
        }

        public static Plus.HabboHotel.Roleplay.Crime.RpCrimeManager GetRpCrimeManager()
        {
            return _rpCrimeManager;
        }

        public static Plus.HabboHotel.Roleplay.Crime.JailManager GetJailManager()
        {
            return _jailManager;
        }

        public static PoliceManager GetPoliceManager()
        {
            return _policeManager;
        }

        public static Plus.HabboHotel.Roleplay.Mayhem.MayhemManager GetMayhemManager()
        {
            return _mayhemManager;
        }

        public static Plus.HabboHotel.Roleplay.Rubbish.RubbishManager GetRubbishManager()
        {
            return _rubbishManager;
        }

        public static Plus.HabboHotel.GameClients.DisconnectDelayManager GetDisconnectDelayManager()
        {
            return _disconnectDelayManager;
        }

        public static TrashSearchManager GetTrashSearchManager()
        {
            return _trashSearchManager;
        }

        public static RpCooldownManager GetRpCooldownManager()
        {
            return _rpCooldownManager;
        }

        public static BankingManager GetBankingManager()
        {
            return _bankingManager;
        }

        public static ICollection<Habbo> GetUsersCached()
        {
            return UsersCached.Values;
        }

        public static bool RemoveFromCache(int id, out Habbo data)
        {
            return UsersCached.TryRemove(id, out data);
        }
    }
}