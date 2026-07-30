using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("server_landing")]
[Index("Id", Name = "id", IsUnique = true)]
public partial class ServerLandingEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [StringLength(35)]
    [Column("title")]
    public string Title { get; set; }

    [Column("text", TypeName = "text")]
    public string Text { get; set; }

    [StringLength(25)]
    [Column("button_text")]
    public string ButtonText { get; set; }

    [Column("button_type", TypeName = "enum('0','1','2','3')")]
    public string ButtonType { get; set; }

    [StringLength(90)]
    [Column("button_link")]
    public string ButtonLink { get; set; }

    [StringLength(120)]
    [Column("image_link")]
    public string ImageLink { get; set; }
}