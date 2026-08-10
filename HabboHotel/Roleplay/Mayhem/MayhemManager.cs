using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Speech;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Roleplay;

namespace Plus.HabboHotel.Roleplay.Mayhem
{
    public class MayhemManager
    {
        private const int EffectId = 711;
        private const int EventDurationMs = 60000;
        private const int MaxConcurrentBots = 200;
        private const int AttackDamage = 10;
        private const int AttackCooldownMs = 1000;
        private const int ThinkIntervalMs = 300;
        private const int SpawnIntervalStartMs = 900;
        private const int SpawnIntervalEndMs = 120;
        private const int FleeTimeoutMs = 6000;

        private static int _nextBotId = -100000;

        private static readonly (string Name, string Look, string Gender)[] Templates =
        {
            ("Po",          "sh-290-92.lg-4315-1408.hd-209-11.ha-4313-1408.cc-4314-1408", "M"),
            ("Laa-Laa",     "sh-290-92.lg-4312-1408.hd-209-11.ha-4310-1408.cc-4311-1408", "M"),
            ("Dipsy",       "sh-290-92.lg-4309-1408.hd-209-11.ha-4307-1408.cc-4308-1408", "M"),
            ("Tinky Winky", "sh-290-92.lg-4306-1408.hd-209-11.ha-4304-1408.cc-4305-1408", "M"),
        };

        private readonly ConcurrentDictionary<int, MayhemEvent> _events = new();
        private readonly Random _random = new();

        public bool IsActive(int roomId) => _events.ContainsKey(roomId);

        public bool TryStart(Room room, out string error)
        {
            error = string.Empty;
            if (room == null) {
                error = "You aren't in a room.";
                return false;
            }

            if (_events.ContainsKey(room.Id)) {
                error = "Mayhem is already running in this room.";
                return false;
            }

            if (GetArrowTiles(room).Count == 0) {
                error = "Place at least one arrow furni in the room first — that's where the bots spawn.";
                return false;
            }

            long now = PlusEnvironment.Now();
            _events[room.Id] = new MayhemEvent
            {
                RoomId = room.Id,
                EndsAtMs = now + EventDurationMs,
                NextSpawnMs = now
            };

            return true;
        }

        public void Stop(int roomId)
        {
            if (_events.TryGetValue(roomId, out MayhemEvent ev))
                ev.Ending = true;
        }

        public bool TryHitBot(Habbo attacker)
        {
            if (attacker?.CurrentRoom == null)
                return false;

            if (!_events.TryGetValue(attacker.CurrentRoom.Id, out MayhemEvent ev))
                return false;

            RoomUser attackerUser = attacker.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(attacker.Id);
            if (attackerUser == null)
                return false;

            lock (ev.Lock) {
                MayhemBot hit = ev.Bots
                    .Where(b => !b.Fleeing && Gamemap.TileDistance(b.User.X, b.User.Y, attackerUser.X, attackerUser.Y) <= 3)
                    .OrderBy(b => Gamemap.TileDistance(b.User.X, b.User.Y, attackerUser.X, attackerUser.Y))
                    .FirstOrDefault();

                if (hit == null)
                    return false;

                StartFleeing(ev, hit);
                return true;
            }
        }

        public void OnCycle()
        {
            if (_events.IsEmpty)
                return;

            long now = PlusEnvironment.Now();

            foreach (MayhemEvent ev in _events.Values.ToList()) {
                try {
                    ProcessEvent(ev, now);
                } catch (Exception e) {
                    Core.ExceptionLogger.LogException(e);
                    TeardownEvent(ev);
                }
            }
        }

        private void ProcessEvent(MayhemEvent ev, long now)
        {
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(ev.RoomId, out Room room)) {
                TeardownEvent(ev);
                return;
            }

            List<Point> arrows = GetArrowTiles(room);
            List<RoomUser> players = [.. room.GetRoomUserManager().GetRoomUsers().Where(u => u != null && !u.IsAsleep && IsHuntable(u.GetClient()?.GetHabbo()?.GetRpStats()))];

            if (!ev.Ending && (now >= ev.EndsAtMs || players.Count == 0 || arrows.Count == 0))
                ev.Ending = true;

            lock (ev.Lock) {
                if (ev.Ending) {
                    foreach (MayhemBot bot in ev.Bots.Where(b => !b.Fleeing).ToList())
                        StartFleeing(ev, bot);
                } else {
                    TrySpawn(ev, room, arrows, now);
                }

                for (int i = ev.Bots.Count - 1; i >= 0; i--) {
                    MayhemBot bot = ev.Bots[i];

                    if (bot.User.GetRoom() == null) {
                        ev.Bots.RemoveAt(i);
                        continue;
                    }

                    if (bot.Fleeing)
                        UpdateFleeing(room, ev, bot, i, now);
                    else
                        UpdateHunting(room, bot, players, now);
                }

                if (ev.Ending && ev.Bots.Count == 0)
                    TeardownEvent(ev);
            }
        }

