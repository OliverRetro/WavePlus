using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("room_filter")]
[Index("room_id", Name = "room_id")]
[Index("word", Name = "word")]
public partial class room_filter
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [StringLength(15)]
    public string word { get; set; }

    [Column(TypeName = "int(11)")]
    public int room_id { get; set; }
}
