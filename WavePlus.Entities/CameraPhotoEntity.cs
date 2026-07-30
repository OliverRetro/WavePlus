using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("UserId", Name = "user_id")]
[Table("camera_photos")]
public partial class CameraPhotoEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Column("room_id", TypeName = "int(11)")]
    public int RoomId { get; set; }

    [Column("timestamp", TypeName = "int(11)")]
    public int Timestamp { get; set; }

    [Required]
    [Column("url")]
    [StringLength(255)]
    public string Url { get; set; }
}