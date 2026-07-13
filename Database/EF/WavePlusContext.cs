using Microsoft.EntityFrameworkCore;
using Plus.Database.EF.Entities;

namespace Plus.Database.EF;

public partial class WavePlusContext(DbContextOptions<WavePlusContext> options) : DbContext(options)
{
    public virtual DbSet<Achievement> Achievements { get; set; }

    public virtual DbSet<AchievementsTalent> AchievementsTalents { get; set; }

    public virtual DbSet<BadgeDefinition> BadgeDefinitions { get; set; }

    public virtual DbSet<Ban> Bans { get; set; }

    public virtual DbSet<Bot> Bots { get; set; }

    public virtual DbSet<BotsPetCommand> BotsPetCommands { get; set; }

    public virtual DbSet<BotsPetResponse> BotsPetResponses { get; set; }

    public virtual DbSet<BotsPetdatum> BotsPetdata { get; set; }

    public virtual DbSet<BotsResponse> BotsResponses { get; set; }

    public virtual DbSet<BotsSpeech> BotsSpeeches { get; set; }

    public virtual DbSet<CatalogBotPreset> CatalogBotPresets { get; set; }

    public virtual DbSet<CatalogClothing> CatalogClothings { get; set; }

    public virtual DbSet<CatalogDeal> CatalogDeals { get; set; }

    public virtual DbSet<CatalogItem> CatalogItems { get; set; }

    public virtual DbSet<CatalogMarketplaceDatum> CatalogMarketplaceData { get; set; }

    public virtual DbSet<CatalogMarketplaceOffer> CatalogMarketplaceOffers { get; set; }

    public virtual DbSet<CatalogPage> CatalogPages { get; set; }

    public virtual DbSet<CatalogPetRace> CatalogPetRaces { get; set; }

    public virtual DbSet<CatalogPromotion> CatalogPromotions { get; set; }

    public virtual DbSet<CatalogVoucher> CatalogVouchers { get; set; }

    public virtual DbSet<Chatlog> Chatlogs { get; set; }

    public virtual DbSet<ChatlogsConsole> ChatlogsConsoles { get; set; }

    public virtual DbSet<ChatlogsConsoleInvitation> ChatlogsConsoleInvitations { get; set; }

    public virtual DbSet<ClientExternalBadgeText> ClientExternalBadgeTexts { get; set; }

    public virtual DbSet<ClientExternalText> ClientExternalTexts { get; set; }

    public virtual DbSet<Furniture> Furnitures { get; set; }

    public virtual DbSet<FurnitureCopy1> FurnitureCopy1s { get; set; }

