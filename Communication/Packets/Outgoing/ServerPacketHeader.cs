namespace Plus.Communication.Packets.Outgoing
{
    public static class ServerPacketHeader
    {
        // Roleplay packets
        public const int PhoneBankMessageComposer = 6007;
        public const int AtmBankMessageComposer = 6006;
        public const int AnotherStatsMessageComposer = 6005;
        public const int UserStatsMessageComposer = 6004;
        public const int WantedListMessageComposer = 6003;
        public const int OfferItemMessageComposer = 6002;
        public const int TargetLockMessageComposer = 6000;

        // credit to Keorich##1440 for the headers for Production-201611291003-338511768
        public const int UserNftChatStylesMessageComposer = 6100;
        public const int AvatarEffectActivatedMessageComposer = 1959; // PRODUCTION-201611291003-338511768
        public const int HelperToolMessageComposer = 1548; // PRODUCTION-201611291003-338511768
        public const int NameChangeUpdateMessageComposer = 563; // PRODUCTION-201611291003-338511768
        public const int HeightMapMessageComposer = 2753; // PRODUCTION-201611291003-338511768
        public const int CallForHelpPendingCallsMessageComposer = 1121; // PRODUCTION-201611291003-338511768
        public const int ChatMessageComposer = 1446; // PRODUCTION-201611291003-338511768
        public const int GroupMembersMessageComposer = 1200; // PRODUCTION-201611291003-338511768
        public const int OpenBotActionMessageComposer = 1618; // PRODUCTION-201611291003-338511768
        public const int UserObjectMessageComposer = 2725; // PRODUCTION-201611291003-338511768
        public const int ActionMessageComposer = 1631; // PRODUCTION-201611291003-338511768
        public const int ManageGroupMessageComposer = 3965; // PRODUCTION-201611291003-338511768
        public const int FloodControlMessageComposer = 566; // PRODUCTION-201611291003-338511768
        public const int FlatControllerAddedMessageComposer = 2088; // PRODUCTION-201611291003-338511768
        public const int VoucherRedeemOkMessageComposer = 3336; // PRODUCTION-201611291003-338511768
        public const int TradingClosedMessageComposer = 1373; // PRODUCTION-201611291003-338511768
        public const int MarketplaceConfigurationMessageComposer = 1823; // PRODUCTION-201611291003-338511768
        public const int FlatCreatedMessageComposer = 1304; // PRODUCTION-201611291003-338511768
        public const int ServerErrorMessageComposer = 1004; // PRODUCTION-201611291003-338511768
        public const int ScrSendUserInfoMessageComposer = 954; // PRODUCTION-201611291003-338511768
        public const int RecyclerRewardsMessageComposer = 3164; // PRODUCTION-201611291003-338511768
        public const int CheckPetNameMessageComposer = 1503; // PRODUCTION-201611291003-338511768
        public const int GuideSessionAttachedMessageComposer = 1591; // PRODUCTION-201611291003-338511768
        public const int QuestAbortedMessageComposer = 3027; // PRODUCTION-201611291003-338511768
        public const int RespectPetNotificationMessageComposer = 2788; // changed structure - PRODUCTION-201611291003-338511768
        public const int PromotableRoomsMessageComposer = 2468; // PRODUCTION-201611291003-338511768
        public const int CameraPurchaseOkComposer = 2783; // PRODUCTION-201611291003-338511768
        public const int CloseConnectionMessageComposer = 122; // PRODUCTION-201611291003-338511768
        public const int HabboClubOffersMessageComposer = 2405; // PRODUCTION-201611291003-338511768
        public const int CfhTopicsInitMessageComposer = 325; // PRODUCTION-201611291003-338511768
        public const int WiredEffectConfigMessageComposer = 1434; // changed structure - PRODUCTION-201611291003-338511768
        public const int FriendListUpdateMessageComposer = 2800; // PRODUCTION-201611291003-338511768
        public const int ObjectAddMessageComposer = 1534; // PRODUCTION-201611291003-338511768
        public const int GuideSessionDetachedMessageComposer = 138; // PRODUCTION-201611291003-338511768
        public const int MarketplaceCanMakeOfferResultMessageComposer = 54; // PRODUCTION-201611291003-338511768
        public const int NavigatorCollapsedCategoriesMessageComposer = 1543; // PRODUCTION-201611291003-338511768
        public const int RoomRightsListMessageComposer = 1284; // PRODUCTION-201611291003-338511768
        public const int SetGroupIdMessageComposer = 1459; // PRODUCTION-201611291003-338511768
        public const int TradingUpdateMessageComposer = 2024; // PRODUCTION-201611291003-338511768
        public const int QuizDataMessageComposer = 2927; // PRODUCTION-201611291003-338511768
        public const int CarryObjectMessageComposer = 1474; // PRODUCTION-201611291003-338511768
        public const int NewGroupInfoMessageComposer = 2808; // PRODUCTION-201611291003-338511768
        public const int RoomForwardMessageComposer = 160; // PRODUCTION-201611291003-338511768
        public const int GroupFurniSettingsMessageComposer = 3293; // PRODUCTION-201611291003-338511768
        public const int CreditBalanceMessageComposer = 3475; // PRODUCTION-201611291003-338511768
        public const int GnomeBoxMessageComposer = 2380; // changed structure - PRODUCTION-201611291003-338511768
        public const int CatalogUpdatedMessageComposer = 1866; // PRODUCTION-201611291003-338511768
        public const int JoinQueueMessageComposer = 2260; // PRODUCTION-201611291003-338511768
        public const int UserTypingMessageComposer = 1717; // PRODUCTION-201611291003-338511768
        public const int ObjectRemoveMessageComposer = 2703; // PRODUCTION-201611291003-338511768
        public const int RoomEntryInfoMessageComposer = 749; // PRODUCTION-201611291003-338511768
        public const int CatalogOfferMessageComposer = 3388; // PRODUCTION-201611291003-338511768
        public const int CatalogIndexMessageComposer = 1032; // PRODUCTION-201611291003-338511768
        public const int ThreadsListDataMessageComposer = 1073; // PRODUCTION-201611291003-338511768
        public const int UserGameAchievementsMessageComposer = 2265; // PRODUCTION-201611291003-338511768
        public const int GroupFurniConfigMessageComposer = 420; // PRODUCTION-201611291003-338511768
        public const int HabboUserBadgesMessageComposer = 1087; // PRODUCTION-201611291003-338511768
        public const int FlatAccessibleMessageComposer = 3783; // PRODUCTION-201611291003-338511768
        public const int VoucherRedeemErrorMessageComposer = 714; // PRODUCTION-201611291003-338511768
        public const int GetRoomFilterListMessageComposer = 2937; // PRODUCTION-201611291003-338511768
        public const int YouAreNotControllerMessageComposer = 2392; // PRODUCTION-201611291003-338511768
        public const int ModeratorInitMessageComposer = 2696; // PRODUCTION-201611291003-338511768
        public const int ModeratorUserClassMessageComposer = 966; // PRODUCTION-201611291003-338511768
        public const int FloorPlanSendDoorMessageComposer = 1664; // PRODUCTION-201611291003-338511768
        public const int SleepMessageComposer = 1797; // PRODUCTION-201611291003-338511768
        public const int FlatControllerRemovedMessageComposer = 1327; // PRODUCTION-201611291003-338511768
        public const int UniqueMachineIdMessageComposer = 1488; // PRODUCTION-201611291003-338511768
        public const int ItemAddMessageComposer = 2187; // PRODUCTION-201611291003-338511768
        public const int FigureSetIdsMessageComposer = 1450; // PRODUCTION-201611291003-338511768
        public const int InstantMessageErrorMessageComposer = 3359; // PRODUCTION-201611291003-338511768
        public const int UpdateFreezeLivesMessageComposer = 2376; // PRODUCTION-201611291003-338511768
        public const int NavigatorSettingsMessageComposer = 2875; // PRODUCTION-201611291003-338511768
        public const int ItemUpdateMessageComposer = 2009; // PRODUCTION-201611291003-338511768
        public const int AchievementsMessageComposer = 305; // PRODUCTION-201611291003-338511768
        public const int PetBreedingMessageComposer = 634; // PRODUCTION-201611291003-338511768
        public const int LatencyResponseMessageComposer = 10; // PRODUCTION-201611291003-338511768
        public const int RoomReadyMessageComposer = 2031; // PRODUCTION-201611291003-338511768
        public const int HabboActivityPointNotificationMessageComposer = 2275; // PRODUCTION-201611291003-338511768
        public const int CheckGnomeNameMessageComposer = 546; // PRODUCTION-201611291003-338511768
        public const int BuddyListMessageComposer = 3130; // PRODUCTION-201611291003-338511768
        public const int UserTagsMessageComposer = 1255; // PRODUCTION-201611291003-338511768
        public const int MarketPlaceOwnOffersMessageComposer = 3884; // PRODUCTION-201611291003-338511768
        public const int GroupDeletedMessageComposer = 3129; // PRODUCTION-201611291003-338511768
        public const int YoutubeDisplayPlaylistsMessageComposer = 1112; // PRODUCTION-201611291003-338511768
        public const int UpdateStackHeightComposer = 558; // PRODUCTION-201611291003-338511768
        public const int AvatarEffectExpiredMessageComposer = 2228; // PRODUCTION-201611291003-338511768
        public const int TradingCompleteMessageComposer = 2720; // PRODUCTION-201611291003-338511768
        public const int PetInformationMessageComposer = 2901; // PRODUCTION-201611291003-338511768
        public const int ModeratorRoomChatlogMessageComposer = 3434; // PRODUCTION-201611291003-338511768
        public const int ClubGiftsMessageComposer = 619; // PRODUCTION-201611291003-338511768
        public const int MarketplaceMakeOfferResultMessageComposer = 1359; // PRODUCTION-201611291003-338511768
        public const int MotdNotificationMessageComposer = 2035; // PRODUCTION-201611291003-338511768
        public const int TalentTrackMessageComposer = 3406; // PRODUCTION-201611291003-338511768
        public const int MessengerErrorMessageComposer = 892; // PRODUCTION-201611291003-338511768
        public const int GroupInfoMessageComposer = 1702; // PRODUCTION-201611291003-338511768
        public const int SlideObjectBundleMessageComposer = 3207; // PRODUCTION-201611291003-338511768
        public const int FurniListRemoveMessageComposer = 159; // PRODUCTION-201611291003-338511768
        public const int FriendNotificationMessageComposer = 3082; // PRODUCTION-201611291003-338511768
        public const int FurniListNotificationMessageComposer = 2103; // PRODUCTION-201611291003-338511768
        public const int RoomInfoUpdatedMessageComposer = 3297; // PRODUCTION-201611291003-338511768
        public const int FurniListAddMessageComposer = 104; // PRODUCTION-201611291003-338511768
        public const int AvatarEffectMessageComposer = 1167; // PRODUCTION-201611291003-338511768
        public const int OpenConnectionMessageComposer = 758; // PRODUCTION-201611291003-338511768
        public const int FurniListMessageComposer = 994; // PRODUCTION-201611291003-338511768
        public const int FigureUpdateMessageComposer = 2429; // PRODUCTION-201611291003-338511768
        public const int PostUpdatedMessageComposer = 324; // PRODUCTION-201611291003-338511768
        public const int UserFlatCatsMessageComposer = 1562; // PRODUCTION-201611291003-338511768
        public const int GuideSessionPartnerIsTypingMessageComposer = 1016; // PRODUCTION-201611291003-338511768
        public const int ObjectUpdateMessageComposer = 3776; // PRODUCTION-201611291003-338511768
        public const int ThreadUpdatedMessageComposer = 2528; // PRODUCTION-201611291003-338511768
        public const int HabboSearchResultMessageComposer = 973; // PRODUCTION-201611291003-338511768
        public const int RespectNotificationMessageComposer = 2815; // PRODUCTION-201611291003-338511768
        public const int PetHorseFigureInformationMessageComposer = 1924; // changed structure - PRODUCTION-201611291003-338511768
        public const int MessengerInitMessageComposer = 1605; // PRODUCTION-201611291003-338511768
        public const int MarketplaceItemStatsMessageComposer = 725; // PRODUCTION-201611291003-338511768
        public const int ConcurrentUsersGoalProgressMessageComposer = 2737; // PRODUCTION-201611291003-338511768
        public const int ModeratorUserInfoMessageComposer = 2866; // PRODUCTION-201611291003-338511768
        public const int YouAreControllerMessageComposer = 780; // PRODUCTION-201611291003-338511768
        public const int RoomRatingMessageComposer = 482; // PRODUCTION-201611291003-338511768
        public const int RefreshFavouriteGroupMessageComposer = 876; // PRODUCTION-201611291003-338511768
        public const int AvailabilityStatusMessageComposer = 2033; // PRODUCTION-201611291003-338511768
        public const int PlayableGamesMessageComposer = 3805; // PRODUCTION-201611291003-338511768
        public const int AchievementUnlockedMessageComposer = 806; // PRODUCTION-201611291003-338511768
        public const int PostQuizAnswersMessageComposer = 2772; // PRODUCTION-201611291003-338511768
        public const int GroupMembershipRequestedMessageComposer = 1180; // PRODUCTION-201611291003-338511768
        public const int FlatAccessDeniedMessageComposer = 878; // PRODUCTION-201611291003-338511768
        public const int NavigatorFlatCatsMessageComposer = 3244; // PRODUCTION-201611291003-338511768
        public const int AvatarEffectAddedMessageComposer = 3315; // PRODUCTION-201611291003-338511768
        public const int UsersMessageComposer = 374; // PRODUCTION-201611291003-338511768
        public const int SecretKeyMessageComposer = 3885; // PRODUCTION-201611291003-338511768
        public const int TradingStartMessageComposer = 2505; // PRODUCTION-201611291003-338511768
        public const int RoomSettingsDataMessageComposer = 1498; // PRODUCTION-201611291003-338511768
        public const int NewBuddyRequestMessageComposer = 2219; // PRODUCTION-201611291003-338511768
        public const int DoorbellMessageComposer = 2309; // PRODUCTION-201611291003-338511768
        public const int OpenGiftMessageComposer = 56; // PRODUCTION-201611291003-338511768
        public const int NuxNotificationMessageComposer = 717; // PRODUCTION-201611291003-338511768
        public const int CantConnectMessageComposer = 899; // PRODUCTION-201611291003-338511768
        public const int GuideSessionRequesterRoomMessageComposer = 1847; // PRODUCTION-201611291003-338511768
        public const int FloorHeightMapMessageComposer = 1301; // PRODUCTION-201611291003-338511768
        public const int SubmitBullyReportMessageComposer = 2674; // PRODUCTION-201611291003-338511768
        public const int PresentDeliverErrorMessageComposer = 3914; // PRODUCTION-201611291003-338511768
        public const int SellablePetBreedsMessageComposer = 3331; // PRODUCTION-201611291003-338511768
        public const int MarketplaceCancelOfferResultMessageComposer = 3264; // PRODUCTION-201611291003-338511768
        public const int LoadGameMessageComposer = 3654; // PRODUCTION-201611291003-338511768
        public const int AchievementScoreMessageComposer = 1968; // PRODUCTION-201611291003-338511768
        public const int TalentLevelUpMessageComposer = 638; // PRODUCTION-201611291003-338511768
        public const int BuildersClubMembershipMessageComposer = 1452; // PRODUCTION-201611291003-338511768
        public const int PetTrainingPanelMessageComposer = 1164; // PRODUCTION-201611291003-338511768
        public const int QuestCompletedMessageComposer = 949; // PRODUCTION-201611291003-338511768
        public const int QuestionParserMessageComposer = 2665; // PRODUCTION-201611291003-338511768
        public const int UserRightsMessageComposer = 411; // PRODUCTION-201611291003-338511768
        public const int PongMessageComposer = 3928; // PRODUCTION-201611291003-338511768
        public const int UserChangeMessageComposer = 3920; // PRODUCTION-201611291003-338511768
        public const int ModeratorUserChatlogMessageComposer = 3377; // PRODUCTION-201611291003-338511768
        public const int CommunityGoalHallOfFameMessageComposer = 2515; // PRODUCTION-201611291003-338511768
        public const int GiftWrappingConfigurationMessageComposer = 2234; // PRODUCTION-201611291003-338511768
        public const int FloorPlanFloorMapMessageComposer = 3990; // PRODUCTION-201611291003-338511768
        public const int UserNameChangeMessageComposer = 2182; // PRODUCTION-201611291003-338511768
        public const int ThreadReplyMessageComposer = 2049; // PRODUCTION-201611291003-338511768
        public const int TalentTrackLevelMessageComposer = 1203; // PRODUCTION-201611291003-338511768
        public const int TradingConfirmedMessageComposer = 2568; // PRODUCTION-201611291003-338511768
        public const int GroupCreationWindowMessageComposer = 2159; // PRODUCTION-201611291003-338511768
        public const int GetGuestRoomResultMessageComposer = 687; // PRODUCTION-201611291003-338511768
        public const int RoomNotificationMessageComposer = 1992; // PRODUCTION-201611291003-338511768
        public const int InitCryptoMessageComposer = 1347; // PRODUCTION-201611291003-338511768
        public const int SoundSettingsMessageComposer = 513; // PRODUCTION-201611291003-338511768
        public const int WiredTriggerConfigMessageComposer = 383; // PRODUCTION-201611291003-338511768
        public const int ItemsMessageComposer = 1369; // PRODUCTION-201611291003-338511768
        public const int PurchaseOKMessageComposer = 869; // PRODUCTION-201611291003-338511768
        public const int BadgeEditorPartsMessageComposer = 2238; // PRODUCTION-201611291003-338511768
        public const int NewConsoleMessageMessageComposer = 1587; // PRODUCTION-201611291003-338511768
        public const int Game2WeeklyLeaderboardMessageComposer = 2196; // PRODUCTION-201611291003-338511768
        public const int HideWiredConfigMessageComposer = 1155; // PRODUCTION-201611291003-338511768
        public const int IgnoredUsersMessageComposer = 126; // PRODUCTION-201611291003-338511768
        public const int FollowFriendFailedMessageComposer = 3048; // PRODUCTION-201611291003-338511768
        public const int CatalogPageMessageComposer = 804; // PRODUCTION-201611291003-338511768
        public const int AddExperiencePointsMessageComposer = 2156; // PRODUCTION-201611291003-338511768
        public const int AvatarEffectsMessageComposer = 340; // PRODUCTION-201611291003-338511768
        public const int QuestListMessageComposer = 3625; // PRODUCTION-201611291003-338511768
        public const int UnbanUserFromRoomMessageComposer = 3429; // PRODUCTION-201611291003-338511768
        public const int WiredConditionConfigMessageComposer = 1108; // PRODUCTION-201611291003-338511768
        public const int GuideSessionInvitedToGuideRoomMessageComposer = 219; // PRODUCTION-201611291003-338511768
        public const int StickyNoteMessageComposer = 2202; // PRODUCTION-201611291003-338511768
        public const int SanctionStatusMessageComposer = 2221; // PRODUCTION-201611291003-338511768
        public const int ObjectsMessageComposer = 1778; // PRODUCTION-201611291003-338511768
        public const int NewUserExperienceGiftOfferMessageComposer = 91; // PRODUCTION-201611291003-338511768
        public const int RoomVisualizationSettingsMessageComposer = 3547; // PRODUCTION-201611291003-338511768
        public const int PromoArticlesMessageComposer = 286; // PRODUCTION-201611291003-338511768
        public const int MaintenanceStatusMessageComposer = 1350; // PRODUCTION-201611291003-338511768
        public const int BuddyRequestsMessageComposer = 280; // PRODUCTION-201611291003-338511768
        public const int CameraStorageUrlMessageComposer = 3696; // PRODUCTION-201611291003-338511768
        public const int Game3WeeklyLeaderboardMessageComposer = 2196; // PRODUCTION-201611291003-338511768
        public const int AuthenticationOkMessageComposer = 2491; // PRODUCTION-201611291003-338511768
        public const int QuestStartedMessageComposer = 230; // PRODUCTION-201611291003-338511768
        public const int BotInventoryMessageComposer = 3086; // PRODUCTION-201611291003-338511768
        public const int GameListMessageComposer = 222; // PRODUCTION-201611291003-338511768
        public const int SendBullyReportMessageComposer = 3463; // PRODUCTION-201611291003-338511768
        public const int VideoOffersRewardsMessageComposer = 2125; // PRODUCTION-201611291003-338511768
        public const int PerkAllowancesMessageComposer = 2586; // PRODUCTION-201611291003-338511768
        public const int RoomEventMessageComposer = 1840; // PRODUCTION-201611291003-338511768
        public const int MuteAllInRoomMessageComposer = 2533; // PRODUCTION-201611291003-338511768
        public const int Game1WeeklyLeaderboardMessageComposer = 2196; // PRODUCTION-201611291003-338511768
        public const int UpdateFavouriteRoomMessageComposer = 2524; // PRODUCTION-201611291003-338511768
        public const int ModeratorSupportTicketResponseMessageComposer = 934; // PRODUCTION-201611291003-338511768
        public const int YouTubeDisplayVideoMessageComposer = 1411; // PRODUCTION-201611291003-338511768
        public const int RoomPropertyMessageComposer = 2454; // PRODUCTION-201611291003-338511768
        public const int QuestionAnswersMessageComposer = 2589; // PRODUCTION-201611291003-338511768
        public const int ModeratorSupportTicketMessageComposer = 3609; // PRODUCTION-201611291003-338511768
        public const int SetCameraPriceMessageComposer = 3878; // PRODUCTION-201611291003-338511768
        public const int RoomInviteMessageComposer = 3870; // PRODUCTION-201611291003-338511768
        public const int FurniListUpdateMessageComposer = 3151; // PRODUCTION-201611291003-338511768
        public const int BadgesMessageComposer = 717; // PRODUCTION-201611291003-338511768
        public const int SendGameInvitationMessageComposer = 904; // PRODUCTION-201611291003-338511768
        public const int NavigatorSearchResultSetMessageComposer = 2690; // PRODUCTION-201611291003-338511768
        public const int IgnoreStatusMessageComposer = 207; // PRODUCTION-201611291003-338511768
        public const int GuideSessionEndedMessageComposer = 1456; // PRODUCTION-201611291003-338511768
        public const int UpdateMagicTileMessageComposer = 233; // PRODUCTION-201611291003-338511768
        public const int ShoutMessageComposer = 1036; // PRODUCTION-201611291003-338511768
        public const int MoodlightConfigMessageComposer = 2710; // PRODUCTION-201611291003-338511768
        public const int FurnitureAliasesMessageComposer = 1723; // PRODUCTION-201611291003-338511768
        public const int LoveLockDialogueCloseMessageComposer = 382; // PRODUCTION-201611291003-338511768
        public const int GuideSessionErrorMessageComposer = 673; // PRODUCTION-201611291003-338511768
        public const int TradingErrorMessageComposer = 217; // PRODUCTION-201611291003-338511768
        public const int ProfileInformationMessageComposer = 3898; // PRODUCTION-201611291003-338511768
        public const int UnknownCalendarMessageComposer = 2551; // PRODUCTION-201611291003-338511768
        public const int ModeratorRoomInfoMessageComposer = 1333; // changed structure - PRODUCTION-201611291003-338511768
        public const int CampaignMessageComposer = 1745; // PRODUCTION-201611291003-338511768
        public const int LoveLockDialogueMessageComposer = 3753; // PRODUCTION-201611291003-338511768
        public const int GuideSessionStartedMessageComposer = 3209; // PRODUCTION-201611291003-338511768
        public const int PurchaseErrorMessageComposer = 1404; // PRODUCTION-201611291003-338511768
        public const int PopularRoomTagsResultMessageComposer = 2012; // PRODUCTION-201611291003-338511768
        public const int CampaignCalendarDataMessageComposer = 2531; // PRODUCTION-201611291003-338511768
        public const int GiftWrappingErrorMessageComposer = 1517; // PRODUCTION-201611291003-338511768
        public const int WhisperMessageComposer = 2704; // PRODUCTION-201611291003-338511768
        public const int CatalogItemDiscountMessageComposer = 2347; // PRODUCTION-201611291003-338511768
        public const int HabboGroupBadgesMessageComposer = 2402; // PRODUCTION-201611291003-338511768
        public const int CanCreateRoomMessageComposer = 378; // PRODUCTION-201611291003-338511768
        public const int ThreadDataMessageComposer = 509; // PRODUCTION-201611291003-338511768
        public const int HabboClubCenterInfoMessageComposer = 3277; // PRODUCTION-201611291003-338511768
        public const int TradingFinishMessageComposer = 1001; // PRODUCTION-201611291003-338511768
        public const int GuestRoomSearchResultMessageComposer = 52; // PRODUCTION-201611291003-338511768
        public const int DanceMessageComposer = 2233; // PRODUCTION-201611291003-338511768
        public const int GenericErrorMessageComposer = 1600; // PRODUCTION-201611291003-338511768
        public const int NavigatorPreferencesMessageComposer = 518; // PRODUCTION-201611291003-338511768
        public const int GuideSessionMsgMessageComposer = 841; // PRODUCTION-201611291003-338511768
        public const int MutedMessageComposer = 826; // PRODUCTION-201611291003-338511768
        public const int BroadcastMessageAlertMessageComposer = 3801; // PRODUCTION-201611291003-338511768
        public const int YouAreOwnerMessageComposer = 339; // PRODUCTION-201611291003-338511768
        public const int FindFriendsProcessResultMessageComposer = 1210; // PRODUCTION-201611291003-338511768
        public const int GroupMemberUpdatedMessageComposer = 265; // PRODUCTION-201611291003-338511768
        public const int ModeratorTicketChatlogMessageComposer = 607; // PRODUCTION-201611291003-338511768
        public const int BadgeDefinitionsMessageComposer = 2501; // PRODUCTION-201611291003-338511768
        public const int UserRemoveMessageComposer = 2661; // PRODUCTION-201611291003-338511768
        public const int RoomSettingsSavedMessageComposer = 948; // PRODUCTION-201611291003-338511768
        public const int ModeratorUserRoomVisitsMessageComposer = 1752; // PRODUCTION-201611291003-338511768
        public const int RoomErrorNotifMessageComposer = 2913; // PRODUCTION-201611291003-338511768
        public const int UpdateUsernameMessageComposer = 118; // PRODUCTION-201611291003-338511768
        public const int NavigatorLiftedRoomsMessageComposer = 3104; // PRODUCTION-201611291003-338511768
        public const int UnknownGroupMessageComposer = 2445; // PRODUCTION-201611291003-338511768
        public const int NavigatorMetaDataParserMessageComposer = 3052; // PRODUCTION-201611291003-338511768
        public const int UpdateFavouriteGroupMessageComposer = 3403; // PRODUCTION-201611291003-338511768
        public const int GetRelationshipsMessageComposer = 2016; // PRODUCTION-201611291003-338511768
        public const int ItemRemoveMessageComposer = 3208; // PRODUCTION-201611291003-338511768
        public const int BCBorrowedItemsMessageComposer = 3828; // PRODUCTION-201611291003-338511768
        public const int GameAccountStatusMessageComposer = 2893; // PRODUCTION-201611291003-338511768
        public const int ThreadCreatedMessageComposer = 1862; // PRODUCTION-201611291003-338511768
        public const int EnforceCategoryUpdateMessageComposer = 3896; // PRODUCTION-201611291003-338511768
        public const int AchievementProgressedMessageComposer = 2107; // PRODUCTION-201611291003-338511768
        public const int ActivityPointsMessageComposer = 2018; // PRODUCTION-201611291003-338511768
        public const int PetInventoryMessageComposer = 3522; // changed structure - PRODUCTION-201611291003-338511768
        public const int GetRoomBannedUsersMessageComposer = 1869; // PRODUCTION-201611291003-338511768
        public const int UserUpdateMessageComposer = 1640; // PRODUCTION-201611291003-338511768
        public const int FavouritesMessageComposer = 151; // PRODUCTION-201611291003-338511768
        public const int WardrobeMessageComposer = 3315; // PRODUCTION-201611291003-338511768
        public const int QuizResultsMessageComposer = 2772; // PRODUCTION-201611291003-338511768
        public const int MarketPlaceOffersMessageComposer = 680; // PRODUCTION-201611291003-338511768
        public const int LoveLockDialogueSetLockedMessageComposer = 382; // PRODUCTION-201611291003-338511768
        public const int TradingAcceptMessageComposer = 2568; // PRODUCTION-201611291003-338511768
        public const int GameAchievementListMessageComposer = 2265; // PRODUCTION-201611291003-338511768
        public const int GetYouTubePlaylistMessageComposer = 1112; // PRODUCTION-201611291003-338511768
        public const int SetUniqueIdMessageComposer = 1488; // PRODUCTION-201611291003-338511768
        public const int RoomMuteSettingsMessageComposer = 2533; // PRODUCTION-201611291003-338511768
        public const int GetYouTubeVideoMessageComposer = 1411; // PRODUCTION-201611291003-338511768
        public const int GetClubComposer = 2405; // PRODUCTION-201611291003-338511768
        public const int OpenHelpToolMessageComposer = 1121; // PRODUCTION-201611291003-338511768
        public const int UserPerksMessageComposer = 2586; // PRODUCTION-201611291003-338511768
        public const int TargetOfferMessageComposer = 119; // PRODUCTION-201611291003-338511768
        public const int AvatarAspectUpdateMessageComposer = 2429; // PRODUCTION-201611291003-338511768

        public const int RentableSpacesErrorMessageComposer = 1868; // PRODUCTION-201611291003-338511768
        public const int RentableSpaceMessageComposer = 3559; // PRODUCTION-201611291003-338511768

        //public const int CameraPhotoPreviewComposer =-1;//error 404
        //public const int CameraPhotoPurchaseOkComposer =-1;//error 404
        //public const int CameraPriceComposer =-1;//error 404
        //Camera
        public const int CameraPhotoPreviewComposer = 3696; // PRODUCTION-201611291003-338511768
        public const int CameraPhotoPurchaseOkComposer = 2783; // PRODUCTION-201611291003-338511768
        public const int CameraPriceComposer = 3878; // PRODUCTION-201611291003-338511768
        public const int SendRoomThumbnailAlertMessageComposer = 3595; // PRODUCTION-201611291003-338511768

        // Nux Alerts
        public const int NuxAlertMessageComposer = 2023; // PRODUCTION-201611291003-338511768
        public const int NuxUserStatus = 3738; // PRODUCTION-201611291003-338511768

        // Jukebox
        public const int TraxSongInfoMessageComposer = 3365; // PRODUCTION-201611291003-338511768
        public const int LoadJukeboxUserMusicItemsMessageComposer = 2602; // PRODUCTION-201611291003-338511768
        public const int SetJukeboxNowPlayingMessageComposer = 469; // PRODUCTION-201611291003-338511768
        public const int SetJukeboxSongMusicDataMessageComposer = 3365; // PRODUCTION-201611291003-338511768
        public const int SetJukeboxPlayListMessageComposer = 34; // PRODUCTION-201611291003-338511768

        // Quickpolls
        public const int SendPollInvinteMessageComposer = 3785; // PRODUCTION-201611291003-338511768
        public const int PollQuestionsMessageComposer = 2997; // PRODUCTION-201611291003-338511768
        public const int PollErrorAlertMessageComposer = 662; // PRODUCTION-201611291003-338511768
        public const int MatchingPollMessageComposer = 2665; // PRODUCTION-201611291003-338511768
        public const int MatchingPollResultsMessageComposer = 1066; // PRODUCTION-201611291003-338511768
        public const int MatchingPollFinishMessageComposer = 2589; // PRODUCTION-201611291003-338511768

        // Polls
        public const int PollContentsMessageComposer = 2997; // PRODUCTION-201611291003-338511768
        public const int PollOfferMessageComposer = 3785; // PRODUCTION-201611291003-338511768

        // Ferramenta de Ajudantes
        public const int CloseHelperSessionMessageComposer = 138; // PRODUCTION-201611291003-338511768
        public const int InitHelperSessionChatMessageComposer = 3209; // PRODUCTION-201611291003-338511768
        public const int EndHelperSessionMessageComposer = 1456; // PRODUCTION-201611291003-338511768
        public const int HelperSessionSendChatMessageComposer = 841; // PRODUCTION-201611291003-338511768
        public const int HelperSessionVisiteRoomMessageComposer = 1847; // PRODUCTION-201611291003-338511768
        public const int HelperSessionInvinteRoomMessageComposer = 219; // PRODUCTION-201611291003-338511768
        public const int HelperSessionChatIsTypingMessageComposer = 1016; // PRODUCTION-201611291003-338511768
        public const int CallForHelperWindowMessageComposer = 1591; // PRODUCTION-201611291003-338511768
        public const int HandleHelperToolMessageComposer = 1548; // PRODUCTION-201611291003-338511768
        public const int CallForHelperErrorMessageComposer = 673; // PRODUCTION-201611291003-338511768

        // Hall
        public const int UpdateHallOfFameCodeDataMessageComposer = 1745; // PRODUCTION-201611291003-338511768
        public const int UpdateHallOfFameListMessageComposer = 3005; // PRODUCTION-201611291003-338511768

        // Forum de Grupos
        public const int ForumsListDataMessageComposer = 3001; // PRODUCTION-201611291003-338511768
        public const int GroupForumNewThreadMessageComposer = 1862; // PRODUCTION-201611291003-338511768
        public const int GroupForumThreadUpdateMessageComposer = 2528; // PRODUCTION-201611291003-338511768
        public const int GroupForumNewResponseMessageComposer = 2049; // PRODUCTION-201611291003-338511768
        public const int GroupForumThreadRootMessageComposer = 1073; // PRODUCTION-201611291003-338511768
        public const int GroupForumListingsMessageComposer = 3001; // PRODUCTION-201611291003-338511768
        public const int GroupForumReadThreadMessageComposer = 509; // PRODUCTION-201611291003-338511768
        public const int GroupForumDataMessageComposer = 3011; // PRODUCTION-201611291003-338511768


        // test
        public const int EpicPopupFrameComposer = 3945; // PRODUCTION-201611291003-338511768
        public const int CostumNotificationComposer = 909; // PRODUCTION-201611291003-338511768
        public const int HotelClosedAndOpensComposer = 3728; // PRODUCTION-201611291003-338511768
        public const int HotellWillClosedInMinutesComposer = 1050; // PRODUCTION-201611291003-338511768

        // need struture
        public const int BonusRareComposer = 1533; // PRODUCTION-201611291003-338511768
        public const int DailyQuestComposer = 1878; // PRODUCTION-201611291003-338511768
        public const int DebugConsole = 3284; // PRODUCTION-201611291003-338511768


        // freeze 
        public const int FreezeLifesComposer = 2324; // PRODUCTION-201611291003-338511768


        //MysticBox
        public const int MysticBoxStartOpenComposer = 3201; // PRODUCTION-201611291003-338511768
        public const int MysticBoxPrizeComposer = 3712; // PRODUCTION-201611291003-338511768
        public const int MysticBoxCloseComposer = 596; // PRODUCTION-201611291003-338511768


        //EffectsListAdd
        public const int EffectsListAddComposer = 2867; // PRODUCTION-201611291003-338511768
        public const int EffectsListEffectEnableComposer = 1959; // PRODUCTION-201611291003-338511768
        public const int EffectsListRemoveComposer = 2228; // PRODUCTION-201611291003-338511768


        public const int FurniMaticNoRoomError = 3433; // PRODUCTION-201611291003-338511768
        public const int FurniMaticReceiveItem = 468; // PRODUCTION-201611291003-338511768
        public const int FurniMaticRewardsComposer = 3164; // PRODUCTION-201611291003-338511768


        public const int NuxItemListComposer = 3575; // PRODUCTION-201611291003-338511768
        public const int NuxSuggestFreeGiftsMessageComposer = 3639; // PRODUCTION-201611291003-338511768
        public const int MassEventComposer = -1; //error 404

        // Crafting
        public const int CraftableProductsMessageComposer = 1000; // PRODUCTION-201611291003-338511768
        public const int CraftingRecipeMessageComposer = 2774; // PRODUCTION-201611291003-338511768
        public const int CraftingResultMessageComposer = 3128; // PRODUCTION-201611291003-338511768
        public const int CraftingFoundMessageComposer = 2124; // PRODUCTION-201611291003-338511768

        public const int RoomCustomizedAlertComposer = 934; // PRODUCTION-201611291003-338511768
    }
}