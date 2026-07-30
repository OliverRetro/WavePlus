using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("GroupId", Name = "groupid")]
[Index("UserId", Name = "userid")]
[Table("group_memberships")]
public partial class GroupMembershipEntity
{
    [Key]
    [Column("id", TypeName = "int(11) unsigned")]
    public uint Id { get; set; }

    [Column("group_id", TypeName = "int(11) unsigned")]
    public uint GroupId { get; set; }

    [Column("user_id", TypeName = "int(11) unsigned")]
    public uint UserId { get; set; }

    [Column("level", TypeName = "smallint(6)")]
    public short Level { get; set; }
}