    public virtual DbSet<GamesConfig> GamesConfigs { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<GroupMembership> GroupMemberships { get; set; }

    public virtual DbSet<GroupPermission> GroupPermissions { get; set; }

    public virtual DbSet<GroupPermissionKey> GroupPermissionKeys { get; set; }

    public virtual DbSet<GroupRequest> GroupRequests { get; set; }

    public virtual DbSet<GroupRole> GroupRoles { get; set; }

    public virtual DbSet<GroupsItem> GroupsItems { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<ItemsGroup> ItemsGroups { get; set; }

    public virtual DbSet<ItemsYoutube> ItemsYoutubes { get; set; }

    public virtual DbSet<LogsClientNamechange> LogsClientNamechanges { get; set; }

    public virtual DbSet<LogsClientStaff> LogsClientStaffs { get; set; }

    public virtual DbSet<LogsClientTrade> LogsClientTrades { get; set; }

    public virtual DbSet<MessengerFriendship> MessengerFriendships { get; set; }

    public virtual DbSet<MessengerOfflineMessage> MessengerOfflineMessages { get; set; }

    public virtual DbSet<MessengerRequest> MessengerRequests { get; set; }

    public virtual DbSet<ModerationPreset> ModerationPresets { get; set; }

    public virtual DbSet<ModerationPresetActionCategory> ModerationPresetActionCategories { get; set; }

    public virtual DbSet<ModerationPresetActionMessage> ModerationPresetActionMessages { get; set; }

    public virtual DbSet<ModerationTicket> ModerationTickets { get; set; }

    public virtual DbSet<ModerationTopic> ModerationTopics { get; set; }

    public virtual DbSet<ModerationTopicAction> ModerationTopicActions { get; set; }

    public virtual DbSet<NavigatorCategory> NavigatorCategories { get; set; }

    public virtual DbSet<NavigatorPublic> NavigatorPublics { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<PermissionsCommand> PermissionsCommands { get; set; }

    public virtual DbSet<PermissionsGroup> PermissionsGroups { get; set; }

    public virtual DbSet<PermissionsRight> PermissionsRights { get; set; }

    public virtual DbSet<PermissionsSubscription> PermissionsSubscriptions { get; set; }

    public virtual DbSet<Quest> Quests { get; set; }

    public virtual DbSet<Rank> Ranks { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomBan> RoomBans { get; set; }

    public virtual DbSet<RoomChatStyle> RoomChatStyles { get; set; }

    public virtual DbSet<RoomFilter> RoomFilters { get; set; }

    public virtual DbSet<RoomItemsMoodlight> RoomItemsMoodlights { get; set; }

    public virtual DbSet<RoomItemsTeleLink> RoomItemsTeleLinks { get; set; }

    public virtual DbSet<RoomItemsToner> RoomItemsToners { get; set; }

    public virtual DbSet<RoomModel> RoomModels { get; set; }

    public virtual DbSet<RoomPromotion> RoomPromotions { get; set; }

    public virtual DbSet<RoomRight> RoomRights { get; set; }

    public virtual DbSet<RoomTaxiPoint> RoomTaxiPoints { get; set; }

    public virtual DbSet<RpCrime> RpCrimes { get; set; }

    public virtual DbSet<RpCrimeLog> RpCrimeLogs { get; set; }

    public virtual DbSet<RpCrimePenalty> RpCrimePenalties { get; set; }

    public virtual DbSet<RpItem> RpItems { get; set; }

    public virtual DbSet<RpShiftLog> RpShiftLogs { get; set; }

    public virtual DbSet<RpSkill> RpSkills { get; set; }

    public virtual DbSet<RpSkillsLevel> RpSkillsLevels { get; set; }

    public virtual DbSet<RpTrashReward> RpTrashRewards { get; set; }

    public virtual DbSet<RpWeapon> RpWeapons { get; set; }

    public virtual DbSet<RpWeaponSkin> RpWeaponSkins { get; set; }

    public virtual DbSet<ServerLanding> ServerLandings { get; set; }

    public virtual DbSet<ServerLocale> ServerLocales { get; set; }

    public virtual DbSet<ServerReward> ServerRewards { get; set; }

    public virtual DbSet<ServerRewardLog> ServerRewardLogs { get; set; }

    public virtual DbSet<ServerSetting> ServerSettings { get; set; }

    public virtual DbSet<ServerStatus> ServerStatuses { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<Talent> Talents { get; set; }

    public virtual DbSet<TalentsSubLevel> TalentsSubLevels { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAchievement> UserAchievements { get; set; }

    public virtual DbSet<UserBadge> UserBadges { get; set; }

    public virtual DbSet<UserChatBubble> UserChatBubbles { get; set; }

    public virtual DbSet<UserClothing> UserClothings { get; set; }

    public virtual DbSet<UserEffect> UserEffects { get; set; }

    public virtual DbSet<UserFavorite> UserFavorites { get; set; }

    public virtual DbSet<UserIgnore> UserIgnores { get; set; }

    public virtual DbSet<UserInfo> UserInfos { get; set; }

    public virtual DbSet<UserPresent> UserPresents { get; set; }

    public virtual DbSet<UserQuest> UserQuests { get; set; }

    public virtual DbSet<UserRelationship> UserRelationships { get; set; }

    public virtual DbSet<UserRoomvisit> UserRoomvisits { get; set; }

    public virtual DbSet<UserRpBank> UserRpBanks { get; set; }

    public virtual DbSet<UserRpBankLog> UserRpBankLogs { get; set; }

    public virtual DbSet<UserRpSetting> UserRpSettings { get; set; }

    public virtual DbSet<UserRpItem> UserRpItems { get; set; }

    public virtual DbSet<UserRpSkill> UserRpSkills { get; set; }

    public virtual DbSet<UserRpStatistic> UserRpStatistics { get; set; }

    public virtual DbSet<UserRpWeapon> UserRpWeapons { get; set; }

    public virtual DbSet<UserRpWeaponSkin> UserRpWeaponSkins { get; set; }

    public virtual DbSet<UserSavedSearch> UserSavedSearches { get; set; }

    public virtual DbSet<UserStat> UserStats { get; set; }

    public virtual DbSet<UserVoucher> UserVouchers { get; set; }

    public virtual DbSet<UserWardrobe> UserWardrobes { get; set; }

    public virtual DbSet<UsersMacro> UsersMacros { get; set; }

    public virtual DbSet<UsersMacroSetting> UsersMacroSettings { get; set; }

    public virtual DbSet<WiredItem> WiredItems { get; set; }

    public virtual DbSet<Wordfilter> Wordfilters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Category).HasDefaultValueSql("'''identity'''");
            entity.Property(e => e.GroupName).HasDefaultValueSql("'''ACH_'''");
            entity.Property(e => e.Level).HasDefaultValueSql("'1'");
            entity.Property(e => e.ProgressNeeded).HasDefaultValueSql("'1'");
            entity.Property(e => e.RewardAmount).HasDefaultValueSql("'5'");
            entity.Property(e => e.PointsType).HasDefaultValueSql("'0'");
            entity.Property(e => e.RewardPoints).HasDefaultValueSql("'5'");
        });

        modelBuilder.Entity<AchievementsTalent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.AchievementGroup).HasDefaultValueSql("'''ACH_'''");
            entity.Property(e => e.AchievementLevel).HasDefaultValueSql("'1'");
            entity.Property(e => e.ParentCategory).HasDefaultValueSql("'-1'");
            entity.Property(e => e.Prize).HasDefaultValueSql("'''A1 KUMIANKKA'''");
            entity.Property(e => e.Type).HasDefaultValueSql("'''citizenship'''");
        });

