using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Core;
using Plus.HabboHotel.Items.Interactor;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Games.Freeze;
using Plus.HabboHotel.Rooms.Games.Teams;
using Plus.HabboHotel.Rooms.PathFinding;
using Plus.Utilities;

namespace Plus.HabboHotel.Items
{
    public class Item
    {
        public int Id;
        public int BaseItem;
        public string ExtraData;
        public string Figure;
        public string Gender;
        public int GroupId;
        public int InteractingUser;
        public int InteractingUser2;
        public int LimitedNo;
        public int LimitedTot;
        public bool MagicRemove = false;
        public int RoomId;
        public int Rotation;
        public int UpdateCounter;
        public int UserId;
        public string Username;
        public int InteractingBallUser;
        public byte InteractionCount;
        public byte InteractionCountHelper;

        public Team Team;
        public bool PendingReset = false;
        public FreezePowerUp FreezePowerUp;

        public int Value;
        public string WallCoord;
        private bool _updateNeeded;

        private Room _room;
        private static readonly Random _random = new();

        public Item(int id, int roomId, int baseItem, string extraData, int x, int y, double z, int rot, int userId, int group, int limitedNumber, int limitedStack, string wallCoord, Room room = null)
        {
            if (PlusEnvironment.GetGame().GetItemManager().GetItem(baseItem, out ItemData data)) {
                Id = id;
                RoomId = roomId;
                _room = room;
                Data = data;
                BaseItem = baseItem;
                ExtraData = extraData;
                GroupId = group;

                GetX = x;
                GetY = y;
                if (!double.IsInfinity(z))
                    GetZ = z;
                Rotation = rot;
                UpdateNeeded = false;
                UpdateCounter = 0;
                InteractingUser = 0;
                InteractingUser2 = 0;
                InteractingBallUser = 0;
                InteractionCount = 0;
                Value = 0;

                UserId = userId;
                Username = PlusEnvironment.GetUsernameById(userId);

                LimitedNo = limitedNumber;
                LimitedTot = limitedStack;

                switch (GetBaseItem().InteractionType) {
                    case InteractionType.Teleport:
                        RequestUpdate(0, true);
                        break;

                    case InteractionType.Arrow:
                        RequestUpdate(0, true);
                        break;

                    case InteractionType.Hopper:
                        RequestUpdate(0, true);
                        break;

                    case InteractionType.Roller:
                        IsRoller = true;
                        if (roomId > 0) {
                            GetRoom().GetRoomItemHandler().GotRollers = true;
                        }

                        break;

                    case InteractionType.BanzaiScoreBlue:
                    case InteractionType.FootballCounterBlue:
                    case InteractionType.BanzaiGateBlue:
                    case InteractionType.FreezeBlueGate:
                    case InteractionType.FreezeBlueCounter:
                        Team = Team.Blue;
                        break;

                    case InteractionType.BanzaiScoreGreen:
                    case InteractionType.FootballCounterGreen:
                    case InteractionType.BanzaiGateGreen:
                    case InteractionType.FreezeGreenCounter:
                    case InteractionType.FreezeGreenGate:
                        Team = Team.Green;
                        break;

                    case InteractionType.BanzaiScoreRed:
                    case InteractionType.FootballCounterRed:
                    case InteractionType.BanzaiGateRed:
                    case InteractionType.FreezeRedCounter:
                    case InteractionType.FreezeRedGate:
                        Team = Team.Red;
                        break;

                    case InteractionType.BanzaiScoreYellow:
                    case InteractionType.FootballCounterYellow:
                    case InteractionType.BanzaiGateYellow:
                    case InteractionType.FreezeYellowCounter:
                    case InteractionType.FreezeYellowGate:
                        Team = Team.Yellow;
                        break;

                    case InteractionType.BanzaiTele: {
                            ExtraData = "";
                            break;
                        }
                }

                IsWallItem = (GetBaseItem().Type.ToString().ToLower() == "i");
                IsFloorItem = (GetBaseItem().Type.ToString().ToLower() == "s");

                if (IsFloorItem) {
                    GetAffectedTiles = Gamemap.GetAffectedTiles(GetBaseItem().Length, GetBaseItem().Width, GetX, GetY, rot);
                } else if (IsWallItem) {
                    WallCoord = wallCoord;
                    IsWallItem = true;
                    IsFloorItem = false;
                    GetAffectedTiles = [];
                }
            }
        }

        public ItemData Data { get; set; }

        public Dictionary<int, ThreeDCoord> GetAffectedTiles { get; private set; }

        public int GetX { get; set; }

        public int GetY { get; set; }

        public double GetZ { get; set; }

        public bool UpdateNeeded
        {
            get => _updateNeeded;
            set
            {
                if (value && GetRoom() != null)
                    GetRoom().GetRoomItemHandler().QueueRoomItemUpdate(this);
                _updateNeeded = value;
            }
        }

