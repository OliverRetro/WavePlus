using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("UserId", Name = "idx_user_chat_bubbles_user_id")]
[Index("UserId", "ChatBubbleId", Name = "uniq_user_bubble", IsUnique = true)]
[Table("user_chat_bubbles")]
public partial class UserChatBubbleEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("chat_bubble_id", TypeName = "int(11)")]
    public int ChatBubbleId { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Column("created_at", TypeName = "int(11)")]
    public int? CreatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserChatBubbles")]
    public virtual UserEntity User { get; set; }
}