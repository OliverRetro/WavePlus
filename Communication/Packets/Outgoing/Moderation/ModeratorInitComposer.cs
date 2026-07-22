using System;
using System.Collections.Generic;
using Plus.HabboHotel.Moderation;
using Plus.Utilities;

namespace Plus.Communication.Packets.Outgoing.Moderation
{
    internal class ModeratorInitComposer : MessageComposer
    {
        public ICollection<string> UserPresets { get; }
        public ICollection<string> RoomPresets { get; }
        public ICollection<ModerationTicket> Tickets { get; }

        public ModeratorInitComposer(ICollection<string> userPresets, ICollection<string> roomPresets, ICollection<ModerationTicket> tickets)
            : base(ServerPacketHeader.ModeratorInitMessageComposer)
        {
            UserPresets = userPresets;
            RoomPresets = roomPresets;
            Tickets = tickets;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Tickets.Count);
            foreach (ModerationTicket ticket in Tickets) {
                packet.WriteInteger(ticket.Id); // Id
                packet.WriteInteger(ticket.GetStatus(Id)); // Tab ID
                packet.WriteInteger(ticket.Type); // Type
                packet.WriteInteger(ticket.Category); // Category
                packet.WriteInteger(Convert.ToInt32((DateTime.Now - UnixTimestamp.FromUnixTimestamp(ticket.Timestamp)).TotalMilliseconds)); // This should fix the overflow?
                packet.WriteInteger(ticket.Priority); // Priority
                packet.WriteInteger(ticket.Sender == null ? 0 : ticket.Sender.Id); // Sender ID
                packet.WriteInteger(1);
                packet.WriteString(ticket.Sender == null ? string.Empty : ticket.Sender.Username); // Sender Name
                packet.WriteInteger(ticket.Reported == null ? 0 : ticket.Reported.Id); // Reported ID
                packet.WriteString(ticket.Reported == null ? string.Empty : ticket.Reported.Username); // Reported Name
                packet.WriteInteger(ticket.Moderator == null ? 0 : ticket.Moderator.Id); // Moderator ID
                packet.WriteString(ticket.Moderator == null ? string.Empty : ticket.Moderator.Username); // Mod Name
                packet.WriteString(ticket.Issue); // Issue
                packet.WriteInteger(ticket.Room == null ? 0 : ticket.Room.Id); // Room Id
                packet.WriteInteger(0); //LOOP
            }

            packet.WriteInteger(UserPresets.Count);
            foreach (string pre in UserPresets) {
                packet.WriteString(pre);
            }

            /*base.WriteInteger(UserActionPresets.Count);
            foreach (KeyValuePair<string, List<ModerationPresetActionMessages>> Cat in UserActionPresets.ToList())
            {
                base.WriteString(Cat.Key);
                base.WriteBoolean(true);
                base.WriteInteger(Cat.Value.Count);
                foreach (ModerationPresetActionMessages Preset in Cat.Value.ToList())
                {
                    base.WriteString(Preset.Caption);
                    base.WriteString(Preset.MessageText);
                    base.WriteInteger(Preset.BanTime); // Account Ban Hours
                    base.WriteInteger(Preset.IPBanTime); // IP Ban Hours
                    base.WriteInteger(Preset.MuteTime); // Mute in Hours
                    base.WriteInteger(0);//Trading lock duration
                    base.WriteString(Preset.Notice + "\n\nPlease Note: Avatar ban is an IP ban!");
                    base.WriteBoolean(false);//Show HabboWay
                }
            }*/

            // TODO: Figure out
            packet.WriteInteger(0);
            {
                //Loop a string.
            }

            packet.WriteBoolean(true); // Ticket right
            packet.WriteBoolean(true); // Chatlogs
            packet.WriteBoolean(true); // User actions alert etc
            packet.WriteBoolean(true); // Kick users
            packet.WriteBoolean(true); // Ban users
            packet.WriteBoolean(true); // Caution etc
            packet.WriteBoolean(true); // Love you, Tom

            packet.WriteInteger(RoomPresets.Count);
            foreach (string pre in RoomPresets) {
                packet.WriteString(pre);
            }
        }
    }
}