using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("Id", Name = "id", IsUnique = true)]
[Table("bots_petdata")]
public partial class BotsPetdatumEntity
{
    [Key]
    [Column("id", TypeName = "int(11) unsigned")]
    public uint Id { get; set; }

    [Column("type", TypeName = "int(11) unsigned")]
    public uint? Type { get; set; }

    [StringLength(11)]
    [Column("race")]
    public string Race { get; set; }

    [StringLength(11)]
    [Column("color")]
    public string Color { get; set; }

    [Column("energy", TypeName = "int(11)")]
    public int? Energy { get; set; }

    [Column("experience", TypeName = "int(11)")]
    public int? Experience { get; set; }

    [Column("nutrition", TypeName = "int(11)")]
    public int? Nutrition { get; set; }

    [Column("respect", TypeName = "int(11)")]
    public int? Respect { get; set; }

    [Column("createstamp", TypeName = "int(11)")]
    public int? Createstamp { get; set; }

    [Column("have_saddle", TypeName = "int(11)")]
    public int? HaveSaddle { get; set; }

    [Column("hairdye", TypeName = "int(11)")]
    public int? Hairdye { get; set; }

    [Column("pethair", TypeName = "int(11)")]
    public int? Pethair { get; set; }

    [Column("anyone_ride", TypeName = "int(11)")]
    public int? AnyoneRide { get; set; }

    [StringLength(85)]
    [Column("gnome_clothing")]
    public string GnomeClothing { get; set; }
}