        public bool IsRoller { get; }

        public Point Coordinate => new(GetX, GetY);

        public List<Point> GetCoords
        {
            get
            {
                var toReturn = new List<Point>
                {
                    Coordinate
                };

                foreach (ThreeDCoord tile in GetAffectedTiles.Values) {
                    toReturn.Add(new Point(tile.X, tile.Y));
                }

                return toReturn;
            }
        }

        public List<Point> GetSides()
        {
            var sides = new List<Point>
            {
                SquareBehind,
                SquareInFront,
                SquareLeft,
                SquareRight,
                Coordinate
            };
            return sides;
        }

        /// <summary>
        /// The height a user is at when they are actually on this item: the seat's own base for
        /// something you sit or lie in, otherwise the surface you stand on top of.
        ///
        /// Not the same as being anywhere within the item's vertical extent. A tall platform is
        /// open underneath, and someone walking beneath it is not touching it — so they must not
        /// be sat down by it, teleported by it, or have its effects applied.
        /// </summary>
        public double ContactHeight
        {
            get
            {
                ItemData data = GetBaseItem();

                if (data != null && (data.IsSeat || data.InteractionType == InteractionType.Bed || data.InteractionType == InteractionType.TentSmall))
                    return GetZ;

                return TotalHeight;
            }
        }

        /// <summary>Whether a user at <paramref name="userZ"/> is standing on or in this item.</summary>
        public bool IsAtUserLevel(double userZ, double tolerance = 0.5)
        {
            return Math.Abs(userZ - ContactHeight) <= tolerance;
        }

        public int HeightState => int.TryParse(ExtraData, out int state) ? state : 0;

        public double EffectiveHeight
        {
            get
            {
                ItemData data = GetBaseItem();

                if (data == null)
                    return 0.0;

                return data.HasAdjustableHeights ? data.HeightForState(HeightState) : data.Height;
            }
        }

        public int StateCount
        {
            get
            {
                ItemData data = GetBaseItem();

                if (data == null)
                    return 0;

                return data.HasAdjustableHeights ? Math.Max(data.Modes, data.AdjustableHeights.Count) : data.Modes;
            }
        }

        public double TotalHeight => GetZ + EffectiveHeight;

        public bool IsWallItem { get; }

        public bool IsFloorItem { get; }

        public Point SquareInFront
        {
            get
            {
                var sq = new Point(GetX, GetY);

                if (Rotation == 0) {
                    sq.Y--;
                } else if (Rotation == 2) {
                    sq.X++;
                } else if (Rotation == 4) {
                    sq.Y++;
                } else if (Rotation == 6) {
                    sq.X--;
                }

                return sq;
            }
        }

        public Point SquareBehind
        {
            get
            {
                var sq = new Point(GetX, GetY);

                if (Rotation == 0) {
                    sq.Y++;
                } else if (Rotation == 2) {
                    sq.X--;
                } else if (Rotation == 4) {
                    sq.Y--;
                } else if (Rotation == 6) {
                    sq.X++;
                }

                return sq;
            }
        }

        public Point SquareLeft
        {
            get
            {
                var sq = new Point(GetX, GetY);

                if (Rotation == 0) {
                    sq.X++;
                } else if (Rotation == 2) {
                    sq.Y--;
                } else if (Rotation == 4) {
                    sq.X--;
                } else if (Rotation == 6) {
                    sq.Y++;
                }

                return sq;
            }
        }

        public Point SquareRight
        {
            get
            {
                var sq = new Point(GetX, GetY);

                if (Rotation == 0) {
                    sq.X--;
                } else if (Rotation == 2) {
                    sq.Y++;
                } else if (Rotation == 4) {
                    sq.X++;
                } else if (Rotation == 6) {
                    sq.Y--;
                }

                return sq;
            }
        }

