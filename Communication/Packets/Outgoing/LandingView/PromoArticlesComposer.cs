using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.LandingView.Promotions;

namespace Plus.Communication.Packets.Outgoing.LandingView
{
    internal class PromoArticlesComposer : MessageComposer
    {
        public ICollection<Promotion> LandingPromotions { get; }

        public PromoArticlesComposer(ICollection<Promotion> landingPromotions)
            : base(ServerPacketHeader.PromoArticlesMessageComposer)
        {
            LandingPromotions = landingPromotions;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(LandingPromotions.Count); //Count
            foreach (Promotion promotion in LandingPromotions.ToList()) {
                packet.WriteInteger(promotion.Id); //ID
                packet.WriteString(promotion.Title); //Title
                packet.WriteString(promotion.Text); //Text
                packet.WriteString(promotion.ButtonText); //Button text
                packet.WriteInteger(promotion.ButtonType); //Link type 0 and 3
                packet.WriteString(promotion.ButtonLink); //Link to article
                packet.WriteString(promotion.ImageLink); //Image link
            }
        }
    }
}