using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("rp_link_whitelist")]
public partial class RpLinkWhitelistEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("pattern")]
    [StringLength(255)]
    public string Pattern { get; set; } = null!;

    // 'domain' | 'wildcard' | 'prefix'
    [Column("match_type")]
    public string MatchType { get; set; } = "domain";

    [Column("enabled", TypeName = "tinyint(4)")]
    public sbyte Enabled { get; set; } = 1;

    // Admin-set favicon URL shown next to the link title in the confirm popup.
    // Never derived from the user's URL — tied to the matched rule.
    [Column("favicon")]
    [StringLength(255)]
    public string? Favicon { get; set; }
}