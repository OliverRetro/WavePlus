using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("UserId", "Equipped", Name = "idx_user_rp_items_equipped")]
[Index("UserId", "ItemId", Name = "idx_user_rp_items_user_item")]
[Table("user_rp_items")]
public partial class UserRpItemEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("item_id", TypeName = "int(11)")]
    public int ItemId { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Column("uses", TypeName = "smallint(6)")]
    public short Uses { get; set; }

    [Column("slot", TypeName = "int(11)")]
    public int Slot { get; set; }

    [Column("durability_left", TypeName = "int(11)")]
    public int DurabilityLeft { get; set; }

    [Column("equipped")]
    public bool Equipped { get; set; }
}