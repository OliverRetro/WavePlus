using System.Collections.Generic;
using System.Linq;
using System.Text;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class UserUpdateComposer : MessageComposer
    {
        public ICollection<RoomUser> Users { get; }

        public UserUpdateComposer(ICollection<RoomUser> users)
            : base(ServerPacketHeader.UserUpdateMessageComposer)
        {
            Users = users;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Users.Count);
            foreach (RoomUser user in Users.ToList()) {
                packet.WriteInteger(user.VirtualId);
                packet.WriteInteger(user.X);
                packet.WriteInteger(user.Y);
                packet.WriteString(user.Z.ToString("0.00"));
                packet.WriteInteger(user.RotHead);
                packet.WriteInteger(user.RotBody);

                StringBuilder statusComposer = new();
                statusComposer.Append("/");

                foreach (KeyValuePair<string, string> status in user.Statusses.ToList()) {
                    statusComposer.Append(status.Key);

                    if (!string.IsNullOrEmpty(status.Value)) {
                        statusComposer.Append(" ");
                        statusComposer.Append(status.Value);
                    }

                    statusComposer.Append("/");
                }

                statusComposer.Append("/");
                packet.WriteString(statusComposer.ToString());
            }
        }
    }
}