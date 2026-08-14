namespace Plus.HabboHotel.Items
{
    public enum InteractionType
    {
        None,
        Gate,
        PostIt,
        Moodlight,
        Trophy,
        Bed,
        Scoreboard,
        VendingMachine,
        Alert,
        OneWayGate,
        LoveShuffler,
        HabboWheel,
        Dice,
        Bottle,
        Hopper,
        Teleport,
        Pool,
        Roller,
        FootballGate,
        Pet,
        IceSkates,
        NormalSkates,
        LowPool,
        HalloweenPool,
        Football,
        FootballGoalGreen,
        FootballGoalYellow,
        FootballGoalBlue,
        FootballGoalRed,
        FootballCounterGreen,
        FootballCounterYellow,
        FootballCounterBlue,
        FootballCounterRed,
        BanzaiGateBlue,
        BanzaiGateRed,
        BanzaiGateYellow,
        BanzaiGateGreen,
        BanzaiFloor,
        BanzaiScoreBlue,
        BanzaiScoreRed,
        BanzaiScoreYellow,
        BanzaiScoreGreen,
        BanzaiCounter,
        BanzaiTele,
        BanzaiPuck,
        BanzaiPyramid,
        FreezeTimer,
        FreezeExit,
        FreezeRedCounter,
        FreezeBlueCounter,
        FreezeYellowCounter,
        FreezeGreenCounter,
        FreezeYellowGate,
        FreezeRedGate,
        FreezeGreenGate,
        FreezeBlueGate,
        FreezeTileBlock,
        FreezeTile,
        Jukebox,
        MusicDisc,
        PuzzleBox,
        Toner,

        PressurePlate,

        WfFloorSwitch1,
        WfFloorSwitch2,

        Gift,
        Background,
        Mannequin,
        GateVip,
        GuildItem,
        GuildGate,
        GuildForum,

        Tent,
        TentSmall,
        BadgeDisplay,
        StackTool,
        Television,

        WiredEffect,
        WiredTrigger,
        WiredCondition,

        Wallpaper,
        Floor,
        Landscape,

        Badge,
        CrackableEgg,
        Effect,
        Deal,
        RoomDeal,

        HorseSaddle1,
        HorseSaddle2,
        HorseHairstyle,
        HorseBodyDye,
        HorseHairDye,

        GnomeBox,
        Bot,
        PurchasableClothing,
        PetBreedingBox,
        Arrow,
        LoveLock,
        MonsterPlantSeed,
        Cannon,
        Counter,
        CameraPicture,
        FxProvider,
        Exchange,
        TrashBin,
        Atm,
        Bank,
        StockVendor,
        RpVendor,
        Crafting,
        CraftingCorporation,
        Rubbish
    }

    public static class InteractionTypes
    {
        public static InteractionType GetTypeFromString(string type)
        {
            switch (type.ToLower()) {
                case "":
                case "default":
                    return InteractionType.None;
                case "gate":
                    return InteractionType.Gate;
                case "postit":
                    return InteractionType.PostIt;
                case "dimmer":
                    return InteractionType.Moodlight;
                case "trophy":
                    return InteractionType.Trophy;
                case "bed":
                    return InteractionType.Bed;
                case "scoreboard":
                    return InteractionType.Scoreboard;
                case "vendingmachine":
                    return InteractionType.VendingMachine;
                case "alert":
                    return InteractionType.Alert;
                case "onewaygate":
                    return InteractionType.OneWayGate;
                case "loveshuffler":
                    return InteractionType.LoveShuffler;
                case "habbowheel":
                    return InteractionType.HabboWheel;
                case "dice":
                    return InteractionType.Dice;
                case "hopper":
                    return InteractionType.Hopper;
                case "bottle":
                    return InteractionType.Bottle;
                case "teleport":
                    return InteractionType.Teleport;
                case "pool":
                    return InteractionType.Pool;
                case "roller":
                    return InteractionType.Roller;
                case "fbgate":
                    return InteractionType.FootballGate;
                case "iceskates":
                    return InteractionType.IceSkates;
                case "rollerskate":
                    return InteractionType.NormalSkates;
                case "lowpool":
                    return InteractionType.LowPool;
                case "haloweenpool":
                    return InteractionType.HalloweenPool;
                case "ball":
                    return InteractionType.Football;

                case "green_goal":
                    return InteractionType.FootballGoalGreen;
                case "yellow_goal":
                    return InteractionType.FootballGoalYellow;
                case "red_goal":
                    return InteractionType.FootballGoalRed;
                case "blue_goal":
                    return InteractionType.FootballGoalBlue;

                case "green_score":
                    return InteractionType.FootballCounterGreen;
                case "yellow_score":
                    return InteractionType.FootballCounterYellow;
                case "blue_score":
                    return InteractionType.FootballCounterBlue;
                case "red_score":
                    return InteractionType.FootballCounterRed;

                case "bb_blue_gate":
                    return InteractionType.BanzaiGateBlue;
                case "bb_red_gate":
                    return InteractionType.BanzaiGateRed;
                case "bb_yellow_gate":
                    return InteractionType.BanzaiGateYellow;
                case "bb_green_gate":
                    return InteractionType.BanzaiGateGreen;
                case "bb_patch":
                    return InteractionType.BanzaiFloor;

                case "bb_blue_score":
                    return InteractionType.BanzaiScoreBlue;
                case "bb_red_score":
                    return InteractionType.BanzaiScoreRed;
                case "bb_yellow_score":
                    return InteractionType.BanzaiScoreYellow;
                case "bb_green_score":
                    return InteractionType.BanzaiScoreGreen;

                case "banzaicounter":
                    return InteractionType.BanzaiCounter;
                case "bb_teleport":
                    return InteractionType.BanzaiTele;
                case "banzaipuck":
                    return InteractionType.BanzaiPuck;
                case "bb_pyramid":
                    return InteractionType.BanzaiPyramid;

                case "freezetimer":
                    return InteractionType.FreezeTimer;
                case "freezeexit":
                    return InteractionType.FreezeExit;
                case "freezeredcounter":
                    return InteractionType.FreezeRedCounter;
                case "freezebluecounter":
                    return InteractionType.FreezeBlueCounter;
                case "freezeyellowcounter":
                    return InteractionType.FreezeYellowCounter;
                case "freezegreencounter":
                    return InteractionType.FreezeGreenCounter;
                case "freezeyellowgate":
                    return InteractionType.FreezeYellowGate;
                case "freezeredgate":
                    return InteractionType.FreezeRedGate;
                case "freezegreengate":
                    return InteractionType.FreezeGreenGate;
                case "freezebluegate":
                    return InteractionType.FreezeBlueGate;
                case "freezetileblock":
                    return InteractionType.FreezeTileBlock;
                case "freezetile":
                    return InteractionType.FreezeTile;

                case "jukebox":
                    return InteractionType.Jukebox;
                case "musicdisc":
                    return InteractionType.MusicDisc;

                case "pressureplate":
                case "pressure_pad":
                    return InteractionType.PressurePlate;
                case "wf_floor_switch1":
                    return InteractionType.WfFloorSwitch1;
                case "wf_floor_switch2":
                    return InteractionType.WfFloorSwitch2;
                case "puzzlebox":
                    return InteractionType.PuzzleBox;
                case "water":
                    return InteractionType.Pool;
                case "gift":
                    return InteractionType.Gift;
                case "background":
                    return InteractionType.Background;
                case "mannequin":
                    return InteractionType.Mannequin;
                case "vip_gate":
                    return InteractionType.GateVip;
                case "roombg":
                    return InteractionType.Toner;
                case "gld_item":
                    return InteractionType.GuildItem;
                case "gld_gate":
                    return InteractionType.GuildGate;
                case "guild_forum":
                    return InteractionType.GuildForum;
                case "tent":
                    return InteractionType.Tent;
                case "tent_small":
                    return InteractionType.TentSmall;

                case "badge_display":
                    return InteractionType.BadgeDisplay;
                case "stacktool":
                    return InteractionType.StackTool;
                case "television":
                    return InteractionType.Television;

                case "wired_effect":
                    return InteractionType.WiredEffect;
                case "wired_trigger":
                    return InteractionType.WiredTrigger;
                case "wired_condition":
                    return InteractionType.WiredCondition;

                case "floor":
                    return InteractionType.Floor;
                case "wallpaper":
                    return InteractionType.Wallpaper;
                case "landscape":
                    return InteractionType.Landscape;

                case "badge":
                    return InteractionType.Badge;

                case "crackable_egg":
                    return InteractionType.CrackableEgg;
                case "effect":
                    return InteractionType.Effect;
                case "deal":
                    return InteractionType.Deal;
                case "roomdeal":
                    return InteractionType.RoomDeal;

                case "horse_saddle_1":
                    return InteractionType.HorseSaddle1;
                case "horse_saddle_2":
                    return InteractionType.HorseSaddle2;
                case "horse_hairstyle":
                    return InteractionType.HorseHairstyle;
                case "horse_body_dye":
                    return InteractionType.HorseBodyDye;
                case "horse_hair_dye":
                    return InteractionType.HorseHairDye;

                case "gnome_box":
                    return InteractionType.GnomeBox;
                case "bot":
                    return InteractionType.Bot;
                case "purchasable_clothing":
                    return InteractionType.PurchasableClothing;
                case "pet_breeding_box":
                    return InteractionType.PetBreedingBox;
                case "arrow":
                    return InteractionType.Arrow;
                case "lovelock":
                    return InteractionType.LoveLock;
                case "cannon":
                    return InteractionType.Cannon;
                case "counter":
                    return InteractionType.Counter;
                case "camera_picture":
                    return InteractionType.CameraPicture;
                case "fx_provider":
                    return InteractionType.FxProvider;
                case "exchange":
                    return InteractionType.Exchange;
                case "trash_bin":
                    return InteractionType.TrashBin;
                case "atm":
                    return InteractionType.Atm;
                case "bank":
                    return InteractionType.Bank;
                case "stock_vendor":
                    return InteractionType.StockVendor;
                case "rp_vendor":
                    return InteractionType.RpVendor;
                case "crafting":
                    return InteractionType.Crafting;
                case "crafting_corporation":
                    return InteractionType.CraftingCorporation;
                case "rubbish":
                    return InteractionType.Rubbish;
                case "pet":
                    return InteractionType.Pet;
                default: {
                        //Logging.WriteLine("Unknown interaction type in parse code: " + pType, ConsoleColor.Yellow);
                        return InteractionType.None;
                    }
            }
        }
    }
}