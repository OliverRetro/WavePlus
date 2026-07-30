using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Keyless]
[Table("server_status")]
[Index("UsersOnline", Name = "users_online", IsUnique = true)]
public partial class ServerStatusEntity
{
    [Column("users_online", TypeName = "int(11)")]
    public int UsersOnline { get; set; }

    [Column("loaded_rooms", TypeName = "int(11)")]
    public int LoadedRooms { get; set; }
}