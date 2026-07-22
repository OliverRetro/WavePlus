using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("Id", Name = "id", IsUnique = true)]
[Index("OrderNum", Name = "order_num")]
[Table("catalog_pages")]
public partial class CatalogPageEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("parent_id", TypeName = "int(11)")]
    public int ParentId { get; set; }

    [Required]
    [StringLength(35)]
    [Column("caption")]
    public string Caption { get; set; }

    [Column("icon_image", TypeName = "int(11)")]
    public int IconImage { get; set; }

    [Required]
    [Column("visible", TypeName = "enum('0','1')")]
    public string Visible { get; set; }

    [Required]
    [Column("enabled", TypeName = "enum('0','1')")]
    public string Enabled { get; set; }

    [Column("min_rank", TypeName = "int(10) unsigned")]
    public uint MinRank { get; set; }

    [Column("min_vip", TypeName = "int(11)")]
    public int MinVip { get; set; }

    [Column("order_num", TypeName = "int(11)")]
    public int OrderNum { get; set; }

    [Required]
    [StringLength(35)]
    [Column("page_link")]
    public string PageLink { get; set; }

    [Required]
    [StringLength(35)]
    [Column("page_layout")]
    public string PageLayout { get; set; }

    [Required]
    [StringLength(555)]
    [Column("page_strings_1")]
    public string PageStrings1 { get; set; }

    [Required]
    [StringLength(2048)]
    [Column("page_strings_2")]
    public string PageStrings2 { get; set; }
}