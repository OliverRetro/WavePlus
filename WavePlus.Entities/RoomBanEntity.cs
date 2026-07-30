using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[PrimaryKey("UserId", "RoomId")]
[Index("RoomId", Name = "room_id")]
[Index("UserId", Name = "user_id")]
[Table("room_bans")]
public partial class RoomBanEntity
{
    [Key]
    [Column("user_id", TypeName = "int(11) unsigned")]
    public uint UserId { get; set; }

    [Key]
    [Column("room_id", TypeName = "int(11) unsigned")]
    public uint RoomId { get; set; }

    [Column("expire", TypeName = "int(11)")]
    public int Expire { get; set; }
}