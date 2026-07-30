using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("ItemId", Name = "item_id")]
[Table("user_presents")]
public partial class UserPresentEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("item_id", TypeName = "int(10) unsigned")]
    public uint ItemId { get; set; }

    [Column("base_id", TypeName = "int(10) unsigned")]
    public uint BaseId { get; set; }

    [Required]
    [Column("extra_data", TypeName = "text")]
    public string ExtraData { get; set; }
}