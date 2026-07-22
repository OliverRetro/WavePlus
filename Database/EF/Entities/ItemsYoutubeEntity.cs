using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("items_youtube")]
[Index("Id", Name = "id", IsUnique = true)]
public partial class ItemsYoutubeEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(35)]
    [Column("youtube_id")]
    public string YoutubeId { get; set; }

    [Required]
    [StringLength(50)]
    [Column("title")]
    public string Title { get; set; }

    [Required]
    [StringLength(150)]
    [Column("description")]
    public string Description { get; set; }

    [Required]
    [Column("enabled", TypeName = "enum('0','1')")]
    public string Enabled { get; set; }
}