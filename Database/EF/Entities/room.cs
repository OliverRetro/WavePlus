using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("caption", Name = "caption")]
[Index("category", Name = "category")]
[Index("group_id", Name = "group_id")]
[Index("id", Name = "id", IsUnique = true)]
[Index("owner", Name = "owner")]
[Index("roomtype", Name = "roomtype")]
[Index("score", Name = "score")]
[Index("users_now", Name = "users_now")]
public partial class room
{
    [Key]
    [Column(TypeName = "int(10)")]
    public int id { get; set; }

    [Required]
    [Column(TypeName = "enum('public','private')")]
    public string roomtype { get; set; }

    [Required]
    [StringLength(100)]
    public string caption { get; set; }

    [Required]
    [StringLength(75)]
    public string owner { get; set; }

    [Required]
    [StringLength(255)]
    public string description { get; set; }

    [Column(TypeName = "int(11)")]
    public int category { get; set; }

    [Required]
    [Column(TypeName = "enum('open','locked','password','invisible')")]
    public string state { get; set; }

    [Column(TypeName = "int(11)")]
    public int users_now { get; set; }

    [Column(TypeName = "int(11)")]
    public int users_max { get; set; }

    [Required]
    [StringLength(50)]
    public string model_name { get; set; }

    [Column(TypeName = "int(11)")]
    public int score { get; set; }

    [Required]
    [StringLength(100)]
    public string tags { get; set; }

    [Required]
    [StringLength(30)]
    public string password { get; set; }

    [Required]
    [StringLength(10)]
    public string wallpaper { get; set; }

    [Required]
    [StringLength(10)]
    public string floor { get; set; }

    [Required]
    [StringLength(10)]
    public string landscape { get; set; }

    public bool allow_pets { get; set; }

    public bool allow_pets_eat { get; set; }

    public bool room_blocking_disabled { get; set; }

    [Required]
    public bool? allow_hidewall { get; set; }

    [Column(TypeName = "int(1)")]
    public int wallthick { get; set; }

    [Column(TypeName = "int(1)")]
    public int floorthick { get; set; }

    [Column(TypeName = "int(11) unsigned")]
    public uint group_id { get; set; }

    [Required]
    public bool? mute_settings { get; set; }

    [Required]
    public bool? ban_settings { get; set; }

    [Column(TypeName = "tinyint(2)")]
    public sbyte kick_settings { get; set; }

    [Column(TypeName = "int(11)")]
    public int chat_mode { get; set; }

    [Column(TypeName = "int(11)")]
    public int chat_size { get; set; }

    [Column(TypeName = "int(11)")]
    public int chat_speed { get; set; }

    [Column(TypeName = "int(11)")]
    public int chat_extra_flood { get; set; }

    [Column(TypeName = "int(11)")]
    public int chat_hearing_distance { get; set; }

    [Column(TypeName = "int(11)")]
    public int trade_settings { get; set; }

    [Required]
    public bool? push_enabled { get; set; }

    [Required]
    public bool? pull_enabled { get; set; }

    [Required]
    public bool? enables_enabled { get; set; }

    [Required]
    public bool? respect_notifications_enabled { get; set; }

    [Required]
    public bool? pet_morphs_allowed { get; set; }

    [Required]
    public bool? spull_enabled { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string spush_enabled { get; set; }

    [Column(TypeName = "int(11)")]
    public int sale_price { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string lay_enabled { get; set; }

    [Required]
    [StringLength(4)]
    public string safezone { get; set; }

    [Column(TypeName = "int(11)")]
    public int taxi_x { get; set; }

    [Column(TypeName = "int(11)")]
    public int taxi_y { get; set; }

    public double taxi_z { get; set; }

    [Column(TypeName = "int(11)")]
    public int taxi_rotation { get; set; }

    [Column(TypeName = "int(11)")]
    public int taxi_cost { get; set; }

    [Column(TypeName = "int(11)")]
    public int taxi_delay { get; set; }
}
