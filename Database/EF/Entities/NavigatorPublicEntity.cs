using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("OrderNum", Name = "ordernum")]
[Table("navigator_publics")]
public partial class NavigatorPublicEntity
{
    [Key]
    [Column("room_id", TypeName = "int(11)")]
    public int RoomId { get; set; }

    [Required]
    [StringLength(64)]
    [Column("caption")]
    public string Caption { get; set; }

    [Required]
    [StringLength(150)]
    [Column("description")]
    public string Description { get; set; }

    [Required]
    [Column("image_url", TypeName = "text")]
    public string ImageUrl { get; set; }

    [Column("order_num", TypeName = "int(11)")]
    public int OrderNum { get; set; }

    [Required]
    [Column("enabled", TypeName = "enum('0','1')")]
    public string Enabled { get; set; }
}