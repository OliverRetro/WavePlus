using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("Id", Name = "id", IsUnique = true)]
[Table("ranks")]
public partial class RankEntity
{
    [Key]
    [Column("id", TypeName = "int(11) unsigned")]
    public uint Id { get; set; }

    [Required]
    [StringLength(50)]
    [Column("name")]
    public string Name { get; set; }

    [Required]
    [StringLength(5)]
    [Column("badgeid")]
    public string Badgeid { get; set; }

    [Required]
    [StringLength(50)]
    [Column("title")]
    public string Title { get; set; }

    [Required]
    [Column("tab_colour", TypeName = "enum('red','green','pixeldarkblue','orange','blue','settings','pixellightblue')")]
    public string TabColour { get; set; }
}