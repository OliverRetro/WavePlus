using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("furniture")]
[Index("Id", Name = "id", IsUnique = true)]
[Index("SpriteId", Name = "sprite_id")]
public partial class FurnitureEntity
{
    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }

    [Required]
    [StringLength(70)]
    [Column("item_name")]
    public string ItemName { get; set; }

    [Required]
    [StringLength(56)]
    [Column("public_name")]
    public string PublicName { get; set; }

    [Required]
    [Column("type", TypeName = "enum('s','i','e','h','v','r','b','p')")]
    public string Type { get; set; }

    [Column("width", TypeName = "int(11)")]
    public int Width { get; set; }

    [Column("length", TypeName = "int(11)")]
    public int Length { get; set; }

    [Column("stack_height")]
    public double StackHeight { get; set; }

    [Column("can_stack", TypeName = "tinyint(4)")]
    public sbyte CanStack { get; set; }

    [Column("can_sit", TypeName = "tinyint(4)")]
    public sbyte CanSit { get; set; }

    [Column("is_walkable", TypeName = "tinyint(4)")]
    public sbyte? IsWalkable { get; set; }

    [Column("sprite_id", TypeName = "int(11)")]
    public int? SpriteId { get; set; }

    [Required]
    [Column("allow_recycle", TypeName = "enum('0','1')")]
    public string AllowRecycle { get; set; }

    [Required]
    [Column("allow_trade", TypeName = "enum('0','1')")]
    public string AllowTrade { get; set; }

    [Required]
    [Column("allow_marketplace_sell", TypeName = "enum('0','1')")]
    public string AllowMarketplaceSell { get; set; }

    [Required]
    [Column("allow_gift", TypeName = "enum('0','1')")]
    public string AllowGift { get; set; }

    [Required]
    [Column("allow_inventory_stack", TypeName = "enum('0','1')")]
    public string AllowInventoryStack { get; set; }

    [Required]
    [StringLength(123)]
    [Column("interaction_type")]
    public string InteractionType { get; set; }

    [Column("behaviour_data", TypeName = "int(11)")]
    public int BehaviourData { get; set; }

    [Column("interaction_modes_count", TypeName = "int(11)")]
    public int InteractionModesCount { get; set; }

    [Required]
    [StringLength(255)]
    [Column("vending_ids")]
    public string VendingIds { get; set; }

    [Required]
    [StringLength(50)]
    [Column("height_adjustable")]
    public string HeightAdjustable { get; set; }

    [Column("effect_id", TypeName = "int(11)")]
    public int EffectId { get; set; }

    [Column("wired_id", TypeName = "int(11)")]
    public int WiredId { get; set; }

    [Required]
    [Column("is_rare", TypeName = "enum('0','1')")]
    public string IsRare { get; set; }

    [Column("clothing_id", TypeName = "int(11)")]
    public int ClothingId { get; set; }

    [Required]
    [Column("extra_rot", TypeName = "enum('0','1')")]
    public string ExtraRot { get; set; }

    [Column("allow_lay", TypeName = "int(11)")]
    public int? AllowLay { get; set; }
}