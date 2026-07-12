using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("items_youtube")]
[Index("id", Name = "id", IsUnique = true)]
public partial class items_youtube
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [StringLength(35)]
    public string youtube_id { get; set; }

    [Required]
    [StringLength(50)]
    public string title { get; set; }

    [Required]
    [StringLength(150)]
    public string description { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string enabled { get; set; }
}
