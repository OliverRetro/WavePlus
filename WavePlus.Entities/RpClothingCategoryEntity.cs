using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("rp_clothing_categories")]
public partial class RpClothingCategoryEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    [Column("tab_name")]
    public string TabName { get; set; }

    [Column("room_id", TypeName = "int(11)")]
    public int RoomId { get; set; }
}