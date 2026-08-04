using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("user_rp_settings")]
public partial class UserRpSettingEntity
{
    [Key]
    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Column("clickthrough_enabled", TypeName = "tinyint(4)")]
    public sbyte ClickthroughEnabled { get; set; }

    [Column("auto_salary_deposit", TypeName = "tinyint(4)")]
    public sbyte AutoSalaryDeposit { get; set; }

    [Column("furniture_trading_enabled", TypeName = "tinyint(4)")]
    public sbyte FurnitureTradingEnabled { get; set; } = 1;

    [Column("auto_event_transfer", TypeName = "tinyint(4)")]
    public sbyte AutoEventTransfer { get; set; }

    [Column("voice_chat_enabled", TypeName = "tinyint(4)")]
    public sbyte VoiceChatEnabled { get; set; } = 1;

    [Column("livefeed_enabled", TypeName = "tinyint(4)")]
    public sbyte LivefeedEnabled { get; set; } = 1;

    [Column("alerts_enabled", TypeName = "tinyint(4)")]
    public sbyte AlertsEnabled { get; set; } = 1;

    [Column("link_warning_enabled", TypeName = "tinyint(4)")]
    public sbyte LinkWarningEnabled { get; set; } = 1;

    [Column("chat_higher_enabled", TypeName = "tinyint(4)")]
    public sbyte ChatHigherEnabled { get; set; }

    [Column("fps_max", TypeName = "int(11)")]
    public int FpsMax { get; set; } = 24;

    [Column("drag_rooms_enabled", TypeName = "tinyint(4)")]
    public sbyte DragRoomsEnabled { get; set; } = 1;
}