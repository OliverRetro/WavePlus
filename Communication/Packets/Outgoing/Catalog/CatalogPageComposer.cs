using System.Linq;
using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.Catalog.Utilities;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Outgoing.Catalog
{
    public class CatalogPageComposer : MessageComposer
    {
        public CatalogPage Page { get; }
        public string CatalogueMode { get; }

        public CatalogPageComposer(CatalogPage page, string cataMode)
            : base(ServerPacketHeader.CatalogPageMessageComposer)
        {
            Page = page;
            CatalogueMode = cataMode;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Page.Id);
            packet.WriteString(CatalogueMode);
            packet.WriteString(Page.Template);

            packet.WriteInteger(Page.PageStrings1.Count);
            foreach (string s in Page.PageStrings1)
            {
                packet.WriteString(s);
            }

            packet.WriteInteger(Page.PageStrings2.Count);
            foreach (string s in Page.PageStrings2)
            {
                packet.WriteString(s);
            }

            if (!Page.Template.Equals("frontpage") && !Page.Template.Equals("club_buy"))
            {
                packet.WriteInteger(Page.Items.Count);
                foreach (CatalogItem item in Page.Items.Values)
                {
                    packet.WriteInteger(item.Id);
                    packet.WriteString(item.Name);
                    packet.WriteBoolean(false); //IsRentable
                    packet.WriteInteger(item.CostCredits);

                    if (item.CostDiamonds > 0)
                    {
                        packet.WriteInteger(item.CostDiamonds);
                        packet.WriteInteger(5); // Diamonds
                    }
                    else
                    {
                        packet.WriteInteger(item.CostPixels);
                        packet.WriteInteger(0); // Type of PixelCost
                    }

                    packet.WriteBoolean(ItemUtility.CanGiftItem(item));

                    if (item.Data.InteractionType == InteractionType.Deal || item.Data.InteractionType == InteractionType.RoomDeal)
                    {
                        CatalogDeal deal = null;
                        if (!PlusEnvironment.GetGame().GetCatalog().TryGetDeal(item.Data.BehaviourData, out deal))
                        {
                            packet.WriteInteger(0); //Count
                        }
                        else
                        {
                            packet.WriteInteger(deal.ItemDataList.Count);

                            foreach (CatalogItem dealItem in deal.ItemDataList.ToList())
                            {
                                packet.WriteString(dealItem.Data.Type.ToString());
                                packet.WriteInteger(dealItem.Data.SpriteId);
                                packet.WriteString("");
                                packet.WriteInteger(dealItem.Amount);
                                packet.WriteBoolean(false);
                            }
                        }
                    }
                    else
                    {
                        packet.WriteInteger(string.IsNullOrEmpty(item.Badge) ? 1 : 2); //Count 1 item if there is no badge, otherwise count as 2.

                        if (!string.IsNullOrEmpty(item.Badge))
                        {
                            packet.WriteString("b");
                            packet.WriteString(item.Badge);
                        }

                        packet.WriteString(item.Data.Type.ToString());
                        if (item.Data.Type.ToString().ToLower() == "b")
                        {
                            //This is just a badge, append the name.
                            packet.WriteString(item.Data.ItemName);
                        }
                        else
                        {
                            packet.WriteInteger(item.Data.SpriteId);
                            if (item.Data.InteractionType == InteractionType.Wallpaper || item.Data.InteractionType == InteractionType.Floor || item.Data.InteractionType == InteractionType.Landscape)
                            {
                                packet.WriteString(item.Name.Split('_')[2]);
                            }
                            else if (item.Data.InteractionType == InteractionType.Bot) //Bots
                            {
                                CatalogBot catalogBot = null;
                                if (!PlusEnvironment.GetGame().GetCatalog().TryGetBot(item.ItemId, out catalogBot))
                                    packet.WriteString("hd-180-7.ea-1406-62.ch-210-1321.hr-831-49.ca-1813-62.sh-295-1321.lg-285-92");
                                else
                                    packet.WriteString(catalogBot.Figure);
                            }
                            else if (item.ExtraData != null)
                            {
                                packet.WriteString(item.ExtraData != null ? item.ExtraData : string.Empty);
                            }

                            packet.WriteInteger(item.Amount);
                            packet.WriteBoolean(item.IsLimited); // IsLimited
                            if (item.IsLimited)
                            {
                                packet.WriteInteger(item.LimitedEditionStack);
                                packet.WriteInteger(item.LimitedEditionStack - item.LimitedEditionSells);
                            }
                        }
                    }

                    packet.WriteInteger(0); //club_level
                    packet.WriteBoolean(ItemUtility.CanSelectAmount(item));

                    packet.WriteBoolean(false); // TODO: Figure out
                    packet.WriteString(""); //previewImage -> e.g; catalogue/pet_lion.png
                }
            }
            else
                packet.WriteInteger(0);

            packet.WriteInteger(-1);
            packet.WriteBoolean(false);

            packet.WriteInteger(PlusEnvironment.GetGame().GetCatalog().GetPromotions().ToList().Count); //Count
            foreach (CatalogPromotion promotion in PlusEnvironment.GetGame().GetCatalog().GetPromotions().ToList())
            {
                packet.WriteInteger(promotion.Id);
                packet.WriteString(promotion.Title);
                packet.WriteString(promotion.Image);
                packet.WriteInteger(promotion.Unknown);
                packet.WriteString(promotion.PageLink);
                packet.WriteInteger(promotion.ParentId);
            }
        }
    }
}