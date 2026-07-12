using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("OnlineTime", Name = "OnlineTime")]
[Index("Respect", Name = "Respect")]
[Index("id", Name = "id", IsUnique = true)]
public partial class user_stat
{
    [Key]
    [Column(TypeName = "int(7)")]
    public int id { get; set; }

    [Column(TypeName = "int(7)")]
    public int RoomVisits { get; set; }

    [Column(TypeName = "int(7)")]
    public int OnlineTime { get; set; }

    [Column(TypeName = "int(6)")]
    public int Respect { get; set; }

    [Column(TypeName = "int(6)")]
    public int RespectGiven { get; set; }

    [Column(TypeName = "int(6)")]
    public int GiftsGiven { get; set; }

    [Column(TypeName = "int(6)")]
    public int GiftsReceived { get; set; }

    [Column(TypeName = "int(1)")]
    public int DailyRespectPoints { get; set; }

    [Column(TypeName = "int(1)")]
    public int DailyPetRespectPoints { get; set; }

    [Column(TypeName = "int(7)")]
    public int AchievementScore { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint quest_id { get; set; }

    [Column(TypeName = "int(10)")]
    public int quest_progress { get; set; }

    [Column(TypeName = "int(10)")]
    public int lev_builder { get; set; }

    [Column(TypeName = "int(10)")]
    public int lev_social { get; set; }

    [Column(TypeName = "int(10)")]
    public int lev_identity { get; set; }

    [Column(TypeName = "int(10)")]
    public int lev_explore { get; set; }

    [Column(TypeName = "int(11)")]
    public int groupid { get; set; }

    [Column(TypeName = "int(11)")]
    public int tickets_answered { get; set; }

    [StringLength(6)]
    public string respectsTimestamp { get; set; }

    [Column(TypeName = "int(11)")]
    public int forum_posts { get; set; }
}
