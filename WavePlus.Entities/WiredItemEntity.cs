using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("Id", Name = "id", IsUnique = true)]
[Table("wired_items")]
public partial class WiredItemEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(5000)]
    [Column("items")]
    public string Items { get; set; }

    [Column("delay", TypeName = "int(11)")]
    public int Delay { get; set; }

    [Required]
    [Column("string")]
    [StringLength(5000)]
    public string _string { get; set; }

    [Required]
    [Column("bool", TypeName = "enum('0','1')")]
    public string _bool { get; set; }
}