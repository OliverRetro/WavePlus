using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    internal class ManageGroupComposer : MessageComposer
    {
        public Group Group { get; }
        public string[] BadgeParts { get; }

        public ManageGroupComposer(Group group, string[] badgeParts)
            : base(ServerPacketHeader.ManageGroupMessageComposer)
        {
            Group = group;
            BadgeParts = badgeParts;
        }

        public override void Compose(ServerPacket packet)
        {
            string roomName = Group.GetRoom()?.Name ?? string.Empty;

            // Match Java packet structure exactly
            packet.WriteInteger(1);
            packet.WriteInteger(Group.RoomId);
            packet.WriteString(roomName);

            packet.WriteBoolean(false);
            packet.WriteBoolean(true);

            packet.WriteInteger(Group.Id);
            packet.WriteString(Group.Name);
            packet.WriteString(Group.Description);

            packet.WriteInteger(Group.RoomId);

            packet.WriteInteger(Group.Colour1);
            packet.WriteInteger(Group.Colour2);

            packet.WriteInteger(Group.Type == GroupType.Open ? 0 : Group.Type == GroupType.Locked ? 1 : 2);
            packet.WriteInteger(Group.AdminOnlyDeco);

            packet.WriteBoolean(false);
            packet.WriteString("");

            packet.WriteInteger(5);

            string badge = Group.Badge.Replace("b", "");
            string[] data = badge.Split('s');

            int req = 5 - data.Length;

            foreach (string s in data) {
                packet.WriteInteger(
                    s.Length >= 6
                        ? int.Parse(s.Substring(0, 3))
                        : int.Parse(s.Substring(0, 2)));

                packet.WriteInteger(
                    s.Length >= 6
                        ? int.Parse(s.Substring(3, 2))
                        : int.Parse(s.Substring(2, 2)));

                if (s.Length < 5)
                    packet.WriteInteger(0);
                else if (s.Length >= 6)
                    packet.WriteInteger(int.Parse(s.Substring(5, 1)));
                else
                    packet.WriteInteger(int.Parse(s.Substring(4, 1)));
            }

            while (req > 0) {
                packet.WriteInteger(0);
                packet.WriteInteger(0);
                packet.WriteInteger(0);
                req--;
            }

            packet.WriteString(Group.Badge);
            packet.WriteInteger(Group.MemberCount);
        }
    }
}