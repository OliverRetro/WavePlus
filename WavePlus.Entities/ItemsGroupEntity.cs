using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("GroupId", Name = "group_id")]
[Index("Id", Name = "id", IsUnique = true)]
[Table("items_groups")]
public partial class ItemsGroupEntity
{
    [Key]
    [Column("id", TypeName = "int(11) unsigned")]
    public uint Id { get; set; }

    [Column("group_id", TypeName = "int(11)")]
    public int GroupId { get; set; }
}