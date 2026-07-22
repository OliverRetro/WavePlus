using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("user_clothing")]
[Index("UserId", Name = "user_id")]
public partial class UserClothingEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Required]
    [StringLength(25)]
    [Column("part_id")]
    public string PartId { get; set; }

    [Required]
    [StringLength(45)]
    [Column("part")]
    public string Part { get; set; }
}