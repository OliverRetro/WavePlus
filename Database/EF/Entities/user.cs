using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[PrimaryKey("id", "auth_ticket")]
[Index("activity_points", Name = "activity_points")]
[Index("auth_ticket", Name = "auth_ticket")]
[Index("credits", Name = "credits")]
[Index("home_room", Name = "home_room")]
[Index("id", Name = "id", IsUnique = true)]
[Index("ip_last", Name = "ip_last")]
[Index("ip_reg", Name = "ip_reg")]
[Index("last_online", Name = "last_online")]
[Index("machine_id", Name = "machine_id")]
[Index("mail", Name = "mail")]
[Index("id", "username", "look", "motto", "last_online", Name = "messenger")]
[Index("online", Name = "online")]
[Index("rank", Name = "rank")]
[Index("rank_vip", Name = "rank_vip")]
[Index("username", Name = "username", IsUnique = true)]
public partial class user
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [StringLength(125)]
    public string username { get; set; }

    [StringLength(255)]
    public string password { get; set; }

    public string mail { get; set; }

    [Key]
    [StringLength(60)]
    public string auth_ticket { get; set; }

    [Column(TypeName = "int(1) unsigned")]
    public uint? rank { get; set; }

    [Column(TypeName = "int(1)")]
    public int? rank_vip { get; set; }

    [Column(TypeName = "int(11)")]
    public int? credits { get; set; }

    [Column(TypeName = "int(11)")]
    public int? vip_points { get; set; }

    [Column(TypeName = "int(11)")]
    public int? activity_points { get; set; }

    public string look { get; set; }

    [Column(TypeName = "enum('M','F')")]
    public string gender { get; set; }

    [StringLength(50)]
    public string motto { get; set; }

    [StringLength(12)]
    public string account_created { get; set; }

    [Column(TypeName = "int(11)")]
    public int? last_online { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string online { get; set; }

    [StringLength(45)]
    public string ip_last { get; set; }

    [StringLength(45)]
    public string ip_reg { get; set; }

    [Column(TypeName = "int(10)")]
    public int? home_room { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string is_muted { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string block_newfriends { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string hide_online { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string hide_inroom { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string vip { get; set; }

    [StringLength(15)]
    public string volume { get; set; }

    [Column(TypeName = "int(20)")]
    public int? last_change { get; set; }

    [StringLength(125)]
    public string machine_id { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string focus_preference { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string chat_preference { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string pets_muted { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string bots_muted { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string advertising_report_blocked { get; set; }

    [Column(TypeName = "int(11)")]
    public int? gotw_points { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string ignore_invites { get; set; }

    public double? time_muted { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string allow_gifts { get; set; }

    public double? trading_locked { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string disable_forced_effects { get; set; }

    [Required]
    [Column(TypeName = "enum('1','0')")]
    public string allow_mimic { get; set; }

    [Required]
    public string home_room_data { get; set; }

    [Required]
    public bool? friend_bar_state { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short bubble { get; set; }

    [InverseProperty("user")]
    public virtual ICollection<user_chat_bubble> user_chat_bubbles { get; set; } = [];
}
