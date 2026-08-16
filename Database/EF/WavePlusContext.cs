using Microsoft.EntityFrameworkCore;
using Plus.Database.EF.Entities;

namespace Plus.Database.EF;

public partial class WavePlusContext(DbContextOptions<WavePlusContext> options) : DbContext(options)
{
    public virtual DbSet<AchievementEntity> Achievements { get; set; }

    public virtual DbSet<AchievementsTalentEntity> AchievementsTalents { get; set; }

    public virtual DbSet<BadgeDefinitionEntity> BadgeDefinitions { get; set; }

    public virtual DbSet<BanEntity> Bans { get; set; }

    public virtual DbSet<BotEntity> Bots { get; set; }

    public virtual DbSet<BotsPetCommandEntity> BotsPetCommands { get; set; }

    public virtual DbSet<BotsPetResponseEntity> BotsPetResponses { get; set; }

    public virtual DbSet<BotsPetdatumEntity> BotsPetdata { get; set; }

    public virtual DbSet<BotsResponseEntity> BotsResponses { get; set; }

    public virtual DbSet<BotsSpeechEntity> BotsSpeeches { get; set; }

    public virtual DbSet<CatalogBotPresetEntity> CatalogBotPresets { get; set; }

    public virtual DbSet<CatalogClothingEntity> CatalogClothings { get; set; }

    public virtual DbSet<CatalogDealEntity> CatalogDeals { get; set; }

    public virtual DbSet<CatalogItemEntity> CatalogItems { get; set; }

    public virtual DbSet<CatalogMarketplaceDatumEntity> CatalogMarketplaceData { get; set; }

    public virtual DbSet<CatalogMarketplaceOfferEntity> CatalogMarketplaceOffers { get; set; }

    public virtual DbSet<CatalogPageEntity> CatalogPages { get; set; }

    public virtual DbSet<CatalogPetRaceEntity> CatalogPetRaces { get; set; }

    public virtual DbSet<CatalogPromotionEntity> CatalogPromotions { get; set; }

    public virtual DbSet<CatalogVoucherEntity> CatalogVouchers { get; set; }

    public virtual DbSet<CameraPhotoEntity> CameraPhotos { get; set; }

    public virtual DbSet<CameraWebEntity> CameraWeb { get; set; }

    public virtual DbSet<ChatlogEntity> Chatlogs { get; set; }

    public virtual DbSet<ChatlogsConsoleEntity> ChatlogsConsoles { get; set; }

    public virtual DbSet<ChatlogsConsoleInvitationEntity> ChatlogsConsoleInvitations { get; set; }

    public virtual DbSet<ClientExternalBadgeTextEntity> ClientExternalBadgeTexts { get; set; }

    public virtual DbSet<ClientExternalTextEntity> ClientExternalTexts { get; set; }

    public virtual DbSet<FurnitureEntity> Furnitures { get; set; }

    public virtual DbSet<FurnitureCopy1Entity> FurnitureCopy1s { get; set; }

    public virtual DbSet<GamesConfigEntity> GamesConfigs { get; set; }

    public virtual DbSet<GroupEntity> Groups { get; set; }

    public virtual DbSet<GroupMembershipEntity> GroupMemberships { get; set; }

    public virtual DbSet<GroupPermissionEntity> GroupPermissions { get; set; }

    public virtual DbSet<GroupPermissionKeyEntity> GroupPermissionKeys { get; set; }

    public virtual DbSet<GroupRequestEntity> GroupRequests { get; set; }

    public virtual DbSet<GroupRoleEntity> GroupRoles { get; set; }

    public virtual DbSet<GroupsItemEntity> GroupsItems { get; set; }

    public virtual DbSet<ItemEntity> Items { get; set; }

    public virtual DbSet<ItemsGroupEntity> ItemsGroups { get; set; }

    public virtual DbSet<ItemsYoutubeEntity> ItemsYoutubes { get; set; }

    public virtual DbSet<LogsClientNamechangeEntity> LogsClientNamechanges { get; set; }

