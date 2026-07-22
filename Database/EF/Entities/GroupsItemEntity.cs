using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[PrimaryKey("Id", "Type")]
[Table("groups_items")]
public partial class GroupsItemEntity
{
    [Key]
    [Column("type", TypeName = "enum('base','symbol','color','color2','color3')")]
    public string Type { get; set; }

    [Key]
    [Column("id", TypeName = "int(255)")]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    [Column("firstvalue")]
    public string Firstvalue { get; set; }

    [Required]
    [StringLength(2000)]
    [Column("secondvalue")]
    public string Secondvalue { get; set; }

    [Required]
    [Column("enabled", TypeName = "enum('0','1')")]
    public string Enabled { get; set; }
}