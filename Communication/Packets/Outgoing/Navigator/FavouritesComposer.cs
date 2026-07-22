using System.Collections;

namespace Plus.Communication.Packets.Outgoing.Navigator
{
    internal class FavouritesComposer : MessageComposer
    {
        public ArrayList FavouriteIds { get; }

        public FavouritesComposer(ArrayList favouriteIds)
            : base(ServerPacketHeader.FavouritesMessageComposer)
        {
            FavouriteIds = favouriteIds;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(50);
            packet.WriteInteger(FavouriteIds.Count);

            foreach (int id in FavouriteIds.ToArray()) {
                packet.WriteInteger(id);
            }
        }
    }
}