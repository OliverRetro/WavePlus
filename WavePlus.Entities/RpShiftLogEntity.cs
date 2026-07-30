using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("rp_shift_logs")]
public partial class RpShiftLogEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("group_id", TypeName = "int(11)")]
    public int GroupId { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Column("credits_paid", TypeName = "int(11)")]
    public int CreditsPaid { get; set; }

    [Column("shift_finished", TypeName = "int(11)")]
    public int ShiftFinished { get; set; }
}