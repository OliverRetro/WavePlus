using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("EntryTimestamp", Name = "entry_timestamp")]
[Index("ExitTimestamp", Name = "exit_timestamp")]
[Index("UserId", Name = "user_id")]
[Table("user_roomvisits")]
public partial class UserRoomvisitEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(10) unsigned")]
    public uint UserId { get; set; }

    [Column("room_id", TypeName = "int(10) unsigned")]
    public uint RoomId { get; set; }

    [Column("entry_timestamp")]
    public double EntryTimestamp { get; set; }

    [Column("exit_timestamp")]
    public double ExitTimestamp { get; set; }

    [Column("hour", TypeName = "int(11)")]
    public int Hour { get; set; }

    [Column("minute", TypeName = "int(11)")]
    public int Minute { get; set; }
}