using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("wordfilter")]
[Index("word", Name = "word", IsUnique = true)]
public partial class wordfilter
{
    [Key]
    [StringLength(100)]
    public string word { get; set; }

    [Required]
    [StringLength(255)]
    public string replacement { get; set; }

    [Required]
    [Column(TypeName = "enum('1','0')")]
    public string strict { get; set; }

    [Required]
    [StringLength(100)]
    public string addedby { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string bannable { get; set; }
}
