using Plus.Communication.Packets.Outgoing.Avatar;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.GameClock;

namespace Plus.Communication.Packets.Incoming.Handshake
{
    public class InfoRetrieveEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.SendPacket(new UserObjectComposer(session.GetHabbo()));
            session.SendPacket(new UserPerksComposer(session.GetHabbo()));
            session.SendPacket(new WardrobeConfigComposer());
            // Push the ingame clock at game load (before any room), so the
            // day/night background is correct on first paint — no blue flash.
            session.SendPacket(new GameClockComposer(GameClockService.GetOffset()));
        }
    }
}