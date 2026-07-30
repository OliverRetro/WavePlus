using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Keyless]
[Table("user_xmas15_calendar")]
public partial class UserXmas15CalendarEntity
{
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("status")]
    public int Status { get; set; }

    [Column("day")]
    public int Day { get; set; }
}