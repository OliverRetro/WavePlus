using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[PrimaryKey("Userid", "Group")]
[Index("Userid", Name = "id")]
[Table("user_achievements")]
public partial class UserAchievementEntity
{
    [Key]
    [Column("userid", TypeName = "int(11) unsigned")]
    public uint Userid { get; set; }

    [Key]
    [Column("group")]
    public string Group { get; set; }

    [Column("level", TypeName = "int(11)")]
    public int Level { get; set; }

    [Column("progress", TypeName = "int(11)")]
    public int Progress { get; set; }
}