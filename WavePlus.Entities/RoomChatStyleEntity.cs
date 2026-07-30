using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("room_chat_styles")]
public partial class RoomChatStyleEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [StringLength(25)]
    [Column("name")]
    public string Name { get; set; }

    [StringLength(25)]
    [Column("required_right")]
    public string RequiredRight { get; set; }

    [StringLength(255)]
    [Column("url")]
    public string Url { get; set; }

    [StringLength(25)]
    [Column("font_color")]
    public string FontColor { get; set; }
}