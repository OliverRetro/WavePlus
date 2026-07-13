using Microsoft.EntityFrameworkCore;
using Plus.Database.EF.Entities;

namespace Plus.Database.EF;

public partial class WavePlusContext
{
    public virtual DbSet<UserXmas15Calendar> user_xmas15_calendars { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoomPromotion>(e => e.Property(p => p.RoomId).ValueGeneratedNever());
        modelBuilder.Entity<UsersMacroSetting>(e => e.Property(p => p.UserId).ValueGeneratedNever());
        modelBuilder.Entity<WiredItem>(e => e.Property(p => p.Id).ValueGeneratedNever());
        modelBuilder.Entity<UserXmas15Calendar>(e => e.HasNoKey().ToTable("user_xmas15_calendar"));
    }
}
