using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("group_roles")]
public partial class GroupRoleEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    /// <summary>
    /// GroupEntity&apos;s ID.
    /// </summary>
    [Column("group_id", TypeName = "int(11)")]
    public int GroupId { get; set; }

    /// <summary>
    /// Levels go in order of Slave -&gt; CEO. 1 = Slave, 100 = CEO.
    /// </summary>
    [Column("level", TypeName = "int(11)")]
    public int Level { get; set; }

    /// <summary>
    /// Literal name displayed; ex. general manager
    /// </summary>
    [Required]
    [StringLength(55)]
    [Column("name")]
    public string Name { get; set; }

    /// <summary>
    /// Credits paid via intervals.
    /// </summary>
    [Column("shift_pay", TypeName = "int(11)")]
    public int ShiftPay { get; set; }

    /// <summary>
    /// Shift-Costume (full) to replace users own clothes. Blank for no swap.
    /// </summary>
    [Required]
    [StringLength(255)]
    [Column("shift_costume")]
    public string ShiftCostume { get; set; }

    /// <summary>
    /// The duration at which worker will be auto-rewarded. In minutes.
    /// </summary>
    [Column("shift_duration", TypeName = "smallint(6)")]
    public short? ShiftDuration { get; set; }

    /// <summary>
    /// Replace users motto for the duration of shift? Blank for no swap.
    /// </summary>
    [Required]
    [StringLength(40)]
    [Column("shift_motto")]
    public string ShiftMotto { get; set; }

    /// <summary>Unix timestamp (seconds) the role was created.</summary>
    [Column("created_at", TypeName = "int(11)")]
    public int CreatedAt { get; set; }

    /// <summary>Unix timestamp (seconds) the role was last changed, if ever.</summary>
    [Column("updated_at", TypeName = "int(11)")]
    public int? UpdatedAt { get; set; }
}
