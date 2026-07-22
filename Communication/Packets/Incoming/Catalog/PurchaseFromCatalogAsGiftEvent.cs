using System;
using System.Globalization;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Moderation;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.Catalog.Utilities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Users;
using Plus.Utilities;
using CatalogItem = Plus.HabboHotel.Catalog.CatalogItem;
using CatalogPage = Plus.HabboHotel.Catalog.CatalogPage;

namespace Plus.Communication.Packets.Incoming.Catalog
{
    public class PurchaseFromCatalogAsGiftEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int pageId = packet.PopInt();
            int itemId = packet.PopInt();
            string data = packet.PopString();
            string giftUser = StringCharFilter.Escape(packet.PopString());
            string giftMessage = StringCharFilter.Escape(packet.PopString().Replace(Convert.ToChar(5), ' '));
            int spriteId = packet.PopInt();
            int ribbon = packet.PopInt();
            int colour = packet.PopInt();
            packet.PopBoolean();

            if (PlusEnvironment.GetSettingsManager().TryGetValue("room.item.gifts.enabled") != "1") {
                session.SendNotification("The hotel managers have disabled gifting");
                return;
            }

            if (!PlusEnvironment.GetGame().GetCatalog().TryGetPage(pageId, out CatalogPage page))
                return;

            if (!page.Enabled || !page.Visible || page.MinimumRank > session.GetHabbo().Rank || page.MinimumVip > session.GetHabbo().VipRank && session.GetHabbo().Rank == 1)
                return;

            if (!page.Items.TryGetValue(itemId, out CatalogItem item)) {
                if (page.ItemOffers.ContainsKey(itemId)) {
                    item = page.ItemOffers[itemId];
                    if (item == null)
                        return;
                } else
                    return;
            }

            if (!ItemUtility.CanGiftItem(item))
                return;

            if (!PlusEnvironment.GetGame().GetItemManager().GetGift(spriteId, out ItemData presentData) || presentData.InteractionType != InteractionType.Gift)
                return;

            if (session.GetHabbo().Credits < item.CostCredits) {
                session.SendPacket(new PresentDeliverErrorMessageComposer(true, false));
                return;
            }

            if (session.GetHabbo().Duckets < item.CostPixels) {
                session.SendPacket(new PresentDeliverErrorMessageComposer(false, true));
                return;
            }

            Habbo habbo = PlusEnvironment.GetHabboByUsername(giftUser);
            if (habbo == null) {
                session.SendPacket(new GiftWrappingErrorComposer());
                return;
            }

            if (!habbo.AllowGifts) {
                session.SendNotification("Oops, this user doesn't allow gifts to be sent to them!");
                return;
            }

            if ((DateTime.Now - session.GetHabbo().LastGiftPurchaseTime).TotalSeconds <= 15.0) {
                session.SendNotification("You're purchasing gifts too fast! Please wait 15 seconds!");

                session.GetHabbo().GiftPurchasingWarnings += 1;
                if (session.GetHabbo().GiftPurchasingWarnings >= 25)
                    session.GetHabbo().SessionGiftBlocked = true;
                return;
            }

            if (session.GetHabbo().SessionGiftBlocked)
                return;

            string ed = giftUser + Convert.ToChar(5) + giftMessage + Convert.ToChar(5) + session.GetHabbo().Id + Convert.ToChar(5) + item.Data.Id + Convert.ToChar(5) + spriteId + Convert.ToChar(5) + ribbon + Convert.ToChar(5) + colour;

            int newItemId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                //Insert the dummy item.
                Plus.Database.EF.Entities.ItemEntity dummyItem = new()
                {
                    BaseItem = (uint)presentData.Id,
                    UserId = habbo.Id,
                    ExtraData = ed
                };
                db.Items.Add(dummyItem);
                db.SaveChanges();
                newItemId = (int)dummyItem.Id;

