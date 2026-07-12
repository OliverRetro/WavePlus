using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("skill_id", Name = "idx_user_rp_skills_skill_id")]
[Index("user_id", "skill_id", Name = "uniq_user_rp_skills_user_skill", IsUnique = true)]
public partial class user_rp_skill
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int skill_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int progress { get; set; }

    public bool equipped { get; set; }
}
