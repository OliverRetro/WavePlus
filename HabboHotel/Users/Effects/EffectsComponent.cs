using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Database.EF;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Users.Effects
{
    public sealed class EffectsComponent
    {
        private Habbo _habbo;

        private readonly ConcurrentDictionary<int, AvatarEffect> _effects = new();

        public bool Init(Habbo habbo)
        {
            if (_effects.Count > 0)
                return false;

            uint uid = (uint)habbo.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var effects = db.UserEffects
                    .Where(e => e.UserId == uid)
                    .Select(e => new { e.Id, e.UserId, e.EffectId, e.TotalDuration, e.IsActivated, e.ActivatedStamp, e.Quantity })
                    .ToList();

                foreach (var row in effects) {
                    if (_effects.TryAdd(row.Id, new AvatarEffect(row.Id, (int)(row.UserId ?? 0), row.EffectId ?? 0, row.TotalDuration ?? 0, PlusEnvironment.EnumToBool(row.IsActivated), row.ActivatedStamp ?? 0, row.Quantity ?? 0))) {
                        //umm?
                    }
                }
            }

            _habbo = habbo;
            CurrentEffect = 0;
            return true;
        }

        public bool TryAdd(AvatarEffect effect)
        {
            return _effects.TryAdd(effect.Id, effect);
        }

        public bool HasEffect(int spriteId, bool activatedOnly = false, bool unactivatedOnly = false)
        {
            return (GetEffectNullable(spriteId, activatedOnly, unactivatedOnly) != null);
        }

        public AvatarEffect GetEffectNullable(int spriteId, bool activatedOnly = false, bool unactivatedOnly = false)
        {
            foreach (AvatarEffect effect in _effects.Values.ToList()) {
                if (!effect.HasExpired && effect.SpriteId == spriteId && (!activatedOnly || effect.Activated) && (!unactivatedOnly || !effect.Activated)) {
                    return effect;
                }
            }

            return null;
        }

        public void CheckEffectExpiry(Habbo habbo)
        {
            foreach (AvatarEffect effect in _effects.Values.ToList()) {
                if (effect.HasExpired) {
                    effect.HandleExpiration(habbo);
                }
            }
        }

        public void ApplyEffect(int effectId)
        {
            if (_habbo == null || _habbo.CurrentRoom == null)
                return;

            RoomUser user = _habbo.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(_habbo.Id);
            if (user == null)
                return;

            CurrentEffect = effectId;

            if (user.IsDancing)
                _habbo.CurrentRoom.SendPacket(new DanceComposer(user.VirtualId, 0));
            _habbo.CurrentRoom.SendPacket(new AvatarEffectComposer(user.VirtualId, effectId));
        }

        public ICollection<AvatarEffect> GetAllEffects => _effects.Values;

        public int CurrentEffect { get; set; }

        public void Dispose()
        {
            _effects.Clear();
        }
    }
}