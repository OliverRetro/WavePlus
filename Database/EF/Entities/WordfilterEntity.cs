using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("wordfilter")]
[Index("Word", Name = "word", IsUnique = true)]
public partial class WordfilterEntity
{
    [Key]
    [StringLength(100)]
    [Column("word")]
    public string Word { get; set; }

    [Required]
    [StringLength(255)]
    [Column("replacement")]
    public string Replacement { get; set; }

    [Required]
    [Column("strict", TypeName = "enum('1','0')")]
    public string Strict { get; set; }

    [Required]
    [StringLength(100)]
    [Column("addedby")]
    public string Addedby { get; set; }

    [Required]
    [Column("bannable", TypeName = "enum('0','1')")]
    public string Bannable { get; set; }
}