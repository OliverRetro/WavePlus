using System.Collections.Concurrent;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.Crime
{
    /// <summary>
    /// Handles jailing offenders: teleport to the jail room, swap to the jail outfit, hold
    /// them there until their sentence is up, then revert their look. Sentences are tracked
    /// in memory only.
    /// </summary>
    public sealed class JailManager
    {
        private readonly ConcurrentDictionary<int, JailSentence> _jailed = new();

        public int GetJailRoomId() =>
            int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue("rp.jail.room.id"), out int id) ? id : 0;

        public string GetJailOutfit() => PlusEnvironment.GetSettingsManager().TryGetValue("rp.jail.outfit");

        public bool IsJailed(int userId) => _jailed.ContainsKey(userId);

        /// <summary>Jails the offender for the penalty matching <paramref name="stars"/>.</summary>
        public bool Jail(Habbo habbo, int stars)
        {
            if (habbo == null)
                return false;

            RpCrimePenalty penalty = PlusEnvironment.GetRpCrimeManager().GetPenalty(stars);
            int jailMinutes = penalty?.JailTimeMinutes ?? 0;
            if (jailMinutes <= 0)
                return false;

            // Fine the offender.
            int fine = penalty.FineAmount;
            if (fine > 0) {
                habbo.Credits = System.Math.Max(0, habbo.Credits - fine);
                habbo.GetClient()?.SendPacket(new CreditBalanceComposer(habbo.Credits));
            }

            // A jailed worker stops their shift (this restores their work outfit first).
            if (PlusEnvironment.GetGame().GetShiftManager().IsWorking(habbo.Id))
                PlusEnvironment.GetGame().GetShiftManager().InterruptShift(habbo, habbo.CurrentRoom);

            int jailRoom = GetJailRoomId();
            _jailed[habbo.Id] = new JailSentence
            {
                ReleaseUnix = PlusEnvironment.GetUnixTimestamp() + jailMinutes * 60.0,
                RevertLook = habbo.Look,
                JailRoomId = jailRoom
            };

            ApplyJailOutfit(habbo);

            // Send them to the jail room.
            if (jailRoom > 0 && habbo.CurrentRoomId != jailRoom)
                habbo.PrepareRoom(jailRoom, string.Empty);
            else
                BroadcastLook(habbo);

            habbo.GetClient()?.SendWhisper($"You've been jailed for {jailMinutes} minute(s).", 1);
            return true;
        }

        /// <summary>Returns the jail room id when the user is still serving time (login redirect).</summary>
        public int GetLoginRoomId(Habbo habbo)
        {
            if (habbo == null || !IsJailed(habbo.Id))
                return 0;

            return GetJailRoomId();
        }

        /// <summary>On entering any room: a jailed user is forced back to the jail room.</summary>
        public void HandleRoomEntry(Habbo habbo)
        {
            if (habbo == null || habbo.CurrentRoom == null || !_jailed.TryGetValue(habbo.Id, out JailSentence sentence))
                return;

            int jailRoom = GetJailRoomId();
            if (jailRoom > 0 && habbo.CurrentRoom.RoomId != jailRoom) {
                habbo.PrepareRoom(jailRoom, string.Empty);
                return;
            }

            // In the jail room — make sure the jail outfit is showing.
            ApplyJailOutfit(habbo);
        }

        /// <summary>Game-loop tick: release anyone whose sentence has expired.</summary>
        public void OnCycle()
        {
            double now = PlusEnvironment.GetUnixTimestamp();
            foreach (var kv in _jailed)
            {
                if (now < kv.Value.ReleaseUnix)
                    continue;

                GameClients.GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(kv.Key);
                if (client?.GetHabbo() != null)
                    Release(client.GetHabbo());
                else
                    _jailed.TryRemove(kv.Key, out _); // offline: just drop the sentence
            }
        }

        public void Release(Habbo habbo)
        {
            if (!_jailed.TryRemove(habbo.Id, out JailSentence sentence))
                return;

            if (!string.IsNullOrEmpty(sentence.RevertLook)) {
                habbo.Look = sentence.RevertLook;
                BroadcastLook(habbo);
            }

            habbo.GetClient()?.SendWhisper("Your sentence is over — you're free to go.", 1);
        }

        private void ApplyJailOutfit(Habbo habbo)
        {
            string outfit = GetJailOutfit();
            if (string.IsNullOrWhiteSpace(outfit))
                return;

            habbo.Look = outfit;
            BroadcastLook(habbo);
        }

        private static void BroadcastLook(Habbo habbo)
        {
            Room room = habbo.CurrentRoom;
            RoomUser roomUser = room?.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (roomUser != null)
                room.SendPacket(new UserChangeComposer(roomUser, false));
        }

        private sealed class JailSentence
        {
            public double ReleaseUnix;
            public string RevertLook;
            public int JailRoomId;
        }
    }
}
