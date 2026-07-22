using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Keyless]
[Index("GroupId", Name = "groupid")]
[Index("UserId", Name = "userid")]
[Table("group_requests")]
public partial class GroupRequestEntity
{
    [Column("group_id", TypeName = "int(11) unsigned")]
    public uint GroupId { get; set; }

    [Column("user_id", TypeName = "int(11) unsigned")]
    public uint UserId { get; set; }
}