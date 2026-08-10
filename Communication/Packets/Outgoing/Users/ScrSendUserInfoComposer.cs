using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Users
{
    internal class ScrSendUserInfoComposer : MessageComposer
    {
        private readonly Habbo _habbo;

        public ScrSendUserInfoComposer(Habbo habbo)
            : base(ServerPacketHeader.ScrSendUserInfoMessageComposer)
        {
            _habbo = habbo;
        }

        public override void Compose(ServerPacket packet)
        {
            bool isVip = _habbo != null && _habbo.IsVip;
            int minutesLeft = isVip ? _habbo.VipMinutesRemaining : 0;

            // Client maps: daysToPeriodEnd -> clubDays, periodsSubscribedAhead -> clubPeriods.
            // clubLevel is derived from those, so any positive value keeps the purse out of the
            // "no club" state. The friendly countdown itself is driven by minutesUntilExpiration.
            int daysLeft = isVip ? System.Math.Max(1, (minutesLeft + 1439) / 1440) : 0;

            packet.WriteString("habbo_club");
            packet.WriteInteger(daysLeft);       // daysToPeriodEnd -> clubDays
            packet.WriteInteger(0);              // memberPeriods
            packet.WriteInteger(0);              // periodsSubscribedAhead -> clubPeriods
            packet.WriteInteger(1);              // responseType (login)
            packet.WriteBoolean(isVip);          // hasEverBeenMember
            packet.WriteBoolean(isVip);          // isVip
            packet.WriteInteger(0);              // pastClubDays
            packet.WriteInteger(0);              // pastVipDays
            packet.WriteInteger(minutesLeft);    // minutesUntilExpiration
        }
    }
}