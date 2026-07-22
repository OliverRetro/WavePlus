using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("moderation_topic_actions")]
public partial class ModerationTopicActionEntity
{
    [Key]
    [Column("id", TypeName = "int(11) unsigned")]
    public uint Id { get; set; }

    [Column("parent_id", TypeName = "int(11)")]
    public int ParentId { get; set; }

    [Required]
    [StringLength(255)]
    [Column("type")]
    public string Type { get; set; }

    [Required]
    [StringLength(225)]
    [Column("caption")]
    public string Caption { get; set; }

    [Required]
    [StringLength(255)]
    [Column("message_text")]
    public string MessageText { get; set; }

    [Required]
    [StringLength(255)]
    [Column("default_sanction")]
    public string DefaultSanction { get; set; }

    [Column("mute_time", TypeName = "int(11)")]
    public int MuteTime { get; set; }

    [Column("ban_time", TypeName = "int(11)")]
    public int BanTime { get; set; }

    [Column("ip_time", TypeName = "int(11)")]
    public int IpTime { get; set; }

    [Column("trade_lock_time", TypeName = "int(11)")]
    public int TradeLockTime { get; set; }
}