    public virtual DbSet<LogsClientStaffEntity> LogsClientStaffs { get; set; }

    public virtual DbSet<LogsClientTradeEntity> LogsClientTrades { get; set; }

    public virtual DbSet<MessengerFriendshipEntity> MessengerFriendships { get; set; }

    public virtual DbSet<MessengerOfflineMessageEntity> MessengerOfflineMessages { get; set; }

    public virtual DbSet<MessengerRequestEntity> MessengerRequests { get; set; }

    public virtual DbSet<ModerationPresetEntity> ModerationPresets { get; set; }

    public virtual DbSet<ModerationPresetActionCategoryEntity> ModerationPresetActionCategories { get; set; }

    public virtual DbSet<ModerationPresetActionMessageEntity> ModerationPresetActionMessages { get; set; }

    public virtual DbSet<ModerationTicketEntity> ModerationTickets { get; set; }

    public virtual DbSet<ModerationTopicEntity> ModerationTopics { get; set; }

    public virtual DbSet<ModerationTopicActionEntity> ModerationTopicActions { get; set; }

    public virtual DbSet<NavigatorCategoryEntity> NavigatorCategories { get; set; }

    public virtual DbSet<NavigatorPublicEntity> NavigatorPublics { get; set; }

    public virtual DbSet<PermissionEntity> Permissions { get; set; }

    public virtual DbSet<PermissionsCommandEntity> PermissionsCommands { get; set; }

    public virtual DbSet<PermissionsGroupEntity> PermissionsGroups { get; set; }

    public virtual DbSet<PermissionsRightEntity> PermissionsRights { get; set; }

    public virtual DbSet<PermissionsSubscriptionEntity> PermissionsSubscriptions { get; set; }

    public virtual DbSet<QuestEntity> Quests { get; set; }

    public virtual DbSet<RankEntity> Ranks { get; set; }

    public virtual DbSet<RoomEntity> Rooms { get; set; }

    public virtual DbSet<RoomBanEntity> RoomBans { get; set; }

    public virtual DbSet<RoomChatStyleEntity> RoomChatStyles { get; set; }

    public virtual DbSet<RoomFilterEntity> RoomFilters { get; set; }

    public virtual DbSet<RoomItemsMoodlightEntity> RoomItemsMoodlights { get; set; }

    public virtual DbSet<RoomItemsTeleLinkEntity> RoomItemsTeleLinks { get; set; }

    public virtual DbSet<RoomItemsTonerEntity> RoomItemsToners { get; set; }

    public virtual DbSet<RoomModelEntity> RoomModels { get; set; }

    public virtual DbSet<RoomPromotionEntity> RoomPromotions { get; set; }

    public virtual DbSet<RoomRightEntity> RoomRights { get; set; }

    public virtual DbSet<RpCrimeEntity> RpCrimes { get; set; }

    public virtual DbSet<RpCrimeLogEntity> RpCrimeLogs { get; set; }

    public virtual DbSet<RpCrimePenaltyEntity> RpCrimePenalties { get; set; }

    public virtual DbSet<RpItemEntity> RpItems { get; set; }

    public virtual DbSet<RpClothingSetEntity> RpClothingSets { get; set; }

    public virtual DbSet<RpClothingCategoryEntity> RpClothingCategories { get; set; }

    public virtual DbSet<RpShiftLogEntity> RpShiftLogs { get; set; }

    public virtual DbSet<RpSkillEntity> RpSkills { get; set; }

    public virtual DbSet<RpSkillsLevelEntity> RpSkillsLevels { get; set; }

    public virtual DbSet<RpTrashRewardEntity> RpTrashRewards { get; set; }

    public virtual DbSet<RpWeaponEntity> RpWeapons { get; set; }

    public virtual DbSet<RpWeaponSkinEntity> RpWeaponSkins { get; set; }

    public virtual DbSet<ServerLandingEntity> ServerLandings { get; set; }

