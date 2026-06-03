using System;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Bots
{
    public class TaxiRideRequest
    {
        public int UserId { get; set; }
        public int SourceRoomId { get; set; }
        public int TargetRoomId { get; set; }
        public int TaxiBotTemplateId { get; set; }
        public DateTime ArrivalAtUtc { get; set; }
        public int SpawnX { get; set; }
        public int SpawnY { get; set; }
        public double SpawnZ { get; set; }
        public int SpawnRotation { get; set; }
        public int ExitSpawnX { get; set; }
        public int ExitSpawnY { get; set; }
        public double ExitSpawnZ { get; set; }
        public int ExitSpawnRotation { get; set; }
        public int SourceCost { get; set; }
        public int TaxiVirtualId { get; set; }
        public int ExitTaxiVirtualId { get; set; }
        public bool Spawned { get; set; }
        public bool PassengerBoarded { get; set; }
        public DateTime PassengerBoardedAtUtc { get; set; }
        public bool Arrived { get; set; }
        public int EscortTicks { get; set; }
        public bool ArrivalMessageSent { get; set; }
        public DateTime ArrivalMessageAtUtc { get; set; }
        public DateTime ExitRemovalAtUtc { get; set; }
        public int ExitTargetX { get; set; }
        public int ExitTargetY { get; set; }
        public DateTime ExitTimeoutAtUtc { get; set; }
        public RoomUser TaxiUser { get; set; }
    }
}
