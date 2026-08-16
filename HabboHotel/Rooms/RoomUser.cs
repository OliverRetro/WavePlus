using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Interactor;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay;
using Plus.HabboHotel.Rooms.Games.Freeze;
using Plus.HabboHotel.Rooms.Games.Teams;
using Plus.HabboHotel.Rooms.PathFinding;

namespace Plus.HabboHotel.Rooms
{
    public class RoomUser(int habboId, int roomId, int virtualId, Room room)
    {
        public bool AllowOverride = false;
        public BotAI BotAI;
        public RoomBot BotData;
        public bool CanWalk = true;
        public int CarryItemId; //byte
        public int CarryTimer; //byte
        // Countdown (in room cycles) after a gesture/action temporarily cleared the avatar effect;
        // when it hits zero the stored CurrentEffect is re-applied. 0 = nothing pending.
        public int EffectReapplyTimer;
        public int ChatSpamCount;
        public int ChatSpamTicks = 16;
        public ItemEffectType CurrentItemEffect = ItemEffectType.None;
        public int DanceId;
        public bool FastWalking = false;
        public bool SuperFastWalking = false;

        /// <summary>Walk speed as a plain multiplier on the normal pace. 1.0 is a regular avatar,
        /// 2.5 is fastwalk, 4.0 is superfastwalk — none of them skip tiles, they just cover them
        /// faster. Set this directly to hand out any other tier (a sprint, a limp, a debuff).
        /// 0 means "no override, use the fastwalk flags".</summary>
        public double WalkSpeedOverride = 0d;

        // Last tier the room was told about. Clients assume 1.0 for an avatar they have heard
        // nothing about, so this starts matching that assumption.
        public double LastSentWalkSpeed = 1.0;
        public int FreezeCounter;
        public int FreezeLives = 0;
        public bool Freezed = false;
        public bool Frozen;
        public int GateId = 0;

        public int GoalX; //byte
        public int GoalY; //byte
        public int HabboId = habboId;
        public int HorseId = 0;
        public int IdleTime = 0; //byte
        public bool InteractingGate = false;
        public int InternalRoomId = 0;
        public bool IsAsleep;
        public bool IsWalking;
        public int LastBubble = 0;
        public double LastInteraction = 0;
        public Item LastItem = null;
        public int LockedTilesCount = 0;

        // Remaining route, ordered [goal, ..., start]. Every node carries the height the
        // pathfinder picked for that tile, which with 3D pathing is not necessarily the top of it.
        public List<Vector3D> Path = new();

        // Set from IO threads (a click) and consumed by the room thread, hence volatile: it is the
        // release that publishes the GoalX/GoalY written just before it.
        public volatile bool PathRecalcNeeded;
        public int PathStep = 1;

        // Earliest time (monotonic ms, see PlusEnvironment.MonotonicMs) this user may take their
        // next walking step. The room's movement loop runs every 50ms so clicks register almost
        // instantly, but actual steps stay on the classic 500ms-per-tile cadence via this gate.
        public long NextStepMs;

        // Whether the step in flight already replaced a cancelled one. Reset on every real arrival,
        // so a click can turn the avatar around at most once per tile and a stream of clicks can
        // never keep it pinned on one square. See RoomUserManager.CanAbortStep.
        public bool StepAborted;

        // Tile the avatar stepped off, and the moment (unix ms) the current step was committed.
        // The client animates X,Y -> SetX,SetY linearly over one step interval, so together with
        // SetX/SetY these let combat reconstruct where the avatar actually WAS ON SCREEN at any
        // point in the last two steps, instead of guessing from whole-tile snapshots.
        // See RpProximity.IsWithinWeaponRange.
        public int PrevX;
        public int PrevY;
        public double PrevZ;
        public long StepCommittedMs;

        public Pet PetData;

        public int PrevTime = 0;
        public bool RidingHorse = false;
        public int RoomId = roomId;
        public int RotBody = 0; //byte
        public int RotHead = 0; //byte

        public bool SetStep;
        public int SetX; //byte
        public int SetY; //byte
        public double SetZ;
        public double SignTime;
        public byte SqState = 3;
        public int TeleDelay = -1; //byte
        public bool TeleportEnabled;
        public bool UpdateNeeded = true;
        public int VirtualId = virtualId;