    public virtual DbSet<ServerLocaleEntity> ServerLocales { get; set; }

    public virtual DbSet<ServerRewardEntity> ServerRewards { get; set; }

    public virtual DbSet<ServerRewardLogEntity> ServerRewardLogs { get; set; }

    public virtual DbSet<ServerSettingEntity> ServerSettings { get; set; }

    public virtual DbSet<ServerStatusEntity> ServerStatuses { get; set; }

    public virtual DbSet<SubscriptionEntity> Subscriptions { get; set; }

    public virtual DbSet<TalentEntity> Talents { get; set; }

    public virtual DbSet<TalentsSubLevelEntity> TalentsSubLevels { get; set; }

    public virtual DbSet<UserEntity> Users { get; set; }

    public virtual DbSet<UserAchievementEntity> UserAchievements { get; set; }

    public virtual DbSet<UserBadgeEntity> UserBadges { get; set; }

    public virtual DbSet<UserChatBubbleEntity> UserChatBubbles { get; set; }

    public virtual DbSet<UserClothingEntity> UserClothings { get; set; }

    public virtual DbSet<UserEffectEntity> UserEffects { get; set; }

    public virtual DbSet<UserFavoriteEntity> UserFavorites { get; set; }

    public virtual DbSet<UserIgnoreEntity> UserIgnores { get; set; }

    public virtual DbSet<UserInfoEntity> UserInfos { get; set; }

    public virtual DbSet<UserPresentEntity> UserPresents { get; set; }

    public virtual DbSet<UserQuestEntity> UserQuests { get; set; }

    public virtual DbSet<UserRelationshipEntity> UserRelationships { get; set; }

    public virtual DbSet<UserRoomvisitEntity> UserRoomvisits { get; set; }

    public virtual DbSet<UserRpBankEntity> UserRpBanks { get; set; }

    public virtual DbSet<UserRpBankLogEntity> UserRpBankLogs { get; set; }

    public virtual DbSet<UserRpSettingEntity> UserRpSettings { get; set; }

    public virtual DbSet<RpLinkWhitelistEntity> RpLinkWhitelist { get; set; }

    public virtual DbSet<UserRpItemEntity> UserRpItems { get; set; }

    public virtual DbSet<CraftingRecipeEntity> CraftingRecipes { get; set; }

    public virtual DbSet<CraftingRecipeIngredientEntity> CraftingRecipeIngredients { get; set; }

    public virtual DbSet<UserCraftingRecipeEntity> UserCraftingRecipes { get; set; }

    public virtual DbSet<GroupCraftingRecipeEntity> GroupCraftingRecipes { get; set; }

    public virtual DbSet<UserRpSkillEntity> UserRpSkills { get; set; }

    public virtual DbSet<UserRpStatisticEntity> UserRpStatistics { get; set; }

    public virtual DbSet<UserRpWeaponEntity> UserRpWeapons { get; set; }

    public virtual DbSet<UserRpWeaponSkinEntity> UserRpWeaponSkins { get; set; }

    public virtual DbSet<UserSavedSearchEntity> UserSavedSearches { get; set; }

    public virtual DbSet<UserStatEntity> UserStats { get; set; }

    public virtual DbSet<UserVoucherEntity> UserVouchers { get; set; }

    public virtual DbSet<UserWardrobeEntity> UserWardrobes { get; set; }

    public virtual DbSet<UsersMacroEntity> UsersMacros { get; set; }

    public virtual DbSet<UsersMacroSettingEntity> UsersMacroSettings { get; set; }

    public virtual DbSet<WiredItemEntity> WiredItems { get; set; }