        private void TrySpawn(MayhemEvent ev, Room room, List<Point> arrows, long now)
        {
            if (now < ev.NextSpawnMs || ev.Bots.Count >= MaxConcurrentBots)
                return;

            // Spawn rate ramps up across the event: slow interval at the start, fast near the end.
            double elapsed = Math.Clamp(1.0 - (ev.EndsAtMs - now) / (double)EventDurationMs, 0, 1);
            int interval = (int)(SpawnIntervalStartMs - (SpawnIntervalStartMs - SpawnIntervalEndMs) * elapsed);
            ev.NextSpawnMs = now + interval;

            foreach (Point arrow in arrows) {
                if (ev.Bots.Count >= MaxConcurrentBots)
                    break;

                bool occupied = ev.Bots.Any(b => b.User.X == arrow.X && b.User.Y == arrow.Y);
                if (occupied)
                    continue;

                SpawnBot(ev, room, arrow);
            }
        }

        private void SpawnBot(MayhemEvent ev, Room room, Point tile)
        {
            (string name, string look, string gender) = Templates[_random.Next(Templates.Length)];
            double z = room.GetGameMap().SqAbsoluteHeight(tile.X, tile.Y);

            List<RandomSpeech> speeches = [];
            RoomBot bot = new(_nextBotId--, room.Id, "generic", "stand", name, string.Empty, look,
                tile.X, tile.Y, z, 0, 0, 0, 0, 0, ref speeches, gender, 0, 0,
                false, 0, false, 0, EffectId);

            RoomUser user = room.GetRoomUserManager().DeployBot(bot, null);
            if (user == null)
                return;

            user.Chat("DIE!");
            ev.Bots.Add(new MayhemBot { User = user });
        }

        private void UpdateHunting(Room room, MayhemBot bot, List<RoomUser> players, long now)
        {
            if (players.Count == 0)
                return;

            RoomUser target = bot.TargetHabboId == 0
                ? null
                : players.FirstOrDefault(p => p.HabboId == bot.TargetHabboId);

            if (target == null) {
                target = bot.TargetHabboId == 0
                    ? players[_random.Next(players.Count)]
                    : players.OrderBy(p => Gamemap.TileDistance(bot.User.X, bot.User.Y, p.X, p.Y)).First();

                bot.TargetHabboId = target.HabboId;
                bot.NextThinkMs = 0;
            }

            if (Gamemap.TileDistance(bot.User.X, bot.User.Y, target.X, target.Y) <= 1) {
                if (now >= bot.NextAttackMs) {
                    bot.NextAttackMs = now + AttackCooldownMs;
                    Attack(room, bot, target);
                }
                return;
            }

            if (now < bot.NextThinkMs)
                return;

            bot.NextThinkMs = now + ThinkIntervalMs;

            Point? sq = GetBestAdjacentSquare(room, target, bot.User);
            if (sq.HasValue && (bot.User.GoalX != sq.Value.X || bot.User.GoalY != sq.Value.Y))
                bot.User.MoveTo(sq.Value.X, sq.Value.Y);
        }

        private static void Attack(Room room, MayhemBot bot, RoomUser target)
        {
            Habbo victim = target.GetClient()?.GetHabbo();
            UserRpStats stats = victim?.GetRpStats();
            if (victim == null || stats == null || stats.IsDead || stats.IsGodMode || stats.PassiveMode)
                return;

            bot.User.SetRot(RotationTowards(bot.User.X, bot.User.Y, target.X, target.Y), false);

            int healthBefore = stats.Health;
            int damageToHealth = Math.Min(healthBefore, Math.Max(0, AttackDamage - stats.Shield));
            bool lethal = damageToHealth >= healthBefore && healthBefore > 0;

            stats.Shield = Math.Max(0, stats.Shield - AttackDamage);
            if (damageToHealth > 0)
                stats.Health -= damageToHealth;

            stats.DamageTaken += AttackDamage;

            room.SendPacket(new ShoutComposer(bot.User.VirtualId,
                "*mauls " + victim.Username + " for " + AttackDamage + " damage*", 0, 29, isRpAction: true));

            if (lethal) {
                stats.Health = 0;
                LiveFeedService.LiveFeed("👾 " + LiveFeedService.Name(victim.Username, "red") + " was torn apart by the Teletubbies", "red");
                UserRpStats.Kill(victim, target);
            }

            victim.TrySendUserStatsUpdate(true);
        }

