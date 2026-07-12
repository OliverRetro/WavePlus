using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("id", Name = "id", IsUnique = true)]
[Index("order_num", Name = "order_num")]
public partial class catalog_page
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int parent_id { get; set; }

    [Required]
    [StringLength(35)]
    public string caption { get; set; }

    [Column(TypeName = "int(11)")]
    public int icon_image { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string visible { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string enabled { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint min_rank { get; set; }

    [Column(TypeName = "int(11)")]
    public int min_vip { get; set; }

    [Column(TypeName = "int(11)")]
    public int order_num { get; set; }

    [Required]
    [StringLength(35)]
    public string page_link { get; set; }

    [Required]
    [StringLength(35)]
    public string page_layout { get; set; }

    [Required]
    [StringLength(555)]
    public string page_strings_1 { get; set; }

    [Required]
    [StringLength(2048)]
    public string page_strings_2 { get; set; }
}
