using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.RpItem.Item
{
    public static class RpItemClothingService
    {
        private static readonly ConcurrentDictionary<int, string> _baseLooks = new();

        public static void Refresh(Habbo habbo)
        {
            if (habbo == null)
                return;

            UserRpItems items = habbo.GetRpItems();
            if (items == null)
                return;

            List<UserRpItem> clothing = items.Items
                .Where(i => i.Equipped && i.ItemData != null && i.ItemData.HasClothing)
                .ToList();

            if (clothing.Count == 0) {
                if (_baseLooks.TryRemove(habbo.Id, out string baseLook) && baseLook != null) {
                    habbo.Look = baseLook;
                    // Removing the last clothing item mid-shift must hand the figure back to work.
                    if (!PlusEnvironment.GetGame().GetShiftManager().ReapplyCostume(habbo))
                        Broadcast(habbo);
                }
                return;
            }

            string root = _baseLooks.GetOrAdd(habbo.Id, _ => TrueOwnLook(habbo));

            string look = root;
            foreach (UserRpItem it in clothing) {
                string figure = ResolveFigure(it.ItemData.Clothing, habbo.Gender);
                if (!string.IsNullOrWhiteSpace(figure))
                    look = MergeFigure(look, figure);
            }

            if (habbo.Look != look) {
                habbo.Look = look;
                Broadcast(habbo);
            }
        }

        public static void RestoreOnLogout(Habbo habbo)
        {
            if (habbo != null && _baseLooks.TryRemove(habbo.Id, out string baseLook) && baseLook != null)
                habbo.Look = baseLook;
        }

        public static string GetBaseLook(Habbo habbo) =>
            habbo != null && _baseLooks.TryGetValue(habbo.Id, out string baseLook) ? baseLook : null;

        public static bool HasClothingEquipped(Habbo habbo) => GetBaseLook(habbo) != null;

        private static string TrueOwnLook(Habbo habbo) =>
            PlusEnvironment.GetGame().GetShiftManager().IsWorking(habbo.Id) && habbo.OldLook != null ? habbo.OldLook : habbo.Look;

        private static string ResolveFigure(string clothing, string gender)
        {
            if (string.IsNullOrWhiteSpace(clothing))
                return null;

            if (!clothing.Contains('|'))
                return clothing;

            Dictionary<string, string> figures = new();
            foreach (string part in clothing.Split(';')) {
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                string[] split = part.Split('|');
                if (split.Length == 2)
                    figures[split[0].ToLower()] = split[1];
            }

            string g = (gender ?? "m").ToLower();
            if (figures.TryGetValue(g, out string fig))
                return fig;
            return figures.TryGetValue("m", out fig) ? fig : null;
        }

        public static string MergeFigure(string baseLook, string overrideFigure)
        {
            Dictionary<string, string> lookParts = new();

            foreach (string part in (baseLook ?? "").Split('.')) {
                if (string.IsNullOrWhiteSpace(part))
                    continue;
                string[] split = part.Split('-');
                if (split.Length > 0)
                    lookParts[split[0]] = part;
            }

            foreach (string part in overrideFigure.Split('.')) {
                if (string.IsNullOrWhiteSpace(part))
                    continue;
                string[] split = part.Split('-');
                if (split.Length > 0)
                    lookParts[split[0]] = part;
            }

            return string.Join(".", lookParts.Values);
        }

        private static void Broadcast(Habbo habbo)
        {
            Room room = habbo.CurrentRoom;
            RoomUser roomUser = room?.GetRoomUserManager()?.GetRoomUserByHabbo(habbo.Id);
            if (roomUser != null)
                room.SendPacket(new UserChangeComposer(roomUser, false));
        }
    }
}