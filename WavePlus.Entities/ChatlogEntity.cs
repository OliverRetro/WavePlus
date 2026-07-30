using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("RoomId", Name = "room_id")]
[Index("UserId", Name = "user_id")]
[Table("chatlogs")]
public partial class ChatlogEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(10) unsigned")]
    public uint UserId { get; set; }

    [Column("room_id", TypeName = "int(10) unsigned")]
    public uint RoomId { get; set; }

    [Required]
    [Column("message", TypeName = "text")]
    public string Message { get; set; }

    // The `timestamp` column is `double NOT NULL` in the schema; mapping it as int (the
    // scaffolded type) both truncated the value and mismatched ChatLogEntry.Timestamp (double).
    [Column("timestamp")]
    public double Timestamp { get; set; }
}