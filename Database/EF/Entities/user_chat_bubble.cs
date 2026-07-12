using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("user_id", Name = "idx_user_chat_bubbles_user_id")]
[Index("user_id", "chat_bubble_id", Name = "uniq_user_bubble", IsUnique = true)]
public partial class user_chat_bubble
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int chat_bubble_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? created_at { get; set; }

    [ForeignKey("user_id")]
    [InverseProperty("user_chat_bubbles")]
    public virtual user user { get; set; }
}
