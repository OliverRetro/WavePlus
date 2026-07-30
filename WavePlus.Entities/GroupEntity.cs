using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("Id", Name = "id", IsUnique = true)]
[Index("OwnerId", Name = "owner")]
[Index("RoomId", Name = "room_id")]
[Table("groups")]
public partial class GroupEntity
{
    [Key]
    [Column("id", TypeName = "int(11) unsigned")]
    public uint Id { get; set; }

    [Required]
    [StringLength(50)]
    [Column("name")]
    public string Name { get; set; }

    [Required]
    [StringLength(255)]
    [Column("desc")]
    public string Desc { get; set; }

    [Required]
    [StringLength(50)]
    [Column("badge")]
    public string Badge { get; set; }

    [Column("owner_id", TypeName = "int(11) unsigned")]
    public uint OwnerId { get; set; }

    [Column("created", TypeName = "int(50)")]
    public int Created { get; set; }

    [Column("room_id", TypeName = "int(10) unsigned")]
    public uint RoomId { get; set; }

    [Required]
    [Column("state", TypeName = "enum('0','1','2')")]
    public string State { get; set; }

    [Column("colour1", TypeName = "int(11)")]
    public int Colour1 { get; set; }

    [Column("colour2", TypeName = "int(11)")]
    public int Colour2 { get; set; }

    [Required]
    [Column("admindeco", TypeName = "enum('0','1')")]
    public string Admindeco { get; set; }

    [Required]
    [Column("forum_enabled", TypeName = "enum('0','1')")]
    public string ForumEnabled { get; set; }

    [Column("group_type", TypeName = "smallint(6)")]
    public short GroupType { get; set; }
}