        public int X = 0; //byte
        public int Y = 0; //byte
        public double Z = 0;

        public FreezePowerUp BanzaiPowerUp;
        public bool IsLying = false;
        public bool IsSitting = false;
        private GameClient _mClient;
        private Room _mRoom = room;
        public bool MoonwalkEnabled = false;
        public bool ShieldActive;
        public int ShieldCounter;
        public Team Team;
        public bool FreezeInteracting;
        public int UserId;
        public bool IsJumping = false;

        public bool IsRolling = false;
        public int RollerDelay = 0;

        public int LLPartner = 0;
        public double TimeInRoom = 0;
        public DateTime TimedFreezeUntilUtc;

        private int _timedFreezeId;
        private int _timedFreezePreviousEffect;

        private int _itemConsumptionId;
        private int _fastwalkId;

        /// <summary>Normal avatar pace — one tile per <see cref="RoomUserManager.StepIntervalMs"/>.</summary>
        public const double WalkSpeedNormal = 1.0;

        /// <summary>Fastwalk tier.</summary>
        public const double WalkSpeedFast = 2.5;

        /// <summary>Superfastwalk tier.</summary>
        public const double WalkSpeedSuperFast = 4.0;

        /// <summary>Slowest a tier may make a step, so a bad multiplier can't stall a walk.</summary>
        private const int MaxStepDurationMs = 4000;

        public Point Coordinate => new(X, Y);

        /// <summary>
        /// Tiered walk speed. Tiers scale how long a tile takes, they never skip tiles: the avatar
        /// walks every square of its path, just quicker.
        /// </summary>
        public double WalkSpeed
        {
            get
            {
                if (WalkSpeedOverride > 0d)
                    return WalkSpeedOverride;

                if (SuperFastWalking)
                    return WalkSpeedSuperFast;

                return FastWalking ? WalkSpeedFast : WalkSpeedNormal;
            }
        }

        /// <summary>
        /// How long this user's next tile takes, in milliseconds — rounded up to a whole number of
        /// room ticks.
        ///
        /// The rounding is not cosmetic. A step commits when a room tick finds
        /// <see cref="NextStepMs"/> due, so the shortest cadence the server can actually deliver
        /// is a multiple of <see cref="RoomManager.CycleIntervalMs"/>. Leave the raw value in and
        /// the two disagree on every tile: superfastwalk asks for 125ms, the ticks deliver
        /// 100/150/100/150, and the client — which is told to animate exactly this number — spends
        /// every other tile either frozen early or overrunning. Ask for what the grid can give and
        /// the delivered cadence matches the animation by construction.
        /// </summary>
        public int StepDurationMs
        {
            get
            {
                double speed = WalkSpeed;
                if (speed <= 0d)
                    speed = WalkSpeedNormal;

                int duration = Math.Clamp((int)Math.Round(RoomUserManager.StepIntervalMs / speed), RoomUserManager.MinStepIntervalMs, MaxStepDurationMs);

                int remainder = duration % RoomManager.CycleIntervalMs;
                if (remainder != 0)
                    duration += RoomManager.CycleIntervalMs - remainder;

                return Math.Min(duration, MaxStepDurationMs);
            }
        }

        /// <summary>
        /// How long the client should take to slide the avatar across a tile. Deliberately equal
        /// to the step interval — the tick-rounded one, so the client animates the cadence the
        /// server can really deliver — and sent explicitly so the client never has to re-derive it
        /// from a speed multiplier.
        ///
        /// Padding this to absorb a late step does not work: the renderer interpolates from where
        /// the avatar currently is, so an animation longer than the real cadence never completes a
        /// tile and the shortfall compounds into a visible drift. Step lateness is fixed where it
        /// happens instead — the room tick and <see cref="NextStepMs"/> both schedule from their
        /// target rather than from "now".
        /// </summary>
        public int StepAnimationMs => StepDurationMs;

        public bool IsPet => IsBot && BotData.IsPet;

        public int CurrentEffect => GetClient().GetHabbo().Effects().CurrentEffect;

        public bool IsDancing => DanceId >= 1;

        public bool IsTrading { get; set; } = false;

        public int TradePartner { get; set; } = 0;

