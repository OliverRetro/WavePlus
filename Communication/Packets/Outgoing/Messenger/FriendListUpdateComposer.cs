using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.Communication.Packets.Outgoing.Messenger
{
    internal class FriendListUpdateComposer : MessageComposer
    {
        public int FriendId { get; }

        public Habbo Habbo { get; }
        public MessengerBuddy Buddy { get; }

        public FriendListUpdateComposer(int friendId)
            : base(ServerPacketHeader.FriendListUpdateMessageComposer)
        {
            FriendId = friendId;
        }

        public FriendListUpdateComposer(Habbo habbo, MessengerBuddy buddy)
            : base(ServerPacketHeader.FriendListUpdateMessageComposer)
        {
            Habbo = habbo;
            Buddy = buddy;
        }

        public override void Compose(ServerPacket packet)
        {
            if (Habbo != null)
            {
                packet.WriteInteger(0); //Category Count
                packet.WriteInteger(1); //Updates Count
                packet.WriteInteger(0); //Update

                Buddy.Serialize(packet, Habbo);
            }
            else
            {
                packet.WriteInteger(0); //Category Count
                packet.WriteInteger(1); //Updates Count
                packet.WriteInteger(-1); //Update
                packet.WriteInteger(FriendId);
            }
        }
    }
}