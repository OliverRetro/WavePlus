using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("moderation_preset_action_messages")]
public partial class ModerationPresetActionMessageEntity
{
    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }

    [Column("parent_id", TypeName = "int(10) unsigned")]
    public uint ParentId { get; set; }

    [Required]
    [StringLength(32)]
    [Column("caption")]
    public string Caption { get; set; }

    [Required]
    [Column("message_text", TypeName = "text")]
    public string MessageText { get; set; }

    [Column("mute_hours", TypeName = "int(11)")]
    public int MuteHours { get; set; }

    [Column("ban_hours", TypeName = "int(11)")]
    public int BanHours { get; set; }

    [Column("ip_ban_hours", TypeName = "int(11)")]
    public int IpBanHours { get; set; }

    [Column("trade_lock_days", TypeName = "int(11)")]
    public int TradeLockDays { get; set; }

    [Required]
    [Column("notice", TypeName = "text")]
    public string Notice { get; set; }
}