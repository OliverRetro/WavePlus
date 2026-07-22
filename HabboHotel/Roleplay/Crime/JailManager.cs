using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Police;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Roleplay;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;

namespace Plus.HabboHotel.Roleplay.Crime
{
    public sealed class JailManager
    {
        private readonly ConcurrentDictionary<int, byte> _jailed = new();

        public sealed class JailSpawn
        {
            public int roomid { get; set; }
            public int x { get; set; } = -1;
            public int y { get; set; } = -1;
            public double z { get; set; }
            public int rotation { get; set; }
        }

        public int GetJailRoomId() =>
            int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue("rp.jail.room.id"), out int id) ? id : 0;

        public JailSpawn GetJailSpawn()
        {
            string raw = PlusEnvironment.GetSettingsManager().TryGetValue("rp.jail.spawn");
            if (!string.IsNullOrWhiteSpace(raw)) {
                try {
                    JailSpawn spawn = JsonSerializer.Deserialize<JailSpawn>(raw);
                    if (spawn != null && spawn.roomid > 0)
                        return spawn;
                } catch { /* malformed setting — fall through to the plain room id */ }
            }

            int roomId = GetJailRoomId();
            return roomId > 0 ? new JailSpawn { roomid = roomId } : null;
        }

        public string GetJailOutfit() => PlusEnvironment.GetSettingsManager().TryGetValue("rp.jail.outfit");

