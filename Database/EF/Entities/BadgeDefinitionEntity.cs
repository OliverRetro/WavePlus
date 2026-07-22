using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("Code", Name = "code", IsUnique = true)]
[Table("badge_definitions")]
public partial class BadgeDefinitionEntity
{
    [Key]
    [StringLength(35)]
    [Column("code")]
    public string Code { get; set; }

    [Required]
    [StringLength(25)]
    [Column("required_right")]
    public string RequiredRight { get; set; }
}