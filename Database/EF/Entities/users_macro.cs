using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("user_id", "name", Name = "uniq_user_macro", IsUnique = true)]
public partial class users_macro
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [Required]
    [Column(TypeName = "tinytext")]
    public string name { get; set; }

    [Required]
    [StringLength(16000)]
    public string configs { get; set; }
}
