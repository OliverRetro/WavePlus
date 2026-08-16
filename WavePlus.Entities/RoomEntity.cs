using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("Caption", Name = "caption")]
[Index("Category", Name = "category")]
[Index("GroupId", Name = "group_id")]
[Index("Id", Name = "id", IsUnique = true)]
[Index("Owner", Name = "owner")]
[Index("Roomtype", Name = "roomtype")]
[Index("Score", Name = "score")]
[Index("UsersNow", Name = "users_now")]
[Table("rooms")]
public partial class RoomEntity
{
    [Key]
    [Column("id", TypeName = "int(10)")]
    public int Id { get; set; }

    [Required]
    [Column("roomtype", TypeName = "enum('public','private')")]
    public string Roomtype { get; set; }

    [Required]
    [StringLength(100)]
    [Column("caption")]
    public string Caption { get; set; }

    [Required]
    [StringLength(75)]
    [Column("owner")]
    public string Owner { get; set; }

    [Required]
    [StringLength(255)]
    [Column("description")]
    public string Description { get; set; }

    [Column("category", TypeName = "int(11)")]
    public int Category { get; set; }

    [Required]
    [Column("state", TypeName = "enum('open','locked','password','invisible')")]
    public string State { get; set; }

    [Column("users_now", TypeName = "int(11)")]
    public int UsersNow { get; set; }

    [Column("users_max", TypeName = "int(11)")]
    public int UsersMax { get; set; }

    [Required]
    [StringLength(50)]
    [Column("model_name")]
    public string ModelName { get; set; }

    [Column("score", TypeName = "int(11)")]
    public int Score { get; set; }

    [Required]
    [StringLength(100)]
    [Column("tags")]
    public string Tags { get; set; }

    [Required]
    [StringLength(30)]
    [Column("password")]
    public string Password { get; set; }

    [Required]
    [StringLength(10)]
    [Column("wallpaper")]
    public string Wallpaper { get; set; }

    [Required]
    [StringLength(10)]
    [Column("floor")]
    public string Floor { get; set; }

    [Required]
    [StringLength(10)]
    [Column("landscape")]
    public string Landscape { get; set; }

    [Column("allow_pets")]
    public bool AllowPets { get; set; }

    [Column("allow_pets_eat")]
    public bool AllowPetsEat { get; set; }

    [Column("room_blocking_disabled")]
    public bool RoomBlockingDisabled { get; set; }

    [Column("pathfinding_3d")]
    public bool Pathfinding3d { get; set; }

    [Required]
    [Column("allow_hidewall")]
    public bool? AllowHidewall { get; set; }

    [Column("wallthick", TypeName = "int(1)")]
    public int Wallthick { get; set; }

    [Column("floorthick", TypeName = "int(1)")]
    public int Floorthick { get; set; }

    [Column("group_id", TypeName = "int(11) unsigned")]
    public uint GroupId { get; set; }

    [Required]
    [Column("mute_settings")]
    public bool? MuteSettings { get; set; }

    [Required]
    [Column("ban_settings")]
    public bool? BanSettings { get; set; }

    [Column("kick_settings", TypeName = "tinyint(2)")]
    public sbyte KickSettings { get; set; }

    [Column("chat_mode", TypeName = "int(11)")]
    public int ChatMode { get; set; }

    [Column("chat_size", TypeName = "int(11)")]
    public int ChatSize { get; set; }

    [Column("chat_speed", TypeName = "int(11)")]
    public int ChatSpeed { get; set; }

    [Column("chat_extra_flood", TypeName = "int(11)")]
    public int ChatExtraFlood { get; set; }

    [Column("chat_hearing_distance", TypeName = "int(11)")]
    public int ChatHearingDistance { get; set; }

    [Column("trade_settings", TypeName = "int(11)")]
    public int TradeSettings { get; set; }

    [Required]
    [Column("push_enabled")]
    public bool? PushEnabled { get; set; }

    [Required]
    [Column("pull_enabled")]
    public bool? PullEnabled { get; set; }

    [Required]
    [Column("enables_enabled")]
    public bool? EnablesEnabled { get; set; }

    [Required]
    [Column("respect_notifications_enabled")]
    public bool? RespectNotificationsEnabled { get; set; }

    [Required]
    [Column("pet_morphs_allowed")]
    public bool? PetMorphsAllowed { get; set; }

    [Required]
    [Column("spull_enabled")]
    public bool? SpullEnabled { get; set; }

    [Required]
    [Column("spush_enabled", TypeName = "enum('0','1')")]
    public string SpushEnabled { get; set; }

    [Column("sale_price", TypeName = "int(11)")]
    public int SalePrice { get; set; }

    [Required]
    [Column("lay_enabled", TypeName = "enum('0','1')")]
    public string LayEnabled { get; set; }

    [Required]
    [StringLength(4)]
    [Column("safezone")]
    public string Safezone { get; set; }
}