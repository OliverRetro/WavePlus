using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("UserId", "Name", Name = "uniq_user_macro", IsUnique = true)]
[Table("users_macros")]
public partial class UsersMacroEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Required]
    [Column("name", TypeName = "tinytext")]
    public string Name { get; set; }

    [Required]
    [StringLength(16000)]
    [Column("configs")]
    public string Configs { get; set; }
}