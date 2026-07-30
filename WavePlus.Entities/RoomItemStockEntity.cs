using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("room_item_stock")]
[Index("RoomId", "FurniId", "StockType", "ItemId", Name = "room_item_stock_unique", IsUnique = true)]
public partial class RoomItemStockEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("room_id", TypeName = "int(11)")]
    public int RoomId { get; set; }

    [Column("furni_id", TypeName = "int(11)")]
    public int FurniId { get; set; }

    [Required]
    [StringLength(20)]
    [Column("stock_type")]
    public string StockType { get; set; }

    [Column("item_id", TypeName = "int(11)")]
    public int ItemId { get; set; }

    [Column("amount", TypeName = "int(11)")]
    public int Amount { get; set; }

    // Credits charged per unit. 0 means the entry is free.
    [Column("price", TypeName = "int(11)")]
    public int Price { get; set; }
}