using System;
using System.Collections.Generic;
using System.Text;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class UserUpdateComposer : MessageComposer
    {
        [ThreadStatic] private static StringBuilder _statusBuilder;

        private readonly Entry[] _entries;
        private readonly int _count;

        public UserUpdateComposer(ICollection<RoomUser> users)
            : base(ServerPacketHeader.UserUpdateMessageComposer)
        {
            _entries = new Entry[users.Count];

            StringBuilder builder = _statusBuilder ??= new StringBuilder(64);
            int count = 0;

            foreach (RoomUser user in users) {
                if (user == null)
                    continue;

                int zMillis = (int)Math.Round(user.Z * 1000d);

                _entries[count++] = new Entry(user.VirtualId, user.X, user.Y, zMillis, user.RotHead, user.RotBody, BuildStatus(builder, user));
            }

            _count = count;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(_count);

            for (int i = 0; i < _count; i++) {
                Entry entry = _entries[i];

                packet.WriteInteger(entry.VirtualId);
                packet.WriteInteger(entry.X);
                packet.WriteInteger(entry.Y);
                packet.WriteInteger(entry.ZMillis);
                packet.WriteInteger(entry.RotHead);
                packet.WriteInteger(entry.RotBody);
                packet.WriteString(entry.Status);
            }
        }

        private static string BuildStatus(StringBuilder builder, RoomUser user)
        {
            builder.Clear();
            builder.Append('/');

            foreach (KeyValuePair<string, string> status in user.Statusses) {
                builder.Append(status.Key);

                if (!string.IsNullOrEmpty(status.Value)) {
                    builder.Append(' ');
                    builder.Append(status.Value);
                }

                builder.Append('/');
            }

            builder.Append('/');

            string cached = user.SerializedStatus;

            if (cached != null && builder.Equals(cached.AsSpan()))
                return cached;

            cached = builder.ToString();
            user.SerializedStatus = cached;

            return cached;
        }

        private readonly struct Entry(int virtualId, int x, int y, int zMillis, int rotHead, int rotBody, string status)
        {
            public readonly int VirtualId = virtualId;
            public readonly int X = x;
            public readonly int Y = y;
            public readonly int ZMillis = zMillis;
            public readonly int RotHead = rotHead;
            public readonly int RotBody = rotBody;
            public readonly string Status = status;
        }
    }
}