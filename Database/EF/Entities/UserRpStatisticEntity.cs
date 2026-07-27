using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("UserId", Name = "uniq_user_rp_statistics_user_id", IsUnique = true)]
[Table("user_rp_statistics")]
public partial class UserRpStatisticEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Column("health", TypeName = "int(11)")]
    public int Health { get; set; }

    [Column("shield", TypeName = "int(11)")]
    public int Shield { get; set; }

    [Column("energy", TypeName = "int(11)")]
    public int Energy { get; set; }

    [Column("hunger", TypeName = "int(11)")]
    public int Hunger { get; set; }

    [Column("experience", TypeName = "int(11)")]
    public int Experience { get; set; }

    [Column("knockouts", TypeName = "int(11)")]
    public int Knockouts { get; set; }

    [Column("deaths", TypeName = "int(11)")]
    public int Deaths { get; set; }

    [Column("arrested", TypeName = "int(11)")]
    public int Arrested { get; set; }

    [Column("escapes", TypeName = "int(11)")]
    public int Escapes { get; set; }

    [Column("damage_dealt", TypeName = "bigint(20)")]
    public long DamageDealt { get; set; }

    [Column("damage_taken", TypeName = "bigint(20)")]
    public long DamageTaken { get; set; }

    [Column("attacks", TypeName = "int(11)")]
    public int Attacks { get; set; }

    [Column("attacked", TypeName = "int(11)")]
    public int Attacked { get; set; }

    [Column("knowledge", TypeName = "int(11)")]
    public int Knowledge { get; set; }

    [Column("strength", TypeName = "int(11)")]
    public int Strength { get; set; }

    [Column("attribute_points", TypeName = "int(11)")]
    public int AttributePoints { get; set; }

    [Column("is_dead", TypeName = "int(11)")]
    public int IsDead { get; set; }

    [Column("hospital_release_time")]
    public double HospitalReleaseTime { get; set; }

    [Column("hospital_heal_start_time")]
    public double HospitalHealStartTime { get; set; }

    [Column("hospital_health_start", TypeName = "int(11)")]
    public int HospitalHealthStart { get; set; }

    [Column("aggression", TypeName = "smallint(6)")]
    public short Aggression { get; set; }

    [Column("passive_mode", TypeName = "tinyint(4)")]
    public sbyte PassiveMode { get; set; }

    [Column("robberies", TypeName = "int(11)")]
    public int Robberies { get; set; }

    [Column("robbed", TypeName = "int(11)")]
    public int Robbed { get; set; }

    [Column("jail_release_time")]
    public double JailReleaseTime { get; set; }

    [Column("jail_pending")]
    public bool JailPending { get; set; }

    [Column("jail_stars", TypeName = "int(11)")]
    public int JailStars { get; set; }

    [Required]
    [StringLength(512)]
    [Column("jail_revert_look")]
    public string JailRevertLook { get; set; }

    [Column("jail_room_id", TypeName = "int(11)")]
    public int JailRoomId { get; set; }

    [Column("is_cuffed")]
    public bool IsCuffed { get; set; }

    [Column("jail_time_left")]
    public double JailTimeLeft { get; set; }
}