        public int TradeId { get; set; } = 0;

        public Dictionary<string, string> Statusses { get; } = [];

        public string SerializedStatus { get; set; }

        public bool NeedsAutoKick
        {
            get
            {
                if (IsBot)
                    return false;

                if (GetClient() == null || GetClient().GetHabbo() == null)
                    return true;

                if (GetClient().GetHabbo().GetPermissions().HasRight("mod_tool") || GetRoom().OwnerId == HabboId)
                    return false;

                return false;
            }
        }

        public bool IsBot => BotData != null;

        public bool IsMovementFrozen => Frozen || TimedFreezeUntilUtc > DateTime.UtcNow || (GetClient()?.GetHabbo()?.GetRpStats()?.IsDead ?? false);

        public void Lay()
        {
            if (!HasStatus("lay")) {
                if (RotBody % 2 == 0) {
                    try {
                        Statusses.Add("lay", "1.0 null");
                        Z -= 0.35;
                        IsLying = true;
                        UpdateNeeded = true;
                    } catch {
                    }
                } else {
                    RotBody--;
                    Statusses.Add("lay", "1.0 null");
                    Z -= 0.35;
                    IsLying = true;
                    UpdateNeeded = true;
                }
            } else {
                Z += 0.35;
                Statusses.Remove("lay");
                Statusses.Remove("1.0");
                IsLying = false;
                UpdateNeeded = true;
            }
        }

        public string GetUsername()
        {
            if (IsBot)
                return string.Empty;

            if (GetClient() != null) {
                if (GetClient().GetHabbo() != null) {
                    return GetClient().GetHabbo().Username;
                }

                return PlusEnvironment.GetUsernameById(HabboId);
            }

            return PlusEnvironment.GetUsernameById(HabboId);
        }

        public void UnIdle()
        {
            if (!IsBot) {
                if (GetClient() != null && GetClient().GetHabbo() != null)
                    GetClient().GetHabbo().TimeAfk = 0;
            }

            IdleTime = 0;

            if (IsAsleep) {
                IsAsleep = false;
                GetRoom().SendPacket(new SleepComposer(VirtualId, false));
            }
        }

        public void Dispose()
        {
            Statusses.Clear();
            _mRoom = null;
            _mClient = null;
        }

        public void Chat(string message, int colour = 0)
        {
            if (GetRoom() == null)
                return;

            if (!IsBot)
                return;

            if (IsPet) {
                foreach (RoomUser user in GetRoom().GetRoomUserManager().GetUserList().ToList()) {
                    if (user == null || user.IsBot)
                        continue;

                    if (user.GetClient() == null || user.GetClient().GetHabbo() == null)
                        return;

                    if (!user.GetClient().GetHabbo().AllowPetSpeech)
                        user.GetClient().SendPacket(new ChatComposer(VirtualId, message, 0, 0));
                }
            } else {
                foreach (RoomUser user in GetRoom().GetRoomUserManager().GetUserList().ToList()) {
                    if (user == null || user.IsBot)
                        continue;

                    if (user.GetClient() == null || user.GetClient().GetHabbo() == null)
                        return;

                    if (!user.GetClient().GetHabbo().AllowBotSpeech)
                        user.GetClient().SendPacket(new ChatComposer(VirtualId, message, 0, (colour == 0 ? 2 : colour)));
                }
            }
        }

        public void HandleSpamTicks()
        {
            if (ChatSpamTicks >= 0) {
                ChatSpamTicks--;

                if (ChatSpamTicks == -1) {
                    ChatSpamCount = 0;
                }
            }
        }

        public bool IncrementAndCheckFlood(out int muteTime)
        {
            muteTime = 0;

            ChatSpamCount++;
            if (ChatSpamTicks == -1)
                ChatSpamTicks = 8;
            else if (ChatSpamCount >= 6) {
                if (GetClient().GetHabbo().GetPermissions().HasRight("events_staff"))
                    muteTime = 3;
                else if (GetClient().GetHabbo().GetPermissions().HasRight("gold_vip"))
                    muteTime = 7;
                else if (GetClient().GetHabbo().GetPermissions().HasRight("silver_vip"))
                    muteTime = 10;
                else
                    muteTime = 20;

                GetClient().GetHabbo().FloodTime = PlusEnvironment.GetUnixTimestamp() + muteTime;

                ChatSpamCount = 0;
                return true;
            }

            return false;
        }