        public IFurniInteractor Interactor
        {
            get
            {
                if (IsWired) {
                    return new InteractorWired();
                }

                if (GetBaseItem().InteractionType == InteractionType.None && GetBaseItem().HasAdjustableHeights) {
                    return new InteractorMultiHeight();
                }

                switch (GetBaseItem().InteractionType) {
                    case InteractionType.Gate:
                        return new InteractorGate();

                    case InteractionType.Teleport:
                        return new InteractorTeleport();

                    case InteractionType.Arrow:
                        return new InteractorArrowTeleport();

                    case InteractionType.Hopper:
                        return new InteractorHopper();

                    case InteractionType.Bottle:
                        return new InteractorSpinningBottle();

                    case InteractionType.Dice:
                        return new InteractorDice();

                    case InteractionType.HabboWheel:
                        return new InteractorHabboWheel();

                    case InteractionType.LoveShuffler:
                        return new InteractorLoveShuffler();

                    case InteractionType.OneWayGate:
                        return new InteractorOneWayGate();

                    case InteractionType.Alert:
                        return new InteractorAlert();

                    case InteractionType.VendingMachine:
                        return new InteractorVendor();

                    case InteractionType.Scoreboard:
                        return new InteractorScoreboard();

                    case InteractionType.PuzzleBox:
                        return new InteractorPuzzleBox();

                    case InteractionType.Mannequin:
                        return new InteractorMannequin();

                    case InteractionType.BanzaiCounter:
                        return new InteractorBanzaiTimer();

                    case InteractionType.FreezeTimer:
                        return new InteractorFreezeTimer();

                    case InteractionType.FreezeTileBlock:
                    case InteractionType.FreezeTile:
                        return new InteractorFreezeTile();

                    case InteractionType.FootballCounterBlue:
                    case InteractionType.FootballCounterGreen:
                    case InteractionType.FootballCounterRed:
                    case InteractionType.FootballCounterYellow:
                        return new InteractorScoreCounter();

                    case InteractionType.BanzaiScoreBlue:
                    case InteractionType.BanzaiScoreGreen:
                    case InteractionType.BanzaiScoreRed:
                    case InteractionType.BanzaiScoreYellow:
                        return new InteractorBanzaiScoreCounter();

                    case InteractionType.WfFloorSwitch1:
                    case InteractionType.WfFloorSwitch2:
                        return new InteractorSwitch();

                    case InteractionType.LoveLock:
                        return new InteractorLoveLock();

                    case InteractionType.Cannon:
                        return new InteractorCannon();

                    case InteractionType.Counter:
                        return new InteractorCounter();

                    case InteractionType.Atm:
                        return new InteractorAtm();

                    case InteractionType.Bank:
                        return new InteractorBank();

                    case InteractionType.StockVendor:
                        return new InteractorStockVendor();

                    case InteractionType.RpVendor:
                        return new InteractorRpVendor();

                    case InteractionType.TrashBin:
                        return new InteractorTrashBin();

                    case InteractionType.Rubbish:
                        return new InteractorRubbish();

                    case InteractionType.PressurePlate:
                        return new InteractorPressurePlate();

                    case InteractionType.None:
                    default:
                        return new InteractorGenericSwitch();
                }
            }
        }

        public bool IsWired
        {
            get
            {
                switch (GetBaseItem().InteractionType) {
                    case InteractionType.WiredEffect:
                    case InteractionType.WiredTrigger:
                    case InteractionType.WiredCondition:
                        return true;
                }

                return false;
            }
        }

        public void SetState(int pX, int pY, double pZ, Dictionary<int, ThreeDCoord> tiles)
        {
            GetX = pX;
            GetY = pY;
            if (!double.IsInfinity(pZ)) {
                GetZ = pZ;
            }

            GetAffectedTiles = tiles;
        }

