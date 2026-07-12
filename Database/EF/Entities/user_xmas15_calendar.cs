using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

/// <summary>
/// Read-only view of the legacy advent-calendar table. Modelled keyless because
/// the WavePlus code only ever SELECTs from it (CalendarComponent.Init).
/// </summary>
[Keyless]
public partial class user_xmas15_calendar
{
    public int user_id { get; set; }

    public int status { get; set; }

    public int day { get; set; }
}
