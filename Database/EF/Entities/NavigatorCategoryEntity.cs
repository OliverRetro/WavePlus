using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("navigator_categories")]
public partial class NavigatorCategoryEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [Column("category", TypeName = "enum('official_view','hotel_view','myworld_view','roomads_view','query')")]
    public string Category { get; set; }

    [Required]
    [StringLength(35)]
    [Column("category_identifier")]
    public string CategoryIdentifier { get; set; }

    [Required]
    [StringLength(35)]
    [Column("public_name")]
    public string PublicName { get; set; }

    [Required]
    [Column("view_mode", TypeName = "enum('REGULAR','THUMBNAIL')")]
    public string ViewMode { get; set; }

    [Column("required_rank", TypeName = "int(11)")]
    public int RequiredRank { get; set; }

    [Required]
    [StringLength(25)]
    [Column("category_type")]
    public string CategoryType { get; set; }

    [Required]
    [Column("search_allowance", TypeName = "enum('NOTHING','SHOW_MORE')")]
    public string SearchAllowance { get; set; }

    [Required]
    [Column("enabled", TypeName = "enum('0','1')")]
    public string Enabled { get; set; }

    [Column("order_id", TypeName = "int(11)")]
    public int OrderId { get; set; }
}