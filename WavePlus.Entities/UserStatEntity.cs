using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("OnlineTime", Name = "OnlineTime")]
[Index("Respect", Name = "Respect")]
[Index("Id", Name = "id", IsUnique = true)]
[Table("user_stats")]
public partial class UserStatEntity
{
    [Key]
    [Column("id", TypeName = "int(7)")]
    public int Id { get; set; }

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

    [Column("quest_id", TypeName = "int(10) unsigned")]
    public uint QuestId { get; set; }

    [Column("quest_progress", TypeName = "int(10)")]
    public int QuestProgress { get; set; }

    [Column("lev_builder", TypeName = "int(10)")]
    public int LevBuilder { get; set; }

    [Column("lev_social", TypeName = "int(10)")]
    public int LevSocial { get; set; }

    [Column("lev_identity", TypeName = "int(10)")]
    public int LevIdentity { get; set; }

    [Column("lev_explore", TypeName = "int(10)")]
    public int LevExplore { get; set; }

    [Column("groupid", TypeName = "int(11)")]
    public int Groupid { get; set; }

    [Column("tickets_answered", TypeName = "int(11)")]
    public int TicketsAnswered { get; set; }

    [StringLength(6)]
    [Column("respectsTimestamp")]
    public string RespectsTimestamp { get; set; }

    [Column("forum_posts", TypeName = "int(11)")]
    public int ForumPosts { get; set; }
}