        modelBuilder.Entity<BadgeDefinition>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PRIMARY");

            entity.Property(e => e.RequiredRight).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<Ban>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.AppealState).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.Bantype).HasDefaultValueSql("'''user'''");
        });

        modelBuilder.Entity<Bot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.AiType).HasDefaultValueSql("'''generic'''");
            entity.Property(e => e.AutomaticChat).HasDefaultValueSql("'''false'''");
            entity.Property(e => e.ChatBubble).HasDefaultValueSql("'2'");
            entity.Property(e => e.Gender).HasDefaultValueSql("'''M'''");
            entity.Property(e => e.MixSentences).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.SpeakingInterval).HasDefaultValueSql("'30'");
            entity.Property(e => e.WalkMode).HasDefaultValueSql("'''freeroam'''");
        });

        modelBuilder.Entity<BotsPetCommand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Input).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<BotsPetResponse>(entity =>
        {
            entity.HasKey(e => e.PetId).HasName("PRIMARY");
        });

        modelBuilder.Entity<BotsPetdatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.AnyoneRide).HasDefaultValueSql("'0'");
            entity.Property(e => e.Color).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Createstamp).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Energy).HasDefaultValueSql("'0'");
            entity.Property(e => e.Experience).HasDefaultValueSql("'0'");
            entity.Property(e => e.GnomeClothing).HasDefaultValueSql("'''-1'''");
            entity.Property(e => e.Hairdye).HasDefaultValueSql("'1'");
            entity.Property(e => e.HaveSaddle).HasDefaultValueSql("'0'");
            entity.Property(e => e.Nutrition).HasDefaultValueSql("'0'");
            entity.Property(e => e.Pethair).HasDefaultValueSql("'-1'");
            entity.Property(e => e.Race).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Respect).HasDefaultValueSql("'0'");
            entity.Property(e => e.Type).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<BotsResponse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.BotAi).HasDefaultValueSql("'''generic'''");
            entity.Property(e => e.ResponseBeverage).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.ResponseMode).HasDefaultValueSql("'''say'''");
            entity.Property(e => e.ResponseText).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<BotsSpeech>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Shout).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.Type).HasDefaultValueSql("'''normal'''");
        });

        modelBuilder.Entity<CatalogBotPreset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.AiType).HasDefaultValueSql("'''generic'''");
        });

        modelBuilder.Entity<CatalogClothing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ClothingName).HasDefaultValueSql("''''''");
            entity.Property(e => e.ClothingParts).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<CatalogDeal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<CatalogItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Amount).HasDefaultValueSql("'1'");
            entity.Property(e => e.Badge).HasDefaultValueSql("''''''");
            entity.Property(e => e.CostCredits).HasDefaultValueSql("'3'");
            entity.Property(e => e.Extradata).HasDefaultValueSql("''''''");
            entity.Property(e => e.OfferActive).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.OfferId).HasDefaultValueSql("'-1'");
            entity.Property(e => e.PointsType).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<CatalogMarketplaceDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<CatalogMarketplaceOffer>(entity =>
        {
            entity.HasKey(e => e.OfferId).HasName("PRIMARY");

            entity.Property(e => e.ItemType).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.State).HasDefaultValueSql("'''1'''");
        });

        modelBuilder.Entity<CatalogPage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Caption).HasDefaultValueSql("'''noname'''");
            entity.Property(e => e.Enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.IconImage).HasDefaultValueSql("'1'");
            entity.Property(e => e.MinRank).HasDefaultValueSql("'1'");
            entity.Property(e => e.OrderNum).HasDefaultValueSql("'69'");
            entity.Property(e => e.PageLayout).HasDefaultValueSql("'''default_3x3'''");
            entity.Property(e => e.PageLink).HasDefaultValueSql("''''''");
            entity.Property(e => e.PageStrings1).HasDefaultValueSql("''''''");
            entity.Property(e => e.PageStrings2).HasDefaultValueSql("''''''");
            entity.Property(e => e.ParentId).HasDefaultValueSql("'-1'");
            entity.Property(e => e.Visible).HasDefaultValueSql("'''1'''");
        });

        modelBuilder.Entity<CatalogPetRace>(entity =>
        {
            entity.Property(e => e.Color1).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Color2).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Has1color).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Has2color).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Raceid).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<CatalogPromotion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Image).HasDefaultValueSql("''''''");
            entity.Property(e => e.PageLink).HasDefaultValueSql("''''''");
            entity.Property(e => e.ParentId).HasDefaultValueSql("'0'");
            entity.Property(e => e.Title).HasDefaultValueSql("''''''");
            entity.Property(e => e.Unknown).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<CatalogVoucher>(entity =>
        {
            entity.HasKey(e => e.Voucher).HasName("PRIMARY");

            entity.Property(e => e.Enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.MaxUses).HasDefaultValueSql("'1'");
            entity.Property(e => e.Type).HasDefaultValueSql("'''credits'''");
            entity.Property(e => e.Value).HasDefaultValueSql("'100'");
        });

        modelBuilder.Entity<Chatlog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<ChatlogsConsole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<ChatlogsConsoleInvitation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<ClientExternalBadgeText>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.BadgeCode).HasDefaultValueSql("''''''");
            entity.Property(e => e.BadgeDesc).HasDefaultValueSql("''''''");
            entity.Property(e => e.BadgeTitle).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<ClientExternalText>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<Furniture>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.AllowGift).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.AllowInventoryStack).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.AllowLay).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.AllowMarketplaceSell).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.AllowRecycle).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.AllowTrade).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.CanStack).HasDefaultValueSql("'1'");
            entity.Property(e => e.ExtraRot).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.HeightAdjustable).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.InteractionModesCount).HasDefaultValueSql("'1'");
            entity.Property(e => e.InteractionType).HasDefaultValueSql("'''default'''");
            entity.Property(e => e.IsRare).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.IsWalkable).HasDefaultValueSql("'0'");
            entity.Property(e => e.Length).HasDefaultValueSql("'1'");
            entity.Property(e => e.PublicName).HasDefaultValueSql("''''''");
            entity.Property(e => e.SpriteId).HasDefaultValueSql("'0'");
            entity.Property(e => e.Type).HasDefaultValueSql("'''s'''");
            entity.Property(e => e.VendingIds).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.Width).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<FurnitureCopy1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.AllowGift).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.AllowInventoryStack).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.AllowMarketplaceSell).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.AllowRecycle).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.AllowTrade).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.CanSit).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.CanStack).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.ExtraRot).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.HeightAdjustable).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.InteractionModesCount).HasDefaultValueSql("'1'");
            entity.Property(e => e.InteractionType).HasDefaultValueSql("'''default'''");
            entity.Property(e => e.IsRare).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.IsWalkable).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.Length).HasDefaultValueSql("'1'");
            entity.Property(e => e.PublicName).HasDefaultValueSql("''''''");
            entity.Property(e => e.Type).HasDefaultValueSql("'''s'''");
            entity.Property(e => e.VendingIds).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.Width).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<GamesConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ColourOne).HasDefaultValueSql("''''''");
            entity.Property(e => e.ColourTwo).HasDefaultValueSql("''''''");
            entity.Property(e => e.GameAssets).HasDefaultValueSql("''''''");
            entity.Property(e => e.GameEnabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.GameServerHost).HasDefaultValueSql("''''''");
            entity.Property(e => e.GameServerPort).HasDefaultValueSql("''''''");
            entity.Property(e => e.GameSwf).HasDefaultValueSql("''''''");
            entity.Property(e => e.LastReset).HasDefaultValueSql("'0'");
            entity.Property(e => e.Name).HasDefaultValueSql("''''''");
            entity.Property(e => e.SocketPolicyPort).HasDefaultValueSql("''''''");
            entity.Property(e => e.StringThree).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Admindeco).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.Colour1).HasDefaultValueSql("'242424'");
            entity.Property(e => e.Colour2).HasDefaultValueSql("'242424'");
            entity.Property(e => e.ForumEnabled).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.State).HasDefaultValueSql("'''0'''");
        });

        modelBuilder.Entity<GroupMembership>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Level).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<GroupPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.GroupId).HasComment("groups.id");
            entity.Property(e => e.LevelId).HasComment("groups_roles.level");
            entity.Property(e => e.PermissionId).HasComment("group_permissiosn_keys.id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<GroupPermissionKey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Description).HasDefaultValueSql("'''nodesc'''");
        });

        modelBuilder.Entity<GroupRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("'''0000-00-00 00:00:00'''");
            entity.Property(e => e.GroupId).HasComment("Group's ID.");
            entity.Property(e => e.Level)
                .HasDefaultValueSql("'1'")
                .HasComment("Levels go in order of Slave -> CEO. 1 = Slave, 100 = CEO.");
            entity.Property(e => e.Name)
                .HasDefaultValueSql("''''''")
                .HasComment("Literal name displayed; ex. general manager");
            entity.Property(e => e.ShiftCostume)
                .HasDefaultValueSql("''''''")
                .HasComment("Shift-Costume (full) to replace users own clothes. Blank for no swap.");
            entity.Property(e => e.ShiftDuration)
                .HasDefaultValueSql("'5'")
                .HasComment("The duration at which worker will be auto-rewarded. In minutes.");
            entity.Property(e => e.ShiftMotto)
                .HasDefaultValueSql("''''''")
                .HasComment("Replace users motto for the duration of shift? Blank for no swap.");
            entity.Property(e => e.ShiftPay).HasComment("Credits paid via intervals.");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<GroupsItem>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.Type }).HasName("PRIMARY");

            entity.Property(e => e.Enabled).HasDefaultValueSql("'''1'''");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.LimitedNumber).HasDefaultValueSql("'0'");
            entity.Property(e => e.LimitedStack).HasDefaultValueSql("'0'");
            entity.Property(e => e.WallPos).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<ItemsGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<ItemsYoutube>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Description).HasDefaultValueSql("''''''");
            entity.Property(e => e.Enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.Title).HasDefaultValueSql("''''''");
            entity.Property(e => e.YoutubeId).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<LogsClientNamechange>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.NewName).HasDefaultValueSql("''''''");
            entity.Property(e => e.OldName).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<LogsClientStaff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.MachineId).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<LogsClientTrade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e._1id).HasDefaultValueSql("'0'");
            entity.Property(e => e._1items).HasDefaultValueSql("'NULL'");
            entity.Property(e => e._2id).HasDefaultValueSql("'0'");
            entity.Property(e => e._2items).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("''''''")
                .IsFixedLength();
        });

        modelBuilder.Entity<MessengerFriendship>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<MessengerOfflineMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<MessengerRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<ModerationPreset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Enabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.Type).HasDefaultValueSql("'''user'''");
        });

        modelBuilder.Entity<ModerationPresetActionCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Caption).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<ModerationPresetActionMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.BanHours).HasDefaultValueSql("'24'");
        });

        modelBuilder.Entity<ModerationTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Status).HasDefaultValueSql("'''open'''");
        });

        modelBuilder.Entity<ModerationTopic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Caption).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<ModerationTopicAction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Caption).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<NavigatorCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Category).HasDefaultValueSql("'''hotel_view'''");
            entity.Property(e => e.CategoryIdentifier).HasDefaultValueSql("''''''");
            entity.Property(e => e.CategoryType).HasDefaultValueSql("'''category'''");
            entity.Property(e => e.Enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.PublicName).HasDefaultValueSql("''''''");
            entity.Property(e => e.RequiredRank).HasDefaultValueSql("'1'");
            entity.Property(e => e.SearchAllowance).HasDefaultValueSql("'''SHOW_MORE'''");
            entity.Property(e => e.ViewMode).HasDefaultValueSql("'''REGULAR'''");
        });

        modelBuilder.Entity<NavigatorPublic>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PRIMARY");

            entity.Property(e => e.Enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.OrderNum).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<PermissionsCommand>(entity =>
        {
            entity.HasKey(e => e.Command).HasName("PRIMARY");

            entity.Property(e => e.Command).HasDefaultValueSql("''''''");
            entity.Property(e => e.GroupId).HasDefaultValueSql("'4'");
        });

        modelBuilder.Entity<PermissionsGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.BadgeCode).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<PermissionsRight>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<PermissionsSubscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<Quest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Action).HasDefaultValueSql("''''''");
            entity.Property(e => e.DataBit).HasDefaultValueSql("''''''");
            entity.Property(e => e.PixelReward).HasDefaultValueSql("'10'");
            entity.Property(e => e.RewardType).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.Type).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<Rank>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.AllowHidewall).HasDefaultValueSql("'1'");
            entity.Property(e => e.BanSettings).HasDefaultValueSql("'1'");
            entity.Property(e => e.Caption).HasDefaultValueSql("'''Room'''");
            entity.Property(e => e.ChatHearingDistance).HasDefaultValueSql("'14'");
            entity.Property(e => e.Description).HasDefaultValueSql("''''''");
            entity.Property(e => e.EnablesEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.Floor).HasDefaultValueSql("'''0.0'''");
            entity.Property(e => e.KickSettings).HasDefaultValueSql("'1'");
            entity.Property(e => e.Landscape).HasDefaultValueSql("'''0.0'''");
            entity.Property(e => e.LayEnabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.MuteSettings).HasDefaultValueSql("'1'");
            entity.Property(e => e.Owner).HasDefaultValueSql("''''''");
            entity.Property(e => e.Password).HasDefaultValueSql("''''''");
            entity.Property(e => e.PetMorphsAllowed).HasDefaultValueSql("'1'");
            entity.Property(e => e.PullEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.PushEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.RespectNotificationsEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.Roomtype).HasDefaultValueSql("'''private'''");
            entity.Property(e => e.Safezone).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.SpullEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.SpushEnabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.State).HasDefaultValueSql("'''open'''");
            entity.Property(e => e.Tags).HasDefaultValueSql("''''''");
            entity.Property(e => e.TaxiX).HasDefaultValueSql("'-1'");
            entity.Property(e => e.TaxiY).HasDefaultValueSql("'-1'");
            entity.Property(e => e.TradeSettings).HasDefaultValueSql("'2'");
            entity.Property(e => e.UsersMax).HasDefaultValueSql("'25'");
            entity.Property(e => e.Wallpaper).HasDefaultValueSql("'''0.0'''");
        });

        modelBuilder.Entity<RoomBan>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoomId }).HasName("PRIMARY");
        });

        modelBuilder.Entity<RoomChatStyle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Name).HasDefaultValueSql("''''''");
            entity.Property(e => e.RequiredRight).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<RoomFilter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Word).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<RoomItemsMoodlight>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Enabled).HasDefaultValueSql("'''0'''");
        });

        modelBuilder.Entity<RoomItemsTeleLink>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<RoomItemsToner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Enabled).HasDefaultValueSql("'''0'''");
        });

        modelBuilder.Entity<RoomModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ClubOnly).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.Custom).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.DoorDir).HasDefaultValueSql("'2'");
            entity.Property(e => e.Poolmap).HasDefaultValueSql("''''''");
            entity.Property(e => e.PublicItems).HasDefaultValueSql("''''''");
            entity.Property(e => e.WallHeight).HasDefaultValueSql("'-1'");
        });

        modelBuilder.Entity<RoomPromotion>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PRIMARY");

            entity.Property(e => e.Description).HasDefaultValueSql("''''''");
            entity.Property(e => e.Title).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<RoomRight>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<RoomTaxiPoint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<RpCrime>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Note).HasDefaultValueSql("''''''");
            entity.Property(e => e.Severity).HasDefaultValueSql("'1'");
            entity.Property(e => e.TimeActive).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<RpCrimeLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<RpCrimePenalty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<RpItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ImageUrl).HasDefaultValueSql("''''''");
            entity.Property(e => e.ItemType).HasDefaultValueSql("'''consumable'''");
            entity.Property(e => e.StackLimit).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<RpShiftLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<RpSkill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.BadgeCode).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Description).HasDefaultValueSql("'''No description yet.'''");
            entity.Property(e => e.Name).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<RpSkillsLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Level).HasDefaultValueSql("'''1'''");
        });

        modelBuilder.Entity<RpTrashReward>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ChanceWeight).HasDefaultValueSql("'1'");
            entity.Property(e => e.Enabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.MaxAmount).HasDefaultValueSql("'1'");
            entity.Property(e => e.MinAmount).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<RpWeapon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.CriticalChance)
                .HasDefaultValueSql("'10'")
                .HasComment("As %. For instance 50% is 50-50 on hitting it, without Skill boosts.");
            entity.Property(e => e.CriticalHitMessage).HasDefaultValueSql("'''*lands a critical hit on %username%, dealing %damage% damage*'''");
            entity.Property(e => e.Description).HasDefaultValueSql("''''''");
            entity.Property(e => e.HitMessage)
                .HasDefaultValueSql("'''*swings at %username%, dealing %damage% damage*'''")
                .HasComment("Variables available username and damage");
            entity.Property(e => e.Image).HasDefaultValueSql("''''''");
            entity.Property(e => e.MaximumDamage).HasDefaultValueSql("'1'");
            entity.Property(e => e.Name).HasDefaultValueSql("''''''");
            entity.Property(e => e.StunChance).HasComment("0-100");
        });

        modelBuilder.Entity<RpWeaponSkin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.CriticalHitMessage).HasDefaultValueSql("'''*lands a critical hit on %username%, dealing %damage% damage*'''");
            entity.Property(e => e.HitMessage).HasDefaultValueSql("'''*swings at %username%, dealing %damage% damage*'''");
        });

        modelBuilder.Entity<ServerLanding>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ButtonLink).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.ButtonText).HasDefaultValueSql("''''''");
            entity.Property(e => e.ButtonType).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.ImageLink).HasDefaultValueSql("''''''");
            entity.Property(e => e.Text).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Title).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<ServerLocale>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("PRIMARY");

            entity.Property(e => e.Value).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<ServerReward>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.RewardType).HasDefaultValueSql("'''none'''");
        });

        modelBuilder.Entity<ServerRewardLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<ServerSetting>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("PRIMARY");

            entity.Property(e => e.Key).HasDefaultValueSql("'''server.variable'''");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Credits).HasDefaultValueSql("'100'");
            entity.Property(e => e.Duckets).HasDefaultValueSql("'100'");
            entity.Property(e => e.Respects).HasDefaultValueSql("'3'");
            entity.Property(e => e.StaticAggression).HasDefaultValueSql("'3'");
            entity.Property(e => e.TrashSearchCooldown).HasDefaultValueSql("'60'");
        });

        modelBuilder.Entity<Talent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Level).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<TalentsSubLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.BadgeCode).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.AuthTicket }).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.AccountCreated)
                .HasDefaultValueSql("'''0'''")
                .IsFixedLength();
            entity.Property(e => e.ActivityPoints).HasDefaultValueSql("'5000'");
            entity.Property(e => e.AdvertisingReportBlocked).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.AllowGifts).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.AllowMimic).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.BlockNewfriends).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.BotsMuted).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.ChatPreference).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.Credits).HasDefaultValueSql("'50000'");
            entity.Property(e => e.DisableForcedEffects).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.FocusPreference).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.FriendBarState).HasDefaultValueSql("'1'");
            entity.Property(e => e.Gender).HasDefaultValueSql("'''M'''");
            entity.Property(e => e.GotwPoints).HasDefaultValueSql("'0'");
            entity.Property(e => e.HideInroom).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.HideOnline).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.HomeRoom).HasDefaultValueSql("'0'");
            entity.Property(e => e.HomeRoomData).HasDefaultValueSql("'''{\"roomid\":3,\"x\":18,\"y\":22,\"z\":0,\"rotation\":4,\"weapon\":0}'''");
            entity.Property(e => e.IgnoreInvites).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.IpLast).HasDefaultValueSql("''''''");
            entity.Property(e => e.IpReg).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.IsMuted).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.LastChange).HasDefaultValueSql("'0'");
            entity.Property(e => e.LastOnline).HasDefaultValueSql("'0'");
            entity.Property(e => e.Look)
                .HasDefaultValueSql("'NULL'")
                .IsFixedLength();
            entity.Property(e => e.MachineId).HasDefaultValueSql("''''''");
            entity.Property(e => e.Mail).HasDefaultValueSql("'''defaultuser@meth0d.org'''");
            entity.Property(e => e.Motto)
                .HasDefaultValueSql("'NULL'")
                .IsFixedLength();
            entity.Property(e => e.Online).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.Password).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.PetsMuted).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.Rank).HasDefaultValueSql("'1'");
            entity.Property(e => e.RankVip).HasDefaultValueSql("'1'");
            entity.Property(e => e.TimeMuted).HasDefaultValueSql("'0'");
            entity.Property(e => e.TradingLocked).HasDefaultValueSql("'0'");
            entity.Property(e => e.Vip).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.VipPoints).HasDefaultValueSql("'0'");
            entity.Property(e => e.Volume).HasDefaultValueSql("'''100,100,100'''");
        });

        modelBuilder.Entity<UserAchievement>(entity =>
        {
            entity.HasKey(e => new { e.Userid, e.Group }).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserBadge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserChatBubble>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("'current_timestamp()'");

            entity.HasOne(d => d.User).WithMany(p => p.UserChatBubbles)
                .HasPrincipalKey(p => p.Id)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_user_chat_bubbles_user");
        });

        modelBuilder.Entity<UserClothing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Part).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<UserEffect>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ActivatedStamp).HasDefaultValueSql("'0'");
            entity.Property(e => e.EffectId).HasDefaultValueSql("'1'");
            entity.Property(e => e.IsActivated).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.Quantity).HasDefaultValueSql("'0'");
            entity.Property(e => e.TotalDuration).HasDefaultValueSql("'3600'");
            entity.Property(e => e.UserId).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserIgnore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserInfo>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserPresent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserQuest>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.QuestId }).HasName("PRIMARY");

            entity.Property(e => e.Progress).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<UserRelationship>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserRoomvisit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserRpBank>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.AccountOpened)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("'''0000-00-00 00:00:00.000000'''");
        });

        modelBuilder.Entity<UserRpSetting>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.Property(e => e.FurnitureTradingEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.VoiceChatEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.LivefeedEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.AlertsEnabled).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<UserRpBankLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ActionType).HasDefaultValueSql("'''DEPOSIT'''");
            entity.Property(e => e.ManagementType).HasDefaultValueSql("'''ATM'''");
            entity.Property(e => e.Timestamp)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("'''0000-00-00 00:00:00.000000'''");
        });

        modelBuilder.Entity<UserRpItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.DurabilityLeft).HasDefaultValueSql("'-1'");
            entity.Property(e => e.Slot).HasDefaultValueSql("'-1'");
        });

        modelBuilder.Entity<UserRpSkill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserRpStatistic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Energy).HasDefaultValueSql("'100'");
            entity.Property(e => e.Health).HasDefaultValueSql("'100'");
            entity.Property(e => e.Hunger).HasDefaultValueSql("'25'");
            entity.Property(e => e.JailRevertLook).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<UserRpWeapon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Slot).HasDefaultValueSql("'-1'");
        });

        modelBuilder.Entity<UserRpWeaponSkin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserSavedSearch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Filter).HasDefaultValueSql("''''''");
            entity.Property(e => e.SearchCode).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<UserStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.DailyPetRespectPoints).HasDefaultValueSql("'3'");
            entity.Property(e => e.DailyRespectPoints).HasDefaultValueSql("'3'");
            entity.Property(e => e.RespectsTimestamp).HasDefaultValueSql("'''10/19'''");
        });

        modelBuilder.Entity<UserVoucher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserWardrobe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Gender).HasDefaultValueSql("'''M'''");
        });

        modelBuilder.Entity<UsersMacro>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Configs).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<UsersMacroSetting>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");
        });

        modelBuilder.Entity<WiredItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e._bool).HasDefaultValueSql("'''0'''");
        });

        modelBuilder.Entity<Wordfilter>(entity =>
        {
            entity.HasKey(e => e.Word).HasName("PRIMARY");

            entity.Property(e => e.Addedby).HasDefaultValueSql("''''''");
            entity.Property(e => e.Bannable).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.Replacement).HasDefaultValueSql("'''Habboon'''");
            entity.Property(e => e.Strict).HasDefaultValueSql("'''1'''");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
