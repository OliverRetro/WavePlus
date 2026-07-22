using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("client_external_badge_texts")]
public partial class ClientExternalBadgeTextEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(35)]
    [Column("badge_code")]
    public string BadgeCode { get; set; }

    [Required]
    [StringLength(75)]
    [Column("badge_title")]
    public string BadgeTitle { get; set; }

    [Required]
    [StringLength(150)]
    [Column("badge_desc")]
    public string BadgeDesc { get; set; }
}