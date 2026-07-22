using System.Collections.Generic;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.Communication.Packets.Outgoing.Messenger
{
    internal class BuddyRequestsComposer : MessageComposer
    {
        public ICollection<MessengerRequest> Requests { get; }

        public BuddyRequestsComposer(ICollection<MessengerRequest> requests)
            : base(ServerPacketHeader.BuddyRequestsMessageComposer)
        {
            Requests = requests;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Requests.Count);
            packet.WriteInteger(Requests.Count);

            foreach (MessengerRequest request in Requests) {
                packet.WriteInteger(request.From);
                packet.WriteString(request.Username);

                UserCache user = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(request.From);
                packet.WriteString(user != null ? user.Look : "");
            }
        }
    }
}