    public virtual DbSet<WordfilterEntity> Wordfilters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CameraPhotoEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<CameraWebEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<AchievementEntity>(entity =>
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

        modelBuilder.Entity<AchievementsTalentEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.AchievementGroup).HasDefaultValueSql("'''ACH_'''");
            entity.Property(e => e.AchievementLevel).HasDefaultValueSql("'1'");
            entity.Property(e => e.ParentCategory).HasDefaultValueSql("'-1'");
            entity.Property(e => e.Prize).HasDefaultValueSql("'''A1 KUMIANKKA'''");
            entity.Property(e => e.Type).HasDefaultValueSql("'''citizenship'''");
        });

        modelBuilder.Entity<BadgeDefinitionEntity>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PRIMARY");

            entity.Property(e => e.RequiredRight).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<BanEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.UserId, e.Bantype }, "userid_bantype").IsUnique();

            entity.Property(e => e.AppealState).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.Bantype).HasDefaultValueSql("'''account'''");
        });

        modelBuilder.Entity<BotEntity>(entity =>
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

        modelBuilder.Entity<BotsPetCommandEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Input).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<BotsPetResponseEntity>(entity =>
        {
            entity.HasKey(e => e.PetId).HasName("PRIMARY");
        });

        modelBuilder.Entity<BotsPetdatumEntity>(entity =>
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

        modelBuilder.Entity<BotsResponseEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.BotAi).HasDefaultValueSql("'''generic'''");
            entity.Property(e => e.ResponseBeverage).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.ResponseMode).HasDefaultValueSql("'''say'''");
            entity.Property(e => e.ResponseText).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<BotsSpeechEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Shout).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.Type).HasDefaultValueSql("'''normal'''");
        });

        modelBuilder.Entity<CatalogBotPresetEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.AiType).HasDefaultValueSql("'''generic'''");
        });

        modelBuilder.Entity<CatalogClothingEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ClothingName).HasDefaultValueSql("''''''");
            entity.Property(e => e.ClothingParts).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<CatalogDealEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<CatalogItemEntity>(entity =>
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

        modelBuilder.Entity<CatalogMarketplaceDatumEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<CatalogMarketplaceOfferEntity>(entity =>
        {
            entity.HasKey(e => e.OfferId).HasName("PRIMARY");

            entity.Property(e => e.ItemType).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.State).HasDefaultValueSql("'''1'''");
        });

        modelBuilder.Entity<CatalogPageEntity>(entity =>
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

        modelBuilder.Entity<CatalogPetRaceEntity>(entity =>
        {
            entity.Property(e => e.Color1).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Color2).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Has1color).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Has2color).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Raceid).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<CatalogPromotionEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Image).HasDefaultValueSql("''''''");
            entity.Property(e => e.PageLink).HasDefaultValueSql("''''''");
            entity.Property(e => e.ParentId).HasDefaultValueSql("'0'");
            entity.Property(e => e.Title).HasDefaultValueSql("''''''");
            entity.Property(e => e.Unknown).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<CatalogVoucherEntity>(entity =>
        {
            entity.HasKey(e => e.Voucher).HasName("PRIMARY");

            entity.Property(e => e.Enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.MaxUses).HasDefaultValueSql("'1'");
            entity.Property(e => e.Type).HasDefaultValueSql("'''credits'''");
            entity.Property(e => e.Value).HasDefaultValueSql("'100'");
        });

        modelBuilder.Entity<ChatlogEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<ChatlogsConsoleEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<ChatlogsConsoleInvitationEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<ClientExternalBadgeTextEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.BadgeCode).HasDefaultValueSql("''''''");
            entity.Property(e => e.BadgeDesc).HasDefaultValueSql("''''''");
            entity.Property(e => e.BadgeTitle).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<ClientExternalTextEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<FurnitureEntity>(entity =>
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

        modelBuilder.Entity<FurnitureCopy1Entity>(entity =>
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

        modelBuilder.Entity<GamesConfigEntity>(entity =>
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

        modelBuilder.Entity<GroupEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Admindeco).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.Colour1).HasDefaultValueSql("'242424'");
            entity.Property(e => e.Colour2).HasDefaultValueSql("'242424'");
            entity.Property(e => e.ForumEnabled).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.State).HasDefaultValueSql("'''0'''");
        });

        modelBuilder.Entity<GroupMembershipEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Level).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<GroupPermissionEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.GroupId).HasComment("groups.id");
            entity.Property(e => e.LevelId).HasComment("groups_roles.level");
            entity.Property(e => e.PermissionId).HasComment("group_permissiosn_keys.id");
        });

        modelBuilder.Entity<GroupPermissionKeyEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Description).HasDefaultValueSql("'''nodesc'''");
        });

        modelBuilder.Entity<GroupRoleEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.GroupId).HasComment("GroupEntity's ID.");
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
        });

        modelBuilder.Entity<GroupsItemEntity>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.Type }).HasName("PRIMARY");

            entity.Property(e => e.Enabled).HasDefaultValueSql("'''1'''");
        });

        modelBuilder.Entity<ItemEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.LimitedNumber).HasDefaultValueSql("'0'");
            entity.Property(e => e.LimitedStack).HasDefaultValueSql("'0'");
            entity.Property(e => e.WallPos).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<ItemsGroupEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<ItemsYoutubeEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Description).HasDefaultValueSql("''''''");
            entity.Property(e => e.Enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.Title).HasDefaultValueSql("''''''");
            entity.Property(e => e.YoutubeId).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<LogsClientNamechangeEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.NewName).HasDefaultValueSql("''''''");
            entity.Property(e => e.OldName).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<LogsClientStaffEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.MachineId).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<LogsClientTradeEntity>(entity =>
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

        modelBuilder.Entity<MessengerFriendshipEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<MessengerOfflineMessageEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<MessengerRequestEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<ModerationPresetEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Enabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.Type).HasDefaultValueSql("'''user'''");
        });

        modelBuilder.Entity<ModerationPresetActionCategoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Caption).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<ModerationPresetActionMessageEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.BanHours).HasDefaultValueSql("'24'");
        });

        modelBuilder.Entity<ModerationTicketEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Status).HasDefaultValueSql("'''open'''");
        });

        modelBuilder.Entity<ModerationTopicEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Caption).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<ModerationTopicActionEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Caption).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<NavigatorCategoryEntity>(entity =>
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

        modelBuilder.Entity<NavigatorPublicEntity>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PRIMARY");

            entity.Property(e => e.Enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.OrderNum).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<PermissionEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<PermissionsCommandEntity>(entity =>
        {
            entity.HasKey(e => e.Command).HasName("PRIMARY");

            entity.Property(e => e.Command).HasDefaultValueSql("''''''");
            entity.Property(e => e.GroupId).HasDefaultValueSql("'4'");
        });

        modelBuilder.Entity<PermissionsGroupEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.BadgeCode).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<PermissionsRightEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<PermissionsSubscriptionEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<QuestEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Action).HasDefaultValueSql("''''''");
            entity.Property(e => e.DataBit).HasDefaultValueSql("''''''");
            entity.Property(e => e.PixelReward).HasDefaultValueSql("'10'");
            entity.Property(e => e.RewardType).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.Type).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<RankEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<RoomEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.AllowHidewall).HasDefaultValueSql("'0'");
            entity.Property(e => e.BanSettings).HasDefaultValueSql("'1'");
            entity.Property(e => e.Caption).HasDefaultValueSql("'''RoomEntity'''");
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
            entity.Property(e => e.Pathfinding3d).HasDefaultValueSql("'0'");
            entity.Property(e => e.PullEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.PushEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.RespectNotificationsEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.Roomtype).HasDefaultValueSql("'''private'''");
            entity.Property(e => e.Safezone).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.SpullEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.SpushEnabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.State).HasDefaultValueSql("'''open'''");
            entity.Property(e => e.Tags).HasDefaultValueSql("''''''");
            entity.Property(e => e.TradeSettings).HasDefaultValueSql("'2'");
            entity.Property(e => e.UsersMax).HasDefaultValueSql("'25'");
            entity.Property(e => e.Wallpaper).HasDefaultValueSql("'''0.0'''");
        });

        modelBuilder.Entity<RoomBanEntity>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoomId }).HasName("PRIMARY");
        });

        modelBuilder.Entity<RoomChatStyleEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Name).HasDefaultValueSql("''''''");
            entity.Property(e => e.RequiredRight).HasDefaultValueSql("''''''");
            entity.Property(e => e.Url).HasDefaultValueSql("''''''");
            entity.Property(e => e.FontColor).HasDefaultValueSql("'#000000'");
        });

        modelBuilder.Entity<RoomFilterEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Word).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<RoomItemsMoodlightEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Enabled).HasDefaultValueSql("'''0'''");
        });

        modelBuilder.Entity<RoomItemsTeleLinkEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<RoomItemsTonerEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Enabled).HasDefaultValueSql("'''0'''");
        });

        modelBuilder.Entity<RoomModelEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ClubOnly).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.Custom).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.DoorDir).HasDefaultValueSql("'2'");
            entity.Property(e => e.Poolmap).HasDefaultValueSql("''''''");
            entity.Property(e => e.PublicItems).HasDefaultValueSql("''''''");
            entity.Property(e => e.WallHeight).HasDefaultValueSql("'-1'");
        });

        modelBuilder.Entity<RoomPromotionEntity>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PRIMARY");

            entity.Property(e => e.Description).HasDefaultValueSql("''''''");
            entity.Property(e => e.Title).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<RoomRightEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<RpCrimeEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Note).HasDefaultValueSql("''''''");
            entity.Property(e => e.Severity).HasDefaultValueSql("'1'");
            entity.Property(e => e.TimeActive).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<RpCrimeLogEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<RpCrimePenaltyEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<RpItemEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ImageUrl).HasDefaultValueSql("''''''");
            entity.Property(e => e.ItemType).HasDefaultValueSql("'''consumable'''");
            entity.Property(e => e.StackLimit).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<RpClothingSetEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Stock).HasDefaultValueSql("'0'");
            entity.Property(e => e.BasePrice).HasDefaultValueSql("'0'");
            entity.Property(e => e.DiscountPrice).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<RpClothingCategoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.TabName).HasDefaultValueSql("'''Unnamed'''");
        });

        modelBuilder.Entity<RpShiftLogEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<CraftingRecipeEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.RewardType).HasDefaultValueSql("'''rp_item'''");
            entity.Property(e => e.RewardAmount).HasDefaultValueSql("'1'");
            entity.Property(e => e.Enabled).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<CraftingRecipeIngredientEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ItemType).HasDefaultValueSql("'''rp_item'''");
            entity.Property(e => e.Amount).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<UserCraftingRecipeEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<GroupCraftingRecipeEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<RpSkillEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.BadgeCode).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Description).HasDefaultValueSql("'''No description yet.'''");
            entity.Property(e => e.Name).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<RpSkillsLevelEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Level).HasDefaultValueSql("'''1'''");
        });

        modelBuilder.Entity<RpTrashRewardEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ChanceWeight).HasDefaultValueSql("'1'");
            entity.Property(e => e.Enabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.MaxAmount).HasDefaultValueSql("'1'");
            entity.Property(e => e.MinAmount).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<RpWeaponEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.AllowDiagonal).HasDefaultValueSql("'1'");
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

        modelBuilder.Entity<RpWeaponSkinEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.CriticalHitMessage).HasDefaultValueSql("'''*lands a critical hit on %username%, dealing %damage% damage*'''");
            entity.Property(e => e.HitMessage).HasDefaultValueSql("'''*swings at %username%, dealing %damage% damage*'''");
        });

        modelBuilder.Entity<ServerLandingEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ButtonLink).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.ButtonText).HasDefaultValueSql("''''''");
            entity.Property(e => e.ButtonType).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.ImageLink).HasDefaultValueSql("''''''");
            entity.Property(e => e.Text).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Title).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<ServerLocaleEntity>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("PRIMARY");

            entity.Property(e => e.Value).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<ServerRewardEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.RewardType).HasDefaultValueSql("'''none'''");
        });

        modelBuilder.Entity<ServerRewardLogEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<ServerSettingEntity>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("PRIMARY");

            entity.Property(e => e.Key).HasDefaultValueSql("'''server.variable'''");
        });

        modelBuilder.Entity<SubscriptionEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Credits).HasDefaultValueSql("'100'");
            entity.Property(e => e.Duckets).HasDefaultValueSql("'100'");
            entity.Property(e => e.Respects).HasDefaultValueSql("'3'");
            entity.Property(e => e.StaticAggression).HasDefaultValueSql("'3'");
            entity.Property(e => e.TrashSearchCooldown).HasDefaultValueSql("'60'");
        });

        modelBuilder.Entity<TalentEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Level).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<TalentsSubLevelEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.BadgeCode).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<UserEntity>(entity =>
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

        modelBuilder.Entity<UserAchievementEntity>(entity =>
        {
            entity.HasKey(e => new { e.Userid, e.Group }).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserBadgeEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserChatBubbleEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.User).WithMany(p => p.UserChatBubbles)
                .HasPrincipalKey(p => p.Id)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_user_chat_bubbles_user");
        });

        modelBuilder.Entity<UserClothingEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Part).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<UserEffectEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ActivatedStamp).HasDefaultValueSql("'0'");
            entity.Property(e => e.EffectId).HasDefaultValueSql("'1'");
            entity.Property(e => e.IsActivated).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.Quantity).HasDefaultValueSql("'0'");
            entity.Property(e => e.TotalDuration).HasDefaultValueSql("'3600'");
            entity.Property(e => e.UserId).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<UserFavoriteEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserIgnoreEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserInfoEntity>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserPresentEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserQuestEntity>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.QuestId }).HasName("PRIMARY");

            entity.Property(e => e.Progress).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<UserRelationshipEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserRoomvisitEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserRpBankEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserRpSettingEntity>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.Property(e => e.FurnitureTradingEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.VoiceChatEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.LivefeedEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.AlertsEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.LinkWarningEnabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.DragRoomsEnabled).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<RpLinkWhitelistEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.MatchType).HasDefaultValueSql("'domain'");
            entity.Property(e => e.Enabled).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<UserRpBankLogEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ActionType).HasDefaultValueSql("'''DEPOSIT'''");
            entity.Property(e => e.ManagementType).HasDefaultValueSql("'''ATM'''");
        });

        modelBuilder.Entity<UserRpItemEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.DurabilityLeft).HasDefaultValueSql("'-1'");
            entity.Property(e => e.Slot).HasDefaultValueSql("'-1'");
        });

        modelBuilder.Entity<UserRpSkillEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserRpStatisticEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Energy).HasDefaultValueSql("'100'");
            entity.Property(e => e.Health).HasDefaultValueSql("'100'");
            entity.Property(e => e.Hunger).HasDefaultValueSql("'25'");
            entity.Property(e => e.JailRevertLook).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<UserRpWeaponEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Slot).HasDefaultValueSql("'-1'");
        });

        modelBuilder.Entity<UserRpWeaponSkinEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserSavedSearchEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Filter).HasDefaultValueSql("''''''");
            entity.Property(e => e.SearchCode).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<UserStatEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.DailyPetRespectPoints).HasDefaultValueSql("'3'");
            entity.Property(e => e.DailyRespectPoints).HasDefaultValueSql("'3'");
            entity.Property(e => e.RespectsTimestamp).HasDefaultValueSql("'''10/19'''");
        });

        modelBuilder.Entity<UserVoucherEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<UserWardrobeEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Gender).HasDefaultValueSql("'''M'''");
        });

        modelBuilder.Entity<UsersMacroEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Configs).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<UsersMacroSettingEntity>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");
        });

        modelBuilder.Entity<WiredItemEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e._bool).HasDefaultValueSql("'''0'''");
        });

        modelBuilder.Entity<WordfilterEntity>(entity =>
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