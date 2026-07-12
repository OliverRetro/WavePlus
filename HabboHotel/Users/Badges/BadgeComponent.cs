using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Inventory.Badges;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.Badges;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Users.Badges
{
    public class BadgeComponent
    {
        private readonly Habbo _player;
        private readonly Dictionary<string, Badge> _badges;

        public BadgeComponent(Habbo player, UserData.UserData data)
        {
            _player = player;
            _badges = new Dictionary<string, Badge>();

            foreach (Badge badge in data.Badges)
            {
                if (!PlusEnvironment.GetGame().GetBadgeManager().TryGetBadge(badge.Code, out BadgeDefinition badgeDefinition) ||
                    badgeDefinition.RequiredRight.Length > 0 && !player.GetPermissions().HasRight(badgeDefinition.RequiredRight))
                    continue;

                if (!_badges.ContainsKey(badge.Code))
                    _badges.Add(badge.Code, badge);
            }
        }

        public int Count => _badges.Count;

        public int EquippedCount
        {
            get
            {
                int i = 0;

                foreach (Badge badge in _badges.Values)
                {
                    if (badge.Slot <= 0) {
                        continue;
                    }

                    i++;
                }

                return i;
            }
        }

        public ICollection<Badge> GetBadges()
        {
            return _badges.Values;
        }

        public Badge GetBadge(string badge)
        {
            return _badges.ContainsKey(badge) ? _badges[badge] : null;
        }

        public bool TryGetBadge(string code, out Badge badge)
        {
            return _badges.TryGetValue(code, out badge);
        }

        public bool HasBadge(string badge)
        {
            return _badges.ContainsKey(badge);
        }

        public void GiveBadge(string code, bool inDatabase, GameClient session)
        {
            if (HasBadge(code))
                return;

            if (!PlusEnvironment.GetGame().GetBadgeManager().TryGetBadge(code.ToUpper(), out BadgeDefinition badge) || badge.RequiredRight.Length > 0 && !session.GetHabbo().GetPermissions().HasRight(badge.RequiredRight))
                return;

            if (inDatabase) {
                using (WavePlusContext db = PlusEnvironment.GetDbContext())
                    db.user_badges.Upsert(new user_badge
                    {
                        user_id = (uint)_player.Id,
                        badge_id = code,
                        badge_slot = 0
                    }).On(b => new { b.user_id, b.badge_id }).Run();
            }

            _badges.Add(code, new Badge(code, 0));

            if (session != null) {
                session.SendPacket(new BadgesComposer(session.GetHabbo().GetBadgeComponent().GetBadges()));
                session.SendPacket(new FurniListNotificationComposer(1, 4));
            }
        }

        public void ResetSlots()
        {
            foreach (Badge badge in _badges.Values)
            {
                badge.Slot = 0;
            }
        }

        public void RemoveBadge(string badge)
        {
            if (!HasBadge(badge)) {
                return;
            }

            uint uid = (uint)_player.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.user_badges.Where(b => b.badge_id == badge && b.user_id == uid).ExecuteDelete();

            if (_badges.ContainsKey(badge))
                _badges.Remove(badge);
        }
    }
}