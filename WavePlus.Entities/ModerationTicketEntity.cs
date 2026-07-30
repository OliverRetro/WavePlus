using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("Id", Name = "id", IsUnique = true)]
[Index("ModeratorId", Name = "moderator_id")]
[Index("ReportedId", Name = "reported_id")]
[Index("SenderId", Name = "sender_id")]
[Index("Status", Name = "status")]
[Table("moderation_tickets")]
public partial class ModerationTicketEntity
{
    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }

    [Column("score", TypeName = "int(11)")]
    public int Score { get; set; }

    [Column("type", TypeName = "int(11)")]
    public int Type { get; set; }

    [Required]
    [Column("status", TypeName = "enum('open','picked','resolved','abusive','invalid','deleted')")]
    public string Status { get; set; }

    [Column("sender_id", TypeName = "int(10) unsigned")]
    public uint SenderId { get; set; }

    [Column("reported_id", TypeName = "int(10) unsigned")]
    public uint ReportedId { get; set; }

    [Column("moderator_id", TypeName = "int(10) unsigned")]
    public uint ModeratorId { get; set; }

    [Required]
    [Column("message", TypeName = "text")]
    public string Message { get; set; }

    [Column("room_id", TypeName = "int(10) unsigned")]
    public uint RoomId { get; set; }

    [Required]
    [StringLength(100)]
    [Column("room_name")]
    public string RoomName { get; set; }

    [Column("timestamp")]
    public double Timestamp { get; set; }
}