using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("SkillId", Name = "idx_user_rp_skills_skill_id")]
[Index("UserId", "SkillId", Name = "uniq_user_rp_skills_user_skill", IsUnique = true)]
[Table("user_rp_skills")]
public partial class UserRpSkillEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Column("skill_id", TypeName = "int(11)")]
    public int SkillId { get; set; }

    [Column("progress", TypeName = "int(11)")]
    public int Progress { get; set; }

    [Column("equipped")]
    public bool Equipped { get; set; }
}