using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("FromId", Name = "from_id")]
[Index("ToId", Name = "to_id")]
[Table("messenger_requests")]
public partial class MessengerRequestEntity
{
    [Key]
    [Column("id", TypeName = "int(11) unsigned")]
    public uint Id { get; set; }

    [Column("from_id", TypeName = "int(10) unsigned")]
    public uint FromId { get; set; }

    [Column("to_id", TypeName = "int(10) unsigned")]
    public uint ToId { get; set; }
}