        public void OnChat(int colour, string message, bool shout)
        {
            if (GetClient() == null || GetClient().GetHabbo() == null || _mRoom == null)
                return;

            if (_mRoom.GetWired().TriggerEvent(WiredBoxType.TriggerUserSays, GetClient().GetHabbo(), message))
                return;

            GetClient().GetHabbo().HasSpoken = true;

            if (_mRoom.WordFilterList.Count > 0 && !GetClient().GetHabbo().GetPermissions().HasRight("word_filter_override")) {
                message = _mRoom.GetFilter().CheckMessage(message);
            }

            MessageComposer packet;
            if (shout)
                packet = new ShoutComposer(VirtualId, message, PlusEnvironment.GetGame().GetChatManager().GetEmotions().GetEmotionsForText(message), colour);
            else
                packet = new ChatComposer(VirtualId, message, PlusEnvironment.GetGame().GetChatManager().GetEmotions().GetEmotionsForText(message), colour);

            if (GetClient().GetHabbo().TentId > 0) {
                _mRoom.SendToTent(GetClient().GetHabbo().Id, GetClient().GetHabbo().TentId, packet);

                packet = new WhisperComposer(VirtualId, "[Tent Chat] " + message, 0, colour);

                List<RoomUser> toNotify = _mRoom.GetRoomUserManager().GetRoomUserByRank(2);

                if (toNotify.Count > 0) {
                    foreach (RoomUser user in toNotify) {
                        if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null ||
                            user.GetClient().GetHabbo().TentId == GetClient().GetHabbo().TentId) {
                            continue;
                        }

                        user.GetClient().SendPacket(packet);
                    }
                }
            } else {
                foreach (RoomUser user in _mRoom.GetRoomUserManager().GetRoomUsers().ToList()) {
                    if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().GetIgnores().IgnoredUserIds().Contains(_mClient.GetHabbo().Id))
                        continue;

                    if (_mRoom.ChatDistance > 0 && Gamemap.TileDistance(X, Y, user.X, user.Y) > _mRoom.ChatDistance)
                        continue;

                    user.GetClient().SendPacket(packet);
                }
            }

            #region Pets/Bots responces

            if (shout) {
                foreach (RoomUser user in _mRoom.GetRoomUserManager().GetUserList().ToList()) {
                    if (!user.IsBot)
                        continue;

                    if (user.IsBot)
                        user.BotAI.OnUserShout(this, message);
                }
            } else {
                foreach (RoomUser user in _mRoom.GetRoomUserManager().GetUserList().ToList()) {
                    if (!user.IsBot)
                        continue;

                    if (user.IsBot)
                        user.BotAI.OnUserSay(this, message);
                }
            }