        private void UpdateFleeing(Room room, MayhemEvent ev, MayhemBot bot, int index, long now)
        {
            bool reached = bot.User.X == bot.FleeTarget.X && bot.User.Y == bot.FleeTarget.Y;
            if (reached || now >= bot.FleeDeadlineMs) {
                room.GetRoomUserManager().RemoveBot(bot.User.VirtualId, true);
                ev.Bots.RemoveAt(index);
                return;
            }

            if (bot.User.GoalX != bot.FleeTarget.X || bot.User.GoalY != bot.FleeTarget.Y)
                bot.User.MoveTo(bot.FleeTarget.X, bot.FleeTarget.Y);
        }

        private void StartFleeing(MayhemEvent ev, MayhemBot bot)
        {
            bot.Fleeing = true;
            bot.FleeDeadlineMs = PlusEnvironment.Now() + FleeTimeoutMs;
            bot.User.ApplyEffect(0);

            Room room = bot.User.GetRoom();
            List<Point> arrows = room == null ? [] : GetArrowTiles(room);
            if (arrows.Count == 0) {
                // Nowhere to run to — just leave on the spot.
                bot.FleeTarget = new Point(bot.User.X, bot.User.Y);
                bot.FleeDeadlineMs = 0;
                return;
            }

            bot.FleeTarget = arrows
                .OrderBy(a => Gamemap.TileDistance(bot.User.X, bot.User.Y, a.X, a.Y))
                .First();
            bot.User.MoveTo(bot.FleeTarget.X, bot.FleeTarget.Y);
        }

        private void TeardownEvent(MayhemEvent ev)
        {
            _events.TryRemove(ev.RoomId, out _);

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(ev.RoomId, out Room room))
                return;

            lock (ev.Lock) {
                foreach (MayhemBot bot in ev.Bots)
                    room.GetRoomUserManager().RemoveBot(bot.User.VirtualId, true);

                ev.Bots.Clear();
            }
        }

        private static bool IsHuntable(UserRpStats stats) =>
            stats != null && !stats.IsDead && !stats.PassiveMode && !stats.IsGodMode;

        private static List<Point> GetArrowTiles(Room room)
        {
            List<Point> tiles = new();
            if (room?.GetRoomItemHandler() == null)
                return tiles;

            foreach (Item item in room.GetRoomItemHandler().GetFloor) {
                if (item?.GetBaseItem()?.InteractionType == InteractionType.Arrow)
                    tiles.Add(new Point(item.GetX, item.GetY));
            }

            return tiles;
        }

        private static Point? GetBestAdjacentSquare(Room room, RoomUser target, RoomUser mover)
        {
            Point[] candidates =
            {
                new(target.X, target.Y - 1),
                new(target.X + 1, target.Y),
                new(target.X, target.Y + 1),
                new(target.X - 1, target.Y),
                new(target.X + 1, target.Y - 1),
                new(target.X + 1, target.Y + 1),
                new(target.X - 1, target.Y + 1),
                new(target.X - 1, target.Y - 1)
            };

            return candidates
                .Where(p => room.GetGameMap().ValidTile(p.X, p.Y) && room.GetGameMap().SquareIsOpen(p.X, p.Y, false))
                .OrderBy(p => Math.Abs(p.X - mover.X) + Math.Abs(p.Y - mover.Y))
                .Cast<Point?>()
                .FirstOrDefault();
        }

        private static int RotationTowards(int x, int y, int tx, int ty)
        {
            int dx = tx - x;
            int dy = ty - y;
            if (dx == 0 && dy < 0) return 0;
            if (dx > 0 && dy < 0) return 1;
            if (dx > 0 && dy == 0) return 2;
            if (dx > 0 && dy > 0) return 3;
            if (dx == 0 && dy > 0) return 4;
            if (dx < 0 && dy > 0) return 5;
            if (dx < 0 && dy == 0) return 6;
            return 7;
        }

        private class MayhemEvent
        {
            public int RoomId;
            public long EndsAtMs;
            public long NextSpawnMs;
            public bool Ending;
            public readonly object Lock = new();
            public readonly List<MayhemBot> Bots = new();
        }

        private class MayhemBot
        {
            public RoomUser User;
            public int TargetHabboId;
            public long NextAttackMs;
            public long NextThinkMs;
            public bool Fleeing;
            public Point FleeTarget;
            public long FleeDeadlineMs;
        }
    }
}