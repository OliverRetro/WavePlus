using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("user_id", Name = "uniq_user_rp_statistics_user_id", IsUnique = true)]
public partial class user_rp_statistic
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int health { get; set; }

    [Column(TypeName = "int(11)")]
    public int shield { get; set; }

    [Column(TypeName = "int(11)")]
    public int energy { get; set; }

    [Column(TypeName = "int(11)")]
    public int hunger { get; set; }

    [Column(TypeName = "int(11)")]
    public int experience { get; set; }

    [Column(TypeName = "int(11)")]
    public int knockouts { get; set; }

    [Column(TypeName = "int(11)")]
    public int deaths { get; set; }

    [Column(TypeName = "int(11)")]
    public int arrested { get; set; }

    [Column(TypeName = "int(11)")]
    public int escapes { get; set; }

    [Column(TypeName = "bigint(20)")]
    public long damage_dealt { get; set; }

    [Column(TypeName = "bigint(20)")]
    public long damage_taken { get; set; }

    [Column(TypeName = "int(11)")]
    public int attacks { get; set; }

    [Column(TypeName = "int(11)")]
    public int attacked { get; set; }

    [Column(TypeName = "int(11)")]
    public int knowledge { get; set; }

    [Column(TypeName = "int(11)")]
    public int strength { get; set; }

    [Column(TypeName = "int(11)")]
    public int is_dead { get; set; }

    public double hospital_release_time { get; set; }

    public double hospital_heal_start_time { get; set; }

    [Column(TypeName = "int(11)")]
    public int hospital_health_start { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short aggression { get; set; }

    [Column(TypeName = "tinyint(4)")]
    public sbyte passive_mode { get; set; }

    [Column(TypeName = "int(11)")]
    public int robberies { get; set; }

    [Column(TypeName = "int(11)")]
    public int robbed { get; set; }

    public double jail_release_time { get; set; }

    public bool jail_pending { get; set; }

    [Column(TypeName = "int(11)")]
    public int jail_stars { get; set; }

    [Required]
    [StringLength(512)]
    public string jail_revert_look { get; set; }

    [Column(TypeName = "int(11)")]
    public int jail_room_id { get; set; }

    public bool is_cuffed { get; set; }

    public double jail_time_left { get; set; }
}