            #endregion
        }

        public void ClearMovement(bool update)
        {
            IsWalking = false;
            NextStepMs = 0;
            Statusses.Remove("mv");
            GoalX = 0;
            GoalY = 0;
            PathRecalcNeeded = false;
            Path.Clear();
            PathStep = 1;
            SetStep = false;
            StepAborted = false;
            SetX = 0;
            SetY = 0;
            SetZ = 0;

            if (update) {
                UpdateNeeded = true;
            }
        }

        public void MoveTo(Point c)
        {
            MoveTo(c.X, c.Y);
        }

        public void MoveTo(int pX, int pY, bool pOverride)
        {
            Gamemap map = GetRoom()?.GetGameMap();
            if (map != null && map.TryGetBedRestTile(pX, pY, out Point bedTile) && !map.SquareHasUsers(bedTile.X, bedTile.Y, this)) {
                pX = bedTile.X;
                pY = bedTile.Y;
            }

            if (!IsBot && GetClient()?.GetHabbo() != null && (X != pX || Y != pY)) {
                InteractorArrowTeleport.CancelDelayedTeleport(GetClient().GetHabbo().Id);
                PassiveModeCommand.CancelPendingToggle(GetClient());
                PlusEnvironment.GetTrashSearchManager()?.CancelForMovement(GetClient().GetHabbo().Id);
            }

            if (TeleportEnabled) {
                UnIdle();
                GetRoom().SendPacket(GetRoom().GetRoomItemHandler().UpdateUserOnRoller(this, new Point(pX, pY), 0, GetRoom().GetGameMap().SqAbsoluteHeight(GoalX, GoalY)));
                if (Statusses.ContainsKey("sit"))
                    Z -= 0.35;
                UpdateNeeded = true;
                return;
            }

            if (!IsBot && GetClient()?.GetHabbo() != null && PlusEnvironment.GetHospitalManager().IsRestrictedHospitalBedTile(GetRoom(), GetClient().GetHabbo(), pX, pY)) {
                GetClient().SendWhisper("You aren't in critical condition!", 1);
                ClearMovement(true);
                return;
            }

            // Only refuse an occupied destination when room blocking is active. When the room
            // allows walk-through (RoomBlockingEnabled != 0) users may target/stand on the same tile.
            // Excluding ourselves matters for the one-tile reversal: mid-step we are still
            // registered on the tile we're walking away from, so a plain occupancy check would
            // refuse the click to walk back onto it.
            if ((GetRoom().GetGameMap().SquareHasUsers(pX, pY, this) && !pOverride && GetRoom().RoomBlockingEnabled == 0) || IsMovementFrozen)
                return;

            UnIdle();

            GoalX = pX;
            GoalY = pY;
            FreezeInteracting = false;

            // Written last: the room thread acts on this flag, and it must never see a new goal
            // flagged for planning before both coordinates are in place.
            PathRecalcNeeded = true;
        }

        public void MoveTo(int pX, int pY)
        {
            MoveTo(pX, pY, false);
        }

        public void UnlockWalking()
        {
            AllowOverride = false;
            CanWalk = true;
        }

        public void ApplyTimedFreeze(TimeSpan duration, int effectId)
        {
            bool isAlreadyTimedFrozen = TimedFreezeUntilUtc > DateTime.UtcNow;

            if (!isAlreadyTimedFrozen)
                _timedFreezePreviousEffect = GetClient()?.GetHabbo()?.Effects()?.CurrentEffect ?? 0;

            TimedFreezeUntilUtc = DateTime.UtcNow.Add(duration);

            if (IsWalking || Statusses.ContainsKey("mv")) {
                ClearMovement(true);
                GetRoom()?.GetRoomUserManager().UpdateUserStatus(this, false);
            }

            ApplyEffect(effectId);

            int freezeId = ++_timedFreezeId;
            _ = ClearTimedFreezeAsync(freezeId, duration);
        }

        private async Task ClearTimedFreezeAsync(int freezeId, TimeSpan duration)
        {
            await Task.Delay(duration);

            if (_timedFreezeId == freezeId) {
                TimedFreezeUntilUtc = DateTime.MinValue;

                ApplyEffect(_timedFreezePreviousEffect);
            }
        }

        public void ApplyTimedItemConsumption(TimeSpan duration, int effectId, int handitemId)
        {
            int cachedEffect = GetClient()?.GetHabbo()?.Effects()?.CurrentEffect ?? 0;
            int cachedHanditem = CarryItemId;

            ApplyEffect(effectId);
            CarryItem(handitemId);

            int consumptionId = ++_itemConsumptionId;
            _ = ClearItemConsumptionAsync(consumptionId, duration, cachedEffect, cachedHanditem);
        }

        private async Task ClearItemConsumptionAsync(int consumptionId, TimeSpan duration, int cachedEffect, int cachedHanditem)
        {
            await Task.Delay(duration);

            // Only restore if no newer consumption has happened in the meantime.
            if (_itemConsumptionId != consumptionId)
                return;

            ApplyEffect(cachedEffect);
            CarryItem(cachedHanditem);
        }

        public void ApplyTimedFastwalk(TimeSpan duration)
        {
            bool previousFastWalking = FastWalking;
            FastWalking = true;

            int fastwalkId = ++_fastwalkId;
            _ = ClearTimedFastwalkAsync(fastwalkId, duration, previousFastWalking);
        }

        private async Task ClearTimedFastwalkAsync(int fastwalkId, TimeSpan duration, bool previousFastWalking)
        {
            await Task.Delay(duration);

            if (_fastwalkId != fastwalkId)
                return;

            FastWalking = previousFastWalking;
        }

        public void SetPos(int pX, int pY, double pZ)
        {
            // A teleport/placement isn't a walked step — drop the interpolation history so combat
            // never lerps a hitbox across the jump.
            PrevX = pX;
            PrevY = pY;
            PrevZ = pZ;
            StepCommittedMs = 0;
            StepAborted = false;

            X = pX;
            Y = pY;
            Z = pZ;
        }

        public void CarryItem(int item)
        {
            CarryItemId = item;

            if (item > 0)
                CarryTimer = 240;
            else
                CarryTimer = 0;

            GetRoom().SendPacket(new CarryObjectComposer(VirtualId, item));
        }

        public void SetRot(int rotation, bool headOnly)
        {
            if (Statusses.ContainsKey("lay") || IsWalking) {
                return;
            }

            int diff = RotBody - rotation;

            RotHead = RotBody;

            if (Statusses.ContainsKey("sit") || headOnly) {
                if (RotBody == 2 || RotBody == 4) {
                    if (diff > 0) {
                        RotHead = RotBody - 1;
                    } else if (diff < 0) {
                        RotHead = RotBody + 1;
                    }
                } else if (RotBody == 0 || RotBody == 6) {
                    if (diff > 0) {
                        RotHead = RotBody - 1;
                    } else if (diff < 0) {
                        RotHead = RotBody + 1;
                    }
                }
            } else if (diff <= -2 || diff >= 2) {
                RotHead = rotation;
                RotBody = rotation;
            } else {
                RotHead = rotation;
            }

            UpdateNeeded = true;
        }

        public bool HasStatus(string key)
        {
            return Statusses.ContainsKey(key);
        }

        public void RemoveStatus(string key)
        {
            if (HasStatus(key))
                Statusses.Remove(key);
        }

        public void SetStatus(string key, string value = "")
        {
            if (Statusses.ContainsKey(key)) {
                Statusses[key] = value;
            } else {
                Statusses.Add(key, value);
            }
        }

        public void ApplyEffect(int effectId)
        {
            if (IsBot) {
                _mRoom.SendPacket(new AvatarEffectComposer(VirtualId, effectId));
                return;
            }

            if (IsBot || GetClient() == null || GetClient().GetHabbo() == null || GetClient().GetHabbo().Effects() == null)
                return;

            GetClient().GetHabbo().Effects().ApplyEffect(effectId);
        }

        public Point SquareInFront
        {
            get
            {
                var sq = new Point(X, Y);

                if (RotBody == 0) {
                    sq.Y--;
                } else if (RotBody == 2) {
                    sq.X++;
                } else if (RotBody == 4) {
                    sq.Y++;
                } else if (RotBody == 6) {
                    sq.X--;
                }

                return sq;
            }
        }

        public Point SquareBehind
        {
            get
            {
                var sq = new Point(X, Y);

                if (RotBody == 0) {
                    sq.Y++;
                } else if (RotBody == 2) {
                    sq.X--;
                } else if (RotBody == 4) {
                    sq.Y--;
                } else if (RotBody == 6) {
                    sq.X++;
                }

                return sq;
            }
        }

        public Point SquareLeft
        {
            get
            {
                var sq = new Point(X, Y);

                if (RotBody == 0) {
                    sq.X++;
                } else if (RotBody == 2) {
                    sq.Y--;
                } else if (RotBody == 4) {
                    sq.X--;
                } else if (RotBody == 6) {
                    sq.Y++;
                }

                return sq;
            }
        }

        public Point SquareRight
        {
            get
            {
                var sq = new Point(X, Y);

                if (RotBody == 0) {
                    sq.X--;
                } else if (RotBody == 2) {
                    sq.Y++;
                } else if (RotBody == 4) {
                    sq.X++;
                } else if (RotBody == 6) {
                    sq.Y--;
                }

                return sq;
            }
        }

        public GameClient GetClient()
        {
            if (IsBot) {
                return null;
            }

            if (_mClient == null)
                _mClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(HabboId);
            return _mClient;
        }

        public Room GetRoom()
        {
            if (_mRoom == null)
                if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(RoomId, out _mRoom))
                    return _mRoom;

            return _mRoom;
        }
    }
}