using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rooms.Games.Teams
{
    public class TeamManager
    {
        public string Game;
        public List<RoomUser> BlueTeam;
        public List<RoomUser> GreenTeam;
        public List<RoomUser> RedTeam;
        public List<RoomUser> YellowTeam;

        public static TeamManager CreateTeam(string game)
        {
            var team = new TeamManager
            {
                Game = game,
                BlueTeam = new List<RoomUser>(),
                RedTeam = new List<RoomUser>(),
                GreenTeam = new List<RoomUser>(),
                YellowTeam = new List<RoomUser>()
            };
            return team;
        }

        public bool CanEnterOnTeam(Team team)
        {
            switch (team)
            {
                case Team.Blue:
                    return BlueTeam.Count < 5;
                case Team.Red:
                    return RedTeam.Count < 5;
                case Team.Yellow:
                    return YellowTeam.Count < 5;
                case Team.Green:
                    return GreenTeam.Count < 5;
                default:
                    return false;
            }
        }

        public void AddUser(RoomUser user)
        {
            if (user.Team.Equals(Team.Blue) && !BlueTeam.Contains(user))
                BlueTeam.Add(user);
            else if (user.Team.Equals(Team.Red) && !RedTeam.Contains(user))
                RedTeam.Add(user);
            else if (user.Team.Equals(Team.Yellow) && !YellowTeam.Contains(user))
                YellowTeam.Add(user);
            else if (user.Team.Equals(Team.Green) && !GreenTeam.Contains(user))
                GreenTeam.Add(user);

            switch (Game.ToLower())
            {
                case "banzai":
                {
                    Room room = user.GetClient().GetHabbo().CurrentRoom;
                    if (room == null)
                        return;

                    foreach (Item item in room.GetRoomItemHandler().GetFloor.ToList())
                    {
                        if (item == null)
                            continue;

                        if (item.GetBaseItem().InteractionType.Equals(InteractionType.BanzaiGateBlue)) {
                            item.ExtraData = BlueTeam.Count.ToString();
                            item.UpdateState();
                            if (BlueTeam.Count == 5) {
                                foreach (RoomUser sser in room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                {
                                    sser.SqState = 0;
                                }

                                room.GetGameMap().GameMap[item.GetX, item.GetY] = 0;
                            }
                        } else if (item.GetBaseItem().InteractionType.Equals(InteractionType.BanzaiGateRed)) {
                            item.ExtraData = RedTeam.Count.ToString();
                            item.UpdateState();
                            if (RedTeam.Count == 5) {
                                foreach (RoomUser sser in room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                {
                                    sser.SqState = 0;
                                }

                                room.GetGameMap().GameMap[item.GetX, item.GetY] = 0;
                            }
                        } else if (item.GetBaseItem().InteractionType.Equals(InteractionType.BanzaiGateGreen)) {
                            item.ExtraData = GreenTeam.Count.ToString();
                            item.UpdateState();
                            if (GreenTeam.Count == 5) {
                                foreach (RoomUser sser in room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                    sser.SqState = 0;

                                room.GetGameMap().GameMap[item.GetX, item.GetY] = 0;
                            }
                        } else if (item.GetBaseItem().InteractionType.Equals(InteractionType.BanzaiGateYellow)) {
                            item.ExtraData = YellowTeam.Count.ToString();
                            item.UpdateState();
                            if (YellowTeam.Count == 5) {
                                foreach (RoomUser sser in room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                    sser.SqState = 0;

                                room.GetGameMap().GameMap[item.GetX, item.GetY] = 0;
                            }
                        }
                    }

                    break;
                }

                case "freeze":
                {
                    Room room = user.GetClient().GetHabbo().CurrentRoom;
                    if (room == null)
                        return;

                    foreach (Item item in room.GetRoomItemHandler().GetFloor.ToList())
                    {
                        if (item == null)
                            continue;

                        if (item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeBlueGate)) {
                            item.ExtraData = BlueTeam.Count.ToString();
                            item.UpdateState();
                        } else if (item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeRedGate)) {
                            item.ExtraData = RedTeam.Count.ToString();
                            item.UpdateState();
                        } else if (item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeGreenGate)) {
                            item.ExtraData = GreenTeam.Count.ToString();
                            item.UpdateState();
                        } else if (item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeYellowGate)) {
                            item.ExtraData = YellowTeam.Count.ToString();
                            item.UpdateState();
                        }
                    }

                    break;
                }
            }
        }

        public void OnUserLeave(RoomUser user)
        {
            //Console.WriteLine("remove user from team! (" + Game + ")");
            if (user.Team.Equals(Team.Blue) && BlueTeam.Contains(user))
                BlueTeam.Remove(user);
            else if (user.Team.Equals(Team.Red) && RedTeam.Contains(user))
                RedTeam.Remove(user);
            else if (user.Team.Equals(Team.Yellow) && YellowTeam.Contains(user))
                YellowTeam.Remove(user);
            else if (user.Team.Equals(Team.Green) && GreenTeam.Contains(user))
                GreenTeam.Remove(user);

            switch (Game.ToLower())
            {
                case "banzai":
                {
                    Room room = user.GetClient().GetHabbo().CurrentRoom;
                    if (room == null)
                        return;

                    foreach (Item item in room.GetRoomItemHandler().GetFloor.ToList())
                    {
                        if (item == null)
                            continue;

                        if (item.GetBaseItem().InteractionType.Equals(InteractionType.BanzaiGateBlue)) {
                            item.ExtraData = BlueTeam.Count.ToString();
                            item.UpdateState();
                            if (room.GetGameMap().GameMap[item.GetX, item.GetY] == 0) {
                                foreach (RoomUser sser in room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                    sser.SqState = 1;

                                room.GetGameMap().GameMap[item.GetX, item.GetY] = 1;
                            }
                        } else if (item.GetBaseItem().InteractionType.Equals(InteractionType.BanzaiGateRed)) {
                            item.ExtraData = RedTeam.Count.ToString();
                            item.UpdateState();
                            if (room.GetGameMap().GameMap[item.GetX, item.GetY] == 0) {
                                foreach (RoomUser sser in room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                    sser.SqState = 1;


                                room.GetGameMap().GameMap[item.GetX, item.GetY] = 1;
                            }
                        } else if (item.GetBaseItem().InteractionType.Equals(InteractionType.BanzaiGateGreen)) {
                            item.ExtraData = GreenTeam.Count.ToString();
                            item.UpdateState();
                            if (room.GetGameMap().GameMap[item.GetX, item.GetY] == 0) {
                                foreach (RoomUser sser in room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                    sser.SqState = 1;

                                room.GetGameMap().GameMap[item.GetX, item.GetY] = 1;
                            }
                        } else if (item.GetBaseItem().InteractionType.Equals(InteractionType.BanzaiGateYellow)) {
                            item.ExtraData = YellowTeam.Count.ToString();
                            item.UpdateState();
                            if (room.GetGameMap().GameMap[item.GetX, item.GetY] == 0) {
                                foreach (RoomUser sser in room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                    sser.SqState = 1;

                                room.GetGameMap().GameMap[item.GetX, item.GetY] = 1;
                            }
                        }
                    }

                    break;
                }
                case "freeze":
                {
                    Room room = user.GetClient().GetHabbo().CurrentRoom;
                    if (room == null)
                        return;

                    foreach (Item item in room.GetRoomItemHandler().GetFloor.ToList())
                    {
                        if (item == null)
                            continue;

                        if (item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeBlueGate)) {
                            item.ExtraData = BlueTeam.Count.ToString();
                            item.UpdateState();
                        } else if (item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeRedGate)) {
                            item.ExtraData = RedTeam.Count.ToString();
                            item.UpdateState();
                        } else if (item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeGreenGate)) {
                            item.ExtraData = GreenTeam.Count.ToString();
                            item.UpdateState();
                        } else if (item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeYellowGate)) {
                            item.ExtraData = YellowTeam.Count.ToString();
                            item.UpdateState();
                        }
                    }

                    break;
                }
            }
        }

        public void Dispose()
        {
            BlueTeam.Clear();
            GreenTeam.Clear();
            RedTeam.Clear();
            YellowTeam.Clear();
        }
    }
}