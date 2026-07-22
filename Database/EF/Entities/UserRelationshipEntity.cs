using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("Target", Name = "target")]
[Index("Type", Name = "type")]
[Index("UserId", Name = "user_id")]
[Table("user_relationships")]
public partial class UserRelationshipEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Column("target", TypeName = "int(11)")]
    public int Target { get; set; }

    [Required]
    [Column("type", TypeName = "enum('1','2','3')")]
    public string Type { get; set; }
}