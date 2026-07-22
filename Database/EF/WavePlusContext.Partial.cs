using Microsoft.EntityFrameworkCore;
using Plus.Database.EF.Entities;

namespace Plus.Database.EF;

public partial class WavePlusContext
{
    public virtual DbSet<UserXmas15CalendarEntity> user_xmas15_calendars { get; set; }

    public virtual DbSet<RoomItemStockEntity> RoomItemStocks { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoomPromotionEntity>(e => e.Property(p => p.RoomId).ValueGeneratedNever());
        modelBuilder.Entity<UsersMacroSettingEntity>(e => e.Property(p => p.UserId).ValueGeneratedNever());
        modelBuilder.Entity<WiredItemEntity>(e => e.Property(p => p.Id).ValueGeneratedNever());
        modelBuilder.Entity<UserXmas15CalendarEntity>(e => e.HasNoKey().ToTable("user_xmas15_calendar"));
    }
}