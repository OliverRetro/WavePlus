using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("group_roles")]
public partial class GroupRoleEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("group_id", TypeName = "int(11)")]
    public int GroupId { get; set; }

    [Column("level", TypeName = "int(11)")]
    public int Level { get; set; }

    [Required]
    [StringLength(55)]
    [Column("name")]
    public string Name { get; set; }

    [Column("shift_pay", TypeName = "int(11)")]
    public int ShiftPay { get; set; }

    [Required]
    [StringLength(255)]
    [Column("shift_costume")]
    public string ShiftCostume { get; set; }

    [Column("shift_duration", TypeName = "smallint(6)")]
    public short? ShiftDuration { get; set; }

    [Required]
    [StringLength(40)]
    [Column("shift_motto")]
    public string ShiftMotto { get; set; }

    [Column("created_at", TypeName = "int(11)")]
    public int CreatedAt { get; set; }

    [Column("updated_at", TypeName = "int(11)")]
    public int? UpdatedAt { get; set; }
}