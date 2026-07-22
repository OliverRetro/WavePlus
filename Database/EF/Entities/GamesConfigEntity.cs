using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("games_config")]
[Index("Id", Name = "id", IsUnique = true)]
public partial class GamesConfigEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(25)]
    [Column("name")]
    public string Name { get; set; }

    [Required]
    [StringLength(25)]
    [Column("colour_one")]
    public string ColourOne { get; set; }

    [Required]
    [StringLength(25)]
    [Column("colour_two")]
    public string ColourTwo { get; set; }

    [Required]
    [StringLength(125)]
    [Column("resource_path")]
    public string ResourcePath { get; set; }

    [Required]
    [StringLength(25)]
    [Column("string_three")]
    public string StringThree { get; set; }

    [Required]
    [StringLength(255)]
    [Column("game_swf")]
    public string GameSwf { get; set; }

    [Required]
    [StringLength(255)]
    [Column("game_assets")]
    public string GameAssets { get; set; }

    [Required]
    [StringLength(25)]
    [Column("game_server_host")]
    public string GameServerHost { get; set; }

    [Required]
    [StringLength(25)]
    [Column("game_server_port")]
    public string GameServerPort { get; set; }

    [Required]
    [StringLength(25)]
    [Column("socket_policy_port")]
    public string SocketPolicyPort { get; set; }

    [Column("game_enabled", TypeName = "enum('0','1')")]
    public string GameEnabled { get; set; }

    [Column("last_reset")]
    public double? LastReset { get; set; }
}