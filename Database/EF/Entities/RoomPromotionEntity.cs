using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("room_promotions")]
public partial class RoomPromotionEntity
{
    [Key]
    [Column("room_id", TypeName = "int(11)")]
    public int RoomId { get; set; }

    [Required]
    [StringLength(35)]
    [Column("title")]
    public string Title { get; set; }

    [Required]
    [StringLength(220)]
    [Column("description")]
    public string Description { get; set; }

    [Column("timestamp_start")]
    public double TimestampStart { get; set; }

    [Column("timestamp_expire")]
    public double TimestampExpire { get; set; }

    [Column("category_id", TypeName = "int(11)")]
    public int CategoryId { get; set; }
}