using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[PrimaryKey("UserId", "QuestId")]
[Table("user_quests")]
public partial class UserQuestEntity
{
    [Key]
    [Column("user_id", TypeName = "int(10) unsigned")]
    public uint UserId { get; set; }

    [Key]
    [Column("quest_id", TypeName = "int(10) unsigned")]
    public uint QuestId { get; set; }

    [Column("progress", TypeName = "int(10)")]
    public int? Progress { get; set; }
}