        private static int GetEscortBonus() =>
            int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue("rp.police.escort.jail.bonus"), out int bonus) ? bonus : 0;

        public bool IsJailed(int userId)
        {
            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            UserRpStats stats = client?.GetHabbo()?.GetRpStats();
            return stats != null && stats.JailReleaseTime > PlusEnvironment.GetUnixTimestamp();
        }

        public bool JailFromEscort(Habbo target, Habbo officer, int stars, string originalLook)
        {
            if (target == null)
                return false;

            RpCrimePenalty penalty = PlusEnvironment.GetRpCrimeManager().GetPenalty(stars);
            int jailMinutes = penalty?.JailTimeMinutes ?? 0;
            if (jailMinutes <= 0)
                return false;

            UserRpStats stats = target.GetRpStats();
            if (stats == null)
                return false;

            TakeFine(target, penalty.FineAmount);

            JailSpawn spawn = GetJailSpawn();
            int jailRoom = spawn != null ? spawn.roomid : (target.CurrentRoomId > 0 ? target.CurrentRoomId : GetJailRoomId());
            stats.JailReleaseTime = PlusEnvironment.GetUnixTimestamp() + jailMinutes * 60.0;
            stats.JailPending = false;
            stats.JailStars = stars;
            stats.JailRevertLook = string.IsNullOrEmpty(originalLook) ? PlusEnvironment.GetPoliceManager().GetOriginalLook(target.Id) ?? target.Look : originalLook;
            stats.JailRoomId = jailRoom;
            stats.Arrested++;
            target.SaveRpStats();
            target.TrySendUserStatsUpdate(true);

            // Achievement: the suspect was arrested (jailed).
            PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(target.GetClient(), "ACH_Arrest", 1);

            _jailed[target.Id] = 0;

            // Stay cuffed and confined while serving the sentence; the jail outfit (which the
            // hotel keeps cuffs on) replaces the plain cuff shirt for the duration.
            PlusEnvironment.GetPoliceManager().EnterJailCuff(target, stats.JailRevertLook);
            ApplyJailOutfit(target);
            SendToJail(target);

            if (officer != null) {
                int bonus = GetEscortBonus();
                if (bonus > 0) {
                    officer.Credits += bonus;
                    officer.GetClient()?.SendPacket(new CreditBalanceComposer(officer.Credits));
                    officer.GetClient()?.SendWhisper($"You escorted {target.Username} to jail (+{bonus} credits).", 1);
                }
            }

            target.GetClient()?.SendWhisper($"You've been jailed for {jailMinutes} minute(s). Fine: {penalty.FineAmount} credits.", 1);
            return true;
        }

        public int GetLoginRoomId(Habbo habbo)
        {
            UserRpStats stats = habbo?.GetRpStats();
            if (stats == null)
                return 0;

            double now = PlusEnvironment.GetUnixTimestamp();

            if (stats.JailTimeLeft > 0) {
                stats.JailReleaseTime = now + stats.JailTimeLeft;
                stats.JailTimeLeft = 0;
                habbo.SaveRpStats();
            }

            if (stats.JailPending) {
                RpCrimePenalty penalty = PlusEnvironment.GetRpCrimeManager().GetPenalty(stats.JailStars);
                int jailMinutes = penalty?.JailTimeMinutes ?? 0;
                stats.JailPending = false;

                if (jailMinutes > 0) {
                    TakeFine(habbo, penalty.FineAmount);
                    stats.JailReleaseTime = now + jailMinutes * 60.0;
                    if (stats.JailRoomId <= 0)
                        stats.JailRoomId = GetJailRoomId();
                } else {
                    stats.JailReleaseTime = 0;
                }
                habbo.SaveRpStats();
            }

            if (stats.JailReleaseTime <= 0)
                return 0;

            // Sentence lapsed while offline — release them.
            if (now >= stats.JailReleaseTime) {
                ClearSentence(habbo, stats, restoreLook: true, whisper: false);
                return 0;
            }

            int jailRoom = stats.JailRoomId > 0 ? stats.JailRoomId : (GetJailSpawn()?.roomid ?? 0);
            return jailRoom;
        }

        public void HandleRoomEntry(Habbo habbo)
        {
            UserRpStats stats = habbo?.GetRpStats();
            if (habbo?.CurrentRoom == null || stats == null || stats.JailReleaseTime <= PlusEnvironment.GetUnixTimestamp())
                return;

            _jailed[habbo.Id] = 0;

            int jailRoom = stats.JailRoomId > 0 ? stats.JailRoomId : (GetJailSpawn()?.roomid ?? 0);

            if (!PoliceManager.GetJailRoomIds().Contains(habbo.CurrentRoom.RoomId)) {
                if (jailRoom > 0 && habbo.CurrentRoom.RoomId != jailRoom)
                    habbo.PrepareRoom(jailRoom, string.Empty);
                return;
            }

            habbo.PendingReconnectPlacement = false;
            PlusEnvironment.GetPoliceManager().EnterJailCuff(habbo, stats.JailRevertLook);
            ApplyJailOutfit(habbo);
            PlaceAtJailSpawn(habbo, GetJailSpawn());
        }

        public void SendToJail(Habbo habbo)
        {
            JailSpawn spawn = GetJailSpawn();
            if (habbo == null || spawn == null)
                return;

            if (habbo.CurrentRoomId != spawn.roomid)
                habbo.PrepareRoom(spawn.roomid, string.Empty);
            else
                PlaceAtJailSpawn(habbo, spawn);
        }

        private static void PlaceAtJailSpawn(Habbo habbo, JailSpawn spawn)
        {
            Room room = habbo?.CurrentRoom;
            if (room == null || spawn == null || room.RoomId != spawn.roomid || spawn.x < 0 || spawn.y < 0)
                return;

            RoomUser roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (roomUser == null || !room.GetGameMap().ValidTile(spawn.x, spawn.y))
                return;

            roomUser.ClearMovement(true);
            room.GetGameMap().UpdateUserMovement(new Point(roomUser.X, roomUser.Y), new Point(spawn.x, spawn.y), roomUser);
            roomUser.SetPos(spawn.x, spawn.y, room.GetGameMap().Model.SqFloorHeight[spawn.x, spawn.y]);
            roomUser.SetRot(spawn.rotation, false);
            roomUser.UpdateNeeded = true;
            room.GetRoomUserManager().UpdateUserStatus(roomUser, false);
        }

        public void Forget(Habbo habbo)
        {
            UserRpStats stats = habbo?.GetRpStats();
            if (stats == null)
                return;

            _jailed.TryRemove(habbo.Id, out _);
            stats.JailReleaseTime = 0;
            stats.JailPending = false;
            stats.JailStars = 0;
            stats.JailRoomId = 0;
            stats.JailTimeLeft = 0;
        }

        public void Pardon(Habbo habbo)
        {
            UserRpStats stats = habbo?.GetRpStats();
            if (stats == null)
                return;

            ClearSentence(habbo, stats, restoreLook: true, whisper: false);
        }

        public void OnDisconnect(Habbo habbo)
        {
            UserRpStats stats = habbo?.GetRpStats();
            if (stats == null)
                return;

            _jailed.TryRemove(habbo.Id, out _);

            double now = PlusEnvironment.GetUnixTimestamp();
            if (stats.JailReleaseTime > now) {
                stats.JailTimeLeft = stats.JailReleaseTime - now;
                stats.JailReleaseTime = 0;
                habbo.SaveRpStats();
            }
        }

        private void ApplyJailOutfit(Habbo habbo)
        {
            string outfit = GetJailOutfit();

            if (string.IsNullOrEmpty(outfit))
                return;

            string gender = habbo.Gender.ToLower();
            string figureString = outfit;
            string selectedFigure = null;

            if (figureString.Contains('|')) {
                Dictionary<string, string> figures = new();
                foreach (string part in figureString.Split(';')) {
                    if (string.IsNullOrWhiteSpace(part))
                        continue;

                    string[] split = part.Split('|');
                    if (split.Length != 2)
                        continue;

                    figures[split[0].ToLower()] = split[1];
                }

                if (figures.TryGetValue(gender, out string fig))
                    selectedFigure = fig;
                else if (figures.TryGetValue("m", out fig))
                    selectedFigure = fig;
            } else {
                selectedFigure = figureString;
            }

            if (string.IsNullOrWhiteSpace(selectedFigure))
                return;

            Dictionary<string, string> lookParts = new();

            foreach (string part in habbo.Look.Split('.')) {
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                string[] split = part.Split('-');
                if (split.Length == 0)
                    continue;

                lookParts[split[0]] = part;
            }

            foreach (string part in selectedFigure.Split('.')) {
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                string[] split = part.Split('-');
                if (split.Length == 0)
                    continue;

                lookParts[split[0]] = part;
            }

            habbo.Look = string.Join(".", lookParts.Values);

            RoomUser roomUser = habbo.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (roomUser != null)
                habbo.CurrentRoom.SendPacket(new UserChangeComposer(roomUser, false));
        }

        public void OnCycle()
        {
            double now = PlusEnvironment.GetUnixTimestamp();
            foreach (var kv in _jailed) {
                GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(kv.Key);
                UserRpStats stats = client?.GetHabbo()?.GetRpStats();

                if (stats == null) {
                    _jailed.TryRemove(kv.Key, out _); // offline: resolved on next login
                    continue;
                }

                if (stats.JailReleaseTime <= 0) {
                    _jailed.TryRemove(kv.Key, out _);
                    continue;
                }

                if (now >= stats.JailReleaseTime)
                    Release(client.GetHabbo());
            }
        }

        public void Release(Habbo habbo)
        {
            UserRpStats stats = habbo?.GetRpStats();
            if (stats == null)
                return;

            ClearSentence(habbo, stats, restoreLook: true, whisper: true);
        }

        private void ClearSentence(Habbo habbo, UserRpStats stats, bool restoreLook, bool whisper)
        {
            _jailed.TryRemove(habbo.Id, out _);

            stats.JailReleaseTime = 0;
            stats.JailPending = false;
            stats.JailStars = 0;
            stats.JailRoomId = 0;
            stats.JailTimeLeft = 0;
            stats.IsCuffed = false;
            string revertLook = stats.JailRevertLook;
            stats.JailRevertLook = "";
            habbo.SaveRpStats();

            PlusEnvironment.GetPoliceManager().Uncuff(habbo);
            if (restoreLook && !string.IsNullOrEmpty(revertLook)) {
                habbo.Look = revertLook;
                BroadcastLook(habbo);
            }

            if (whisper)
                habbo.GetClient()?.SendWhisper("Your sentence is over — you're free to go.", 1);
        }

        private static void TakeFine(Habbo habbo, int fine)
        {
            if (fine <= 0)
                return;

            habbo.Credits = System.Math.Max(0, habbo.Credits - fine);
            habbo.GetClient()?.SendPacket(new CreditBalanceComposer(habbo.Credits));
        }

        private static void BroadcastLook(Habbo habbo)
        {
            Room room = habbo.CurrentRoom;
            RoomUser roomUser = room?.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (roomUser != null)
                room.SendPacket(new UserChangeComposer(roomUser, false));
        }
    }
}