using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("UserOneId", Name = "user_one_id")]
[Index("UserTwoId", Name = "user_two_id")]
[Table("messenger_friendships")]
public partial class MessengerFriendshipEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_one_id", TypeName = "int(10) unsigned")]
    public uint UserOneId { get; set; }

    [Column("user_two_id", TypeName = "int(10) unsigned")]
    public uint UserTwoId { get; set; }
}