                string itemExtraData = null;
                switch (item.Data.InteractionType) {
                    case InteractionType.None:
                        itemExtraData = "";
                        break;

                    #region Pet handling

                    case InteractionType.Pet:

                        try {
                            string[] bits = data.Split('\n');
                            string petName = bits[0];
                            string race = bits[1];
                            string color = bits[2];

                            if (PetUtility.CheckPetName(petName))
                                return;

                            if (race.Length > 2)
                                return;

                            if (color.Length != 6)
                                return;

                            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_PetLover", 1);
                        } catch {
                            return;
                        }

                        break;

                    #endregion

                    case InteractionType.Floor:
                    case InteractionType.Wallpaper:
                    case InteractionType.Landscape:

                        double number = 0;
                        try {
                            number = string.IsNullOrEmpty(data) ? 0 : double.Parse(data, PlusEnvironment.CultureInfo);
                        } catch {
                            //ignored
                        }

                        itemExtraData = number.ToString(CultureInfo.CurrentCulture).Replace(',', '.');
                        break; // maintain extra data // todo: validate

                    case InteractionType.PostIt:
                        itemExtraData = "FFFF33";
                        break;

                    case InteractionType.Moodlight:
                        itemExtraData = "1,1,1,#000000,255";
                        break;

                    case InteractionType.Trophy:
                        itemExtraData = session.GetHabbo().Username + Convert.ToChar(9) + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + Convert.ToChar(9) + data;
                        break;

                    case InteractionType.Mannequin:
                        itemExtraData = "m" + Convert.ToChar(5) + ".ch-210-1321.lg-285-92" + Convert.ToChar(5) + "Default Mannequin";
                        break;

                    case InteractionType.BadgeDisplay:
                        if (!session.GetHabbo().GetBadgeComponent().HasBadge(data)) {
                            session.SendPacket(new BroadcastMessageAlertComposer("Oops, it appears that you do not own this badge."));
                            return;
                        }

                        itemExtraData = data + Convert.ToChar(9) + session.GetHabbo().Username + Convert.ToChar(9) + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year;
                        break;

                    default:
                        itemExtraData = data;
                        break;
                }

                //Insert the present, forever.
                db.UserPresents.Add(new UserPresentEntity
                {
                    ItemId = (uint)newItemId,
                    BaseId = (uint)item.Data.Id,
                    ExtraData = string.IsNullOrEmpty(itemExtraData) ? "" : itemExtraData
                });
                db.SaveChanges();

                //Here we're clearing up a record, this is dumb, but okay.
                uint deleteId = (uint)newItemId;
                db.Items.Where(i => i.Id == deleteId).ExecuteDelete();
            }

            HabboHotel.Items.Item giveItem = ItemFactory.CreateGiftItem(presentData, habbo, ed, ed, newItemId);
            if (giveItem != null) {
                GameClient receiver = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(habbo.Id);
                if (receiver != null) {
                    receiver.GetHabbo().GetInventoryComponent().TryAddItem(giveItem);
                    receiver.SendPacket(new FurniListNotificationComposer(giveItem.Id, 1));
                    receiver.SendPacket(new PurchaseOkComposer());
                    receiver.SendPacket(new FurniListAddComposer(giveItem));
                    receiver.SendPacket(new FurniListUpdateComposer());
                }

                if (habbo.Id != session.GetHabbo().Id && !string.IsNullOrWhiteSpace(giftMessage)) {
                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_GiftGiver", 1);
                    if (receiver != null)
                        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(receiver, "ACH_GiftReceiver", 1);
                    PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.GiftOthers);
                }
            }

            session.SendPacket(new PurchaseOkComposer(item, presentData));

            if (item.CostCredits > 0) {
                session.GetHabbo().Credits -= item.CostCredits;
                session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
            }

            if (item.CostPixels > 0) {
                session.GetHabbo().Duckets -= item.CostPixels;
                session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, session.GetHabbo().Duckets));
            }

            session.GetHabbo().LastGiftPurchaseTime = DateTime.Now;
        }
    }
}