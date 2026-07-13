using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("rp_shift_logs")]
public partial class RpShiftLog
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

    [Column("shift_finished", TypeName = "datetime")]
    public DateTime ShiftFinished { get; set; }
}
