using System.Collections.Generic;

namespace Plus.HabboHotel.Roleplay.Clothing
{
    public sealed class RpClothingCategory
    {
        public int Id { get; }
        public string TabName { get; }
        public int RoomId { get; }

        /// <summary>Visible sets belonging to this tab, in id order.</summary>
        public List<RpClothingSet> Sets { get; } = new();

        public RpClothingCategory(int id, string tabName, int roomId)
        {
            Id = id;
            TabName = tabName ?? "Unnamed";
            RoomId = roomId;
        }
    }
}