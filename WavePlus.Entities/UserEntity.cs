using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[PrimaryKey("Id", "AuthTicket")]
[Index("ActivityPoints", Name = "activity_points")]
[Index("AuthTicket", Name = "auth_ticket")]
[Index("Credits", Name = "credits")]
[Index("HomeRoom", Name = "home_room")]
[Index("Id", Name = "id", IsUnique = true)]
[Index("IpLast", Name = "ip_last")]
[Index("IpReg", Name = "ip_reg")]
[Index("LastOnline", Name = "last_online")]
[Index("MachineId", Name = "machine_id")]
[Index("Mail", Name = "mail")]
[Index("Id", "Username", "Look", "Motto", "LastOnline", Name = "messenger")]
[Index("Online", Name = "online")]
[Index("Rank", Name = "rank")]
[Index("RankVip", Name = "rank_vip")]
[Index("Username", Name = "username", IsUnique = true)]
[Table("users")]
public partial class UserEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(125)]
    [Column("username")]
    public string Username { get; set; }

    [StringLength(255)]
    [Column("password")]
    public string Password { get; set; }

    [Column("mail")]
    public string Mail { get; set; }

    [Key]
    [StringLength(60)]
    [Column("auth_ticket")]
    public string AuthTicket { get; set; }

    [Column("rank", TypeName = "int(1) unsigned")]
    public uint? Rank { get; set; }

    [Column("rank_vip", TypeName = "int(1)")]
    public int? RankVip { get; set; }

    [Column("vip_expire", TypeName = "int(11)")]
    public int? VipExpire { get; set; }

    [Column("credits", TypeName = "int(11)")]
    public int? Credits { get; set; }

    [Column("vip_points", TypeName = "int(11)")]
    public int? VipPoints { get; set; }

    [Column("activity_points", TypeName = "int(11)")]
    public int? ActivityPoints { get; set; }

    [Column("look")]
    public string Look { get; set; }

    [Column("gender", TypeName = "enum('M','F')")]
    public string Gender { get; set; }

    [StringLength(50)]
    [Column("motto")]
    public string Motto { get; set; }

    [StringLength(12)]
    [Column("account_created")]
    public string AccountCreated { get; set; }

    [Column("last_online", TypeName = "int(11)")]
    public int? LastOnline { get; set; }

    [Column("online", TypeName = "enum('0','1')")]
    public string Online { get; set; }

    [StringLength(45)]
    [Column("ip_last")]
    public string IpLast { get; set; }

    [StringLength(45)]
    [Column("ip_reg")]
    public string IpReg { get; set; }

    [Column("home_room", TypeName = "int(10)")]
    public int? HomeRoom { get; set; }

    [Column("is_muted", TypeName = "enum('0','1')")]
    public string IsMuted { get; set; }

    [Column("block_newfriends", TypeName = "enum('0','1')")]
    public string BlockNewfriends { get; set; }

    [Column("hide_online", TypeName = "enum('0','1')")]
    public string HideOnline { get; set; }

    [Column("hide_inroom", TypeName = "enum('0','1')")]
    public string HideInroom { get; set; }

    [Column("vip", TypeName = "enum('0','1')")]
    public string Vip { get; set; }

    [StringLength(15)]
    [Column("volume")]
    public string Volume { get; set; }

    [Column("last_change", TypeName = "int(20)")]
    public int? LastChange { get; set; }

    [StringLength(125)]
    [Column("machine_id")]
    public string MachineId { get; set; }

    [Column("focus_preference", TypeName = "enum('0','1')")]
    public string FocusPreference { get; set; }

    [Column("chat_preference", TypeName = "enum('0','1')")]
    public string ChatPreference { get; set; }

    [Column("pets_muted", TypeName = "enum('0','1')")]
    public string PetsMuted { get; set; }

    [Column("bots_muted", TypeName = "enum('0','1')")]
    public string BotsMuted { get; set; }

    [Column("advertising_report_blocked", TypeName = "enum('0','1')")]
    public string AdvertisingReportBlocked { get; set; }

    [Column("gotw_points", TypeName = "int(11)")]
    public int? GotwPoints { get; set; }

    [Column("ignore_invites", TypeName = "enum('0','1')")]
    public string IgnoreInvites { get; set; }

    [Column("time_muted")]
    public double? TimeMuted { get; set; }

    [Column("allow_gifts", TypeName = "enum('0','1')")]
    public string AllowGifts { get; set; }

    [Column("trading_locked")]
    public double? TradingLocked { get; set; }

    [Required]
    [Column("disable_forced_effects", TypeName = "enum('0','1')")]
    public string DisableForcedEffects { get; set; }

    [Required]
    [Column("allow_mimic", TypeName = "enum('1','0')")]
    public string AllowMimic { get; set; }

    [Required]
    [Column("home_room_data")]
    public string HomeRoomData { get; set; }

    [Required]
    [Column("friend_bar_state")]
    public bool? FriendBarState { get; set; }

    [Column("bubble", TypeName = "smallint(6)")]
    public short Bubble { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<UserChatBubbleEntity> UserChatBubbles { get; set; } = [];
}