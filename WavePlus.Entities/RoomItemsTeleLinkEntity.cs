using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("TeleOneId", Name = "tele_one_id")]
[Table("room_items_tele_links")]
public partial class RoomItemsTeleLinkEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("tele_one_id", TypeName = "int(10) unsigned")]
    public uint TeleOneId { get; set; }

    [Column("tele_two_id", TypeName = "int(10) unsigned")]
    public uint TeleTwoId { get; set; }
}