        public void ProcessUpdates()
        {
            try {
                UpdateCounter--;

                if (UpdateCounter <= 0) {
                    UpdateNeeded = false;
                    UpdateCounter = 0;

                    RoomUser user = null;
                    RoomUser user2 = null;

                    switch (GetBaseItem().InteractionType) {
                        #region Group Gates

                        case InteractionType.GuildGate: {
                                if (ExtraData == "1") {
                                    if (GetRoom().GetRoomUserManager().GetUserForSquare(GetX, GetY) == null) {
                                        ExtraData = "0";
                                        UpdateState(false, true);
                                    } else {
                                        RequestUpdate(2, false);
                                    }
                                }

                                break;
                            }

                        #endregion

                        #region Item Effects

                        case InteractionType.Effect: {
                                if (ExtraData == "1") {
                                    if (GetRoom().GetRoomUserManager().GetUserForSquare(GetX, GetY) == null) {
                                        ExtraData = "0";
                                        UpdateState(false, true);
                                    } else {
                                        RequestUpdate(2, false);
                                    }
                                }

                                break;
                            }

                        #endregion

                        #region One way gates

                        case InteractionType.OneWayGate:

                            user = null;

                            if (InteractingUser > 0) {
                                user = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                            }

                            if (user != null && user.X == GetX && user.Y == GetY) {
                                ExtraData = "1";

                                user.MoveTo(SquareBehind);
                                user.InteractingGate = false;
                                user.GateId = 0;
                                RequestUpdate(1, false);
                                UpdateState(false, true);
                            } else if (user != null && user.Coordinate == SquareBehind) {
                                user.UnlockWalking();

                                ExtraData = "0";
                                InteractingUser = 0;
                                user.InteractingGate = false;
                                user.GateId = 0;
                                UpdateState(false, true);
                            } else if (ExtraData == "1") {
                                ExtraData = "0";
                                UpdateState(false, true);
                            }

                            if (user == null) {
                                InteractingUser = 0;
                            }

                            break;

                        #endregion

                        #region VIP Gate

                        case InteractionType.GateVip:

                            user = null;

                            if (InteractingUser > 0) {
                                user = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                            }

                            int newY = 0;
                            int newX = 0;

                            if (user != null && user.X == GetX && user.Y == GetY) {
                                if (user.RotBody == 4) {
                                    newY = 1;
                                } else if (user.RotBody == 0) {
                                    newY = -1;
                                } else if (user.RotBody == 6) {
                                    newX = -1;
                                } else if (user.RotBody == 2) {
                                    newX = 1;
                                }

                                user.MoveTo(user.X + newX, user.Y + newY);
                                RequestUpdate(1, false);
                            } else if (user != null && (user.Coordinate == SquareBehind || user.Coordinate == SquareInFront)) {
                                user.UnlockWalking();

                                ExtraData = "0";
                                InteractingUser = 0;
                                UpdateState(false, true);
                            } else if (ExtraData == "1") {
                                ExtraData = "0";
                                UpdateState(false, true);
                            }

                            if (user == null) {
                                InteractingUser = 0;
                            }

                            break;

                        #endregion

                        #region Hopper

                        case InteractionType.Hopper: {
                                user = null;
                                user2 = null;
                                bool showHopperEffect = false;
                                bool keepDoorOpen = false;
                                int pause = 0;

                                // Do we have a primary user that wants to go somewhere?
                                if (InteractingUser > 0) {
                                    user = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                                    // Is this user okay?
                                    if (user != null) {
                                        // Is he in the tele?
                                        if (user.Coordinate == Coordinate) {
                                            //Remove the user from the square
                                            user.AllowOverride = false;
                                            if (user.TeleDelay == 0) {
                                                int roomHopId = ItemHopperFinder.GetAHopper(user.RoomId);
                                                int nextHopperId = ItemHopperFinder.GetHopperId(roomHopId);

                                                if (!user.IsBot && user.GetClient() != null &&
                                                    user.GetClient().GetHabbo() != null) {
                                                    user.GetClient().GetHabbo().IsHopping = true;
                                                    user.GetClient().GetHabbo().HopperId = nextHopperId;
                                                    user.GetClient().GetHabbo().PrepareRoom(roomHopId, "");
                                                    //User.GetClient().SendMessage(new RoomForwardComposer(RoomHopId));
                                                    InteractingUser = 0;
                                                }
                                            } else {
                                                user.TeleDelay--;
                                                showHopperEffect = true;
                                            }
                                        }
                                        // Is he in front of the tele?
                                        else if (user.Coordinate == SquareInFront) {
                                            user.AllowOverride = true;
                                            keepDoorOpen = true;

                                            // Lock his walking. We're taking control over him. Allow overriding so he can get in the tele.
                                            if (user.IsWalking && (user.GoalX != GetX || user.GoalY != GetY)) {
                                                user.ClearMovement(true);
                                            }

                                            user.CanWalk = false;
                                            user.AllowOverride = true;

                                            // Move into the tele
                                            user.MoveTo(Coordinate.X, Coordinate.Y, true);
                                        }
                                        // Not even near, do nothing and move on for the next user.
                                        else {
                                            InteractingUser = 0;
                                        }
                                    } else {
                                        // Invalid user, do nothing and move on for the next user. 
                                        InteractingUser = 0;
                                    }
                                }

                                if (InteractingUser2 > 0) {
                                    user2 = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser2);

                                    // Is this user okay?
                                    if (user2 != null) {
                                        // If so, open the door, unlock the user's walking, and try to push him out in the right direction. We're done with him!
                                        keepDoorOpen = true;
                                        user2.UnlockWalking();
                                        user2.MoveTo(SquareInFront);
                                    }

                                    // This is a one time thing, whether the user's valid or not.
                                    InteractingUser2 = 0;
                                }

                                // Set the new item state, by priority
                                if (keepDoorOpen) {
                                    if (ExtraData != "1") {
                                        ExtraData = "1";
                                        UpdateState(false, true);
                                    }
                                } else if (showHopperEffect) {
                                    if (ExtraData != "2") {
                                        ExtraData = "2";
                                        UpdateState(false, true);
                                    }
                                } else {
                                    if (ExtraData != "0") {
                                        if (pause == 0) {
                                            ExtraData = "0";
                                            UpdateState(false, true);
                                            pause = 2;
                                        } else {
                                            pause--;
                                        }
                                    }
                                }

                                // We're constantly going!
                                RequestUpdate(1, false);
                                break;
                            }

                        #endregion

                        #region Teleports

                        case InteractionType.Teleport: {
                                user = null;
                                user2 = null;

                                bool keepDoorOpen = false;
                                bool showTeleEffect = false;

                                // Do we have a primary user that wants to go somewhere?
                                if (InteractingUser > 0) {
                                    user = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                                    // Is this user okay?
                                    if (user != null) {
                                        // Is he in the tele?
                                        if (user.Coordinate == Coordinate) {
                                            //Remove the user from the square
                                            user.AllowOverride = false;

                                            if (ItemTeleporterFinder.IsTeleLinked(Id, GetRoom())) {
                                                showTeleEffect = true;

                                                if (true) {
                                                    // Woop! No more delay.
                                                    int teleId = ItemTeleporterFinder.GetLinkedTele(Id);
                                                    int roomId = ItemTeleporterFinder.GetTeleRoomId(teleId, GetRoom());

                                                    // Do we need to tele to the same room or gtf to another?
                                                    if (roomId == RoomId) {
                                                        Item item = GetRoom().GetRoomItemHandler().GetItem(teleId);

                                                        if (item == null) {
                                                            user.UnlockWalking();
                                                        } else {
                                                            // Set pos
                                                            user.SetPos(item.GetX, item.GetY, item.GetZ);
                                                            user.SetRot(item.Rotation, false);

                                                            // Force tele effect update (dirty)
                                                            item.ExtraData = "2";
                                                            item.UpdateState(false, true);

                                                            // Set secondary interacting user
                                                            item.InteractingUser2 = InteractingUser;
                                                            GetRoom().GetGameMap().RemoveUserFromMap(user, new Point(GetX, GetY));

                                                            InteractingUser = 0;
                                                        }
                                                    } else {
                                                        if (user.TeleDelay == 0) {
                                                            // Let's run the teleport delegate to take futher care of this.. WHY DARIO?!
                                                            if (!user.IsBot && user.GetClient() != null &&
                                                                user.GetClient().GetHabbo() != null) {
                                                                user.GetClient().GetHabbo().IsTeleporting = true;
                                                                user.GetClient().GetHabbo().TeleportingRoomId = roomId;
                                                                user.GetClient().GetHabbo().TeleportId = teleId;
                                                                user.GetClient().GetHabbo().PrepareRoom(roomId, "");
                                                                //User.GetClient().SendMessage(new RoomForwardComposer(RoomId));
                                                                InteractingUser = 0;
                                                            }
                                                        } else {
                                                            user.TeleDelay--;
                                                            showTeleEffect = true;
                                                        }
                                                        //PlusEnvironment.GetGame().GetRoomManager().AddTeleAction(new TeleUserData(User.GetClient().GetMessageHandler(), User.GetClient().GetHabbo(), RoomId, TeleId));
                                                    }

                                                    GetRoom().GetGameMap().GenerateMaps();
                                                    // We're done with this tele. We have another one to bother.
                                                }
                                            } else {
                                                // This tele is not linked, so let's gtfo.
                                                user.UnlockWalking();
                                                InteractingUser = 0;
                                            }
                                        }
                                        // Is he in front of the tele?
                                        else if (user.Coordinate == SquareInFront) {
                                            user.AllowOverride = true;
                                            // Open the door
                                            keepDoorOpen = true;

                                            // Lock his walking. We're taking control over him. Allow overriding so he can get in the tele.
                                            if (user.IsWalking && (user.GoalX != GetX || user.GoalY != GetY)) {
                                                user.ClearMovement(true);
                                            }

                                            user.CanWalk = false;
                                            user.AllowOverride = true;

                                            // Move into the tele
                                            user.MoveTo(Coordinate.X, Coordinate.Y, true);
                                        }
                                        // Not even near, do nothing and move on for the next user.
                                        else {
                                            InteractingUser = 0;
                                        }
                                    } else {
                                        // Invalid user, do nothing and move on for the next user. 
                                        InteractingUser = 0;
                                    }
                                }

                                // Do we have a secondary user that wants to get out of the tele?
                                if (InteractingUser2 > 0) {
                                    user2 = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser2);

                                    // Is this user okay?
                                    if (user2 != null) {
                                        // If so, open the door, unlock the user's walking, and try to push him out in the right direction. We're done with him!
                                        keepDoorOpen = true;
                                        user2.UnlockWalking();
                                        user2.MoveTo(SquareInFront);
                                    }

                                    // This is a one time thing, whether the user's valid or not.
                                    InteractingUser2 = 0;
                                }

                                // Set the new item state, by priority
                                if (showTeleEffect) {
                                    if (ExtraData != "2") {
                                        ExtraData = "2";
                                        UpdateState(false, true);
                                    }
                                } else if (keepDoorOpen) {
                                    if (ExtraData != "1") {
                                        ExtraData = "1";
                                        UpdateState(false, true);
                                    }
                                } else {
                                    if (ExtraData != "0") {
                                        ExtraData = "0";
                                        UpdateState(false, true);
                                    }
                                }

                                // We're constantly going!
                                RequestUpdate(1, false);
                                break;
                            }

                        #endregion

                        #region Bottle

                        case InteractionType.Bottle:
                            ExtraData = RandomNumber.GenerateNewRandom(0, 7).ToString();
                            UpdateState();
                            break;

                        #endregion

                        #region Dice

                        case InteractionType.Dice: {
                                string[] numbers = ["1", "2", "3", "4", "5", "6"];
                                if (ExtraData == "-1")
                                    ExtraData = RandomizeStrings(numbers)[0];
                                UpdateState();
                            }
                            break;

                        #endregion

                        #region Habbo Wheel

                        case InteractionType.HabboWheel:
                            ExtraData = RandomNumber.GenerateRandom(1, 10).ToString();
                            UpdateState();
                            break;

                        #endregion

                        #region Love Shuffler

                        case InteractionType.LoveShuffler:

                            if (ExtraData == "0") {
                                ExtraData = RandomNumber.GenerateNewRandom(1, 4).ToString();
                                RequestUpdate(20, false);
                            } else if (ExtraData != "-1") {
                                ExtraData = "-1";
                            }

                            UpdateState(false, true);
                            break;

                        #endregion

                        #region Alert

                        case InteractionType.Alert:
                            if (ExtraData == "1") {
                                ExtraData = "0";
                                UpdateState(false, true);
                            }

                            break;

                        #endregion

                        #region Vending Machine

                        case InteractionType.VendingMachine:

                            if (ExtraData == "1") {
                                user = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                                if (user == null)
                                    break;
                                user.UnlockWalking();
                                if (GetBaseItem().VendingIds.Count > 0) {
                                    int randomDrink = GetBaseItem().VendingIds[RandomNumber.GenerateRandom(0, (GetBaseItem().VendingIds.Count - 1))];
                                    user.CarryItem(randomDrink);
                                }

                                InteractingUser = 0;
                                ExtraData = "0";

                                UpdateState(false, true);
                            }

                            break;

                        #endregion

                        #region RP Vending Machine

                        case InteractionType.RpVendor:

                            if (ExtraData == "1")
                                InteractorRpVendor.Dispense(this);

                            break;

                        #endregion

                        #region Scoreboard

                        case InteractionType.Scoreboard: {
                                if (string.IsNullOrEmpty(ExtraData))
                                    break;

                                int seconds = 0;

                                try {
                                    seconds = int.Parse(ExtraData);
                                } catch {
                                }

                                if (seconds > 0) {
                                    if (InteractionCountHelper == 1) {
                                        seconds--;
                                        InteractionCountHelper = 0;

                                        ExtraData = seconds.ToString();
                                        UpdateState();
                                    } else
                                        InteractionCountHelper++;

                                    UpdateCounter = 1;
                                } else
                                    UpdateCounter = 0;

                                break;
                            }

                        #endregion

                        #region Banzai Counter

                        case InteractionType.BanzaiCounter: {
                                if (string.IsNullOrEmpty(ExtraData))
                                    break;

                                int seconds = 0;

                                try {
                                    seconds = int.Parse(ExtraData);
                                } catch {
                                }

                                if (seconds > 0) {
                                    if (InteractionCountHelper == 1) {
                                        seconds--;
                                        InteractionCountHelper = 0;

                                        if (GetRoom().GetBanzai().IsBanzaiActive) {
                                            ExtraData = seconds.ToString();
                                            UpdateState();
                                        } else
                                            break;
                                    } else
                                        InteractionCountHelper++;

                                    UpdateCounter = 1;
                                } else {
                                    UpdateCounter = 0;
                                    GetRoom().GetBanzai().BanzaiEnd();
                                }

                                break;
                            }

                        #endregion

                        #region Banzai Tele

                        case InteractionType.BanzaiTele: {
                                ExtraData = string.Empty;
                                UpdateState();
                                break;
                            }

                        #endregion

                        #region Banzai Floor

                        case InteractionType.BanzaiFloor: {
                                if (Value == 3) {
                                    if (InteractionCountHelper == 1) {
                                        InteractionCountHelper = 0;

                                        switch (Team) {
                                            case Team.Blue: {
                                                    ExtraData = "11";
                                                    break;
                                                }

                                            case Team.Green: {
                                                    ExtraData = "8";
                                                    break;
                                                }

                                            case Team.Red: {
                                                    ExtraData = "5";
                                                    break;
                                                }

                                            case Team.Yellow: {
                                                    ExtraData = "14";
                                                    break;
                                                }
                                        }
                                    } else {
                                        ExtraData = "";
                                        InteractionCountHelper++;
                                    }

                                    UpdateState();

                                    InteractionCount++;

                                    if (InteractionCount < 16) {
                                        UpdateCounter = 1;
                                    } else
                                        UpdateCounter = 0;
                                }

                                break;
                            }

                        #endregion

                        #region Banzai Puck

                        case InteractionType.BanzaiPuck: {
                                if (InteractionCount > 4) {
                                    InteractionCount++;
                                    UpdateCounter = 1;
                                } else {
                                    InteractionCount = 0;
                                    UpdateCounter = 0;
                                }

                                break;
                            }

                        #endregion

                        #region Freeze Tile

                        case InteractionType.FreezeTile: {
                                if (InteractingUser > 0) {
                                    ExtraData = "11000";
                                    UpdateState(false, true);
                                    GetRoom().GetFreeze().OnFreezeTiles(this, FreezePowerUp);
                                    InteractingUser = 0;
                                    InteractionCountHelper = 0;
                                }

                                break;
                            }

                        #endregion

                        #region Football Counter

                        case InteractionType.Counter: {
                                if (string.IsNullOrEmpty(ExtraData))
                                    break;

                                int seconds = 0;

                                try {
                                    seconds = int.Parse(ExtraData);
                                } catch {
                                }

                                if (seconds > 0) {
                                    if (InteractionCountHelper == 1) {
                                        seconds--;
                                        InteractionCountHelper = 0;
                                        if (GetRoom().GetSoccer().GameIsStarted) {
                                            ExtraData = seconds.ToString();
                                            UpdateState();
                                        } else
                                            break;
                                    } else
                                        InteractionCountHelper++;

                                    UpdateCounter = 1;
                                } else {
                                    UpdateNeeded = false;
                                    GetRoom().GetSoccer().StopGame();
                                }

                                break;
                            }

                        #endregion

                        #region Freeze Timer

                        case InteractionType.FreezeTimer: {
                                if (string.IsNullOrEmpty(ExtraData))
                                    break;

                                int seconds = 0;

                                try {
                                    seconds = int.Parse(ExtraData);
                                } catch {
                                }

                                if (seconds > 0) {
                                    if (InteractionCountHelper == 1) {
                                        seconds--;
                                        InteractionCountHelper = 0;
                                        if (GetRoom().GetFreeze().GameIsStarted) {
                                            ExtraData = seconds.ToString();
                                            UpdateState();
                                        } else
                                            break;
                                    } else
                                        InteractionCountHelper++;

                                    UpdateCounter = 1;
                                } else {
                                    UpdateNeeded = false;
                                    GetRoom().GetFreeze().StopGame();
                                }

                                break;
                            }

                        #endregion

                        #region Wired

                        case InteractionType.WiredEffect:
                        case InteractionType.WiredTrigger:
                        case InteractionType.WiredCondition: {
                                if (ExtraData == "1") {
                                    ExtraData = "0";
                                    UpdateState(false, true);
                                }
                            }
                            break;

                        #endregion

                        #region Cannon

                        case InteractionType.Cannon: {
                                if (ExtraData != "1")
                                    break;

                                #region Target Calculation

                                Point targetStart = Coordinate;
                                List<Point> targetSquares = [];
                                switch (Rotation) {
                                    case 0: {
                                            targetStart = new Point(GetX - 1, GetY);

                                            if (!targetSquares.Contains(targetStart))
                                                targetSquares.Add(targetStart);

                                            for (int I = 1; I <= 3; I++) {
                                                Point targetSquare = new(targetStart.X - I, targetStart.Y);

                                                if (!targetSquares.Contains(targetSquare))
                                                    targetSquares.Add(targetSquare);
                                            }

                                            break;
                                        }

                                    case 2: {
                                            targetStart = new Point(GetX, GetY - 1);

                                            if (!targetSquares.Contains(targetStart))
                                                targetSquares.Add(targetStart);

                                            for (int I = 1; I <= 3; I++) {
                                                Point targetSquare = new(targetStart.X, targetStart.Y - I);

                                                if (!targetSquares.Contains(targetSquare))
                                                    targetSquares.Add(targetSquare);
                                            }

                                            break;
                                        }

                                    case 4: {
                                            targetStart = new Point(GetX + 2, GetY);

                                            if (!targetSquares.Contains(targetStart))
                                                targetSquares.Add(targetStart);

                                            for (int I = 1; I <= 3; I++) {
                                                Point targetSquare = new(targetStart.X + I, targetStart.Y);

                                                if (!targetSquares.Contains(targetSquare))
                                                    targetSquares.Add(targetSquare);
                                            }

                                            break;
                                        }

                                    case 6: {
                                            targetStart = new Point(GetX, GetY + 2);

                                            if (!targetSquares.Contains(targetStart))
                                                targetSquares.Add(targetStart);

                                            for (int I = 1; I <= 3; I++) {
                                                Point targetSquare = new(targetStart.X, targetStart.Y + I);

                                                if (!targetSquares.Contains(targetSquare))
                                                    targetSquares.Add(targetSquare);
                                            }

                                            break;
                                        }
                                }

                                #endregion

                                if (targetSquares.Count > 0) {
                                    foreach (Point square in targetSquares.ToList()) {
                                        List<RoomUser> affectedUsers = _room.GetGameMap().GetRoomUsers(square).ToList();

                                        if (affectedUsers == null || affectedUsers.Count == 0)
                                            continue;

                                        foreach (RoomUser target in affectedUsers) {
                                            if (target == null || target.IsBot || target.IsPet)
                                                continue;

                                            if (target.GetClient() == null || target.GetClient().GetHabbo() == null)
                                                continue;

                                            if (_room.CheckRights(target.GetClient(), true))
                                                continue;

                                            target.ApplyEffect(4);
                                            target.GetClient().SendPacket(new RoomNotificationComposer("Kicked from room", "You were hit by a cannonball!", "room_kick_cannonball", ""));
                                            target.ApplyEffect(0);
                                            _room.GetRoomUserManager().RemoveUserFromRoom(target.GetClient(), true);
                                        }
                                    }
                                }

                                ExtraData = "2";
                                UpdateState(false, true);
                            }
                            break;

                        #endregion
                    }
                }
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }
        }

        public static string[] RandomizeStrings(string[] arr)
        {
            List<KeyValuePair<int, string>> list = new();
            // Add all strings from array
            // Add new random int each time
            foreach (string s in arr) {
                list.Add(new KeyValuePair<int, string>(_random.Next(), s));
            }

            // Sort the list by the random number
            var sorted = from item in list
                         orderby item.Key
                         select item;
            // Allocate new string array
            string[] result = new string[arr.Length];
            // Copy values to array
            int index = 0;
            foreach (KeyValuePair<int, string> pair in sorted) {
                result[index] = pair.Value;
                index++;
            }

            // Return copied array
            return result;
        }

        public void RequestUpdate(int cycles, bool setUpdate)
        {
            UpdateCounter = cycles;
            if (setUpdate)
                UpdateNeeded = true;
        }

        public void UpdateState()
        {
            UpdateState(true, true);
        }

        public void UpdateState(bool inDb, bool inRoom)
        {
            if (GetRoom() == null)
                return;

            if (inDb)
                GetRoom().GetRoomItemHandler().UpdateItem(this);

            if (inRoom) {
                if (IsFloorItem)
                    GetRoom().SendPacket(new ObjectUpdateComposer(this, GetRoom().OwnerId));
                else
                    GetRoom().SendPacket(new ItemUpdateComposer(this, GetRoom().OwnerId));
            }

            if (IsFloorItem && GetBaseItem() != null && GetBaseItem().HasAdjustableHeights)
                GetRoom().GetGameMap().OnItemHeightChanged(this);
        }

        public void ResetBaseItem()
        {
            Data = null;
            Data = GetBaseItem();
        }

        public ItemData GetBaseItem()
        {
            if (Data == null) {
                if (PlusEnvironment.GetGame().GetItemManager().GetItem(BaseItem, out ItemData I))
                    Data = I;
            }

            return Data;
        }

        public Room GetRoom()
        {
            if (_room != null)
                return _room;

            if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(RoomId, out Room room))
                return room;

            return null;
        }

        public void UserFurniCollision(RoomUser user)
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerUserFurniCollision, user.GetClient().GetHabbo(), this);
        }

        public void UserWalksOnFurni(RoomUser user)
        {
            if (user == null)
                return;

            if (user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            if (GetBaseItem().InteractionType == InteractionType.Tent || GetBaseItem().InteractionType == InteractionType.TentSmall) {
                GetRoom().AddUserToTent(Id, user);
            }

            GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerWalkOnFurni, user.GetClient().GetHabbo(), this);
            user.LastItem = this;
        }

        public void UserWalksOffFurni(RoomUser user)
        {
            if (user == null)
                return;

            if (user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            if (GetBaseItem().InteractionType == InteractionType.Tent || GetBaseItem().InteractionType == InteractionType.TentSmall)
                GetRoom().RemoveUserFromTent(Id, user);

            GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerWalkOffFurni, user.GetClient().GetHabbo(), this);
        }

        public void SetPressurePlate(bool pressed)
        {
            if (GetBaseItem()?.InteractionType != InteractionType.PressurePlate)
                return;

            string state = pressed ? "1" : "0";

            if (ExtraData == state)
                return;

            ExtraData = state;
            UpdateState(false, true);
        }

        public void Destroy()
        {
            _room = null;
            Data = null;
            GetAffectedTiles.Clear();
        }
    }
}