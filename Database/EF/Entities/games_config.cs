using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("games_config")]
[Index("id", Name = "id", IsUnique = true)]
public partial class games_config
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [StringLength(25)]
    public string name { get; set; }

    [Required]
    [StringLength(25)]
    public string colour_one { get; set; }

    [Required]
    [StringLength(25)]
    public string colour_two { get; set; }

    [Required]
    [StringLength(125)]
    public string resource_path { get; set; }

    [Required]
    [StringLength(25)]
    public string string_three { get; set; }

    [Required]
    [StringLength(255)]
    public string game_swf { get; set; }

    [Required]
    [StringLength(255)]
    public string game_assets { get; set; }

    [Required]
    [StringLength(25)]
    public string game_server_host { get; set; }

    [Required]
    [StringLength(25)]
    public string game_server_port { get; set; }

    [Required]
    [StringLength(25)]
    public string socket_policy_port { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string game_enabled { get; set; }

    public double? last_reset { get; set; }
}
