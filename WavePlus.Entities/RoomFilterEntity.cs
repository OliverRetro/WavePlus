using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("room_filter")]
[Index("RoomId", Name = "room_id")]
[Index("Word", Name = "word")]
public partial class RoomFilterEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(15)]
    [Column("word")]
    public string Word { get; set; }

    [Column("room_id", TypeName = "int(11)")]
    public int RoomId { get; set; }
}