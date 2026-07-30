using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("catalog_vouchers")]
public partial class CatalogVoucherEntity
{
    [Key]
    [StringLength(45)]
    [Column("voucher")]
    public string Voucher { get; set; }

    [Required]
    [Column("type", TypeName = "enum('credits','duckets')")]
    public string Type { get; set; }

    [Column("value", TypeName = "int(11)")]
    public int Value { get; set; }

    [Column("current_uses", TypeName = "int(11)")]
    public int CurrentUses { get; set; }

    [Column("max_uses", TypeName = "int(11)")]
    public int MaxUses { get; set; }

    [Column("enabled", TypeName = "enum('0','1')")]
    public string Enabled { get; set; }
}