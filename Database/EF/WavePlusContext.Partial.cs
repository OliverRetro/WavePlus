using Microsoft.EntityFrameworkCore;
using Plus.Database.EF.Entities;

namespace Plus.Database.EF;

public partial class WavePlusContext
{
    public virtual DbSet<user_xmas15_calendar> user_xmas15_calendars { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<room_promotion>(e => e.Property(p => p.room_id).ValueGeneratedNever());
        modelBuilder.Entity<users_macro_setting>(e => e.Property(p => p.user_id).ValueGeneratedNever());
        modelBuilder.Entity<wired_item>(e => e.Property(p => p.id).ValueGeneratedNever());
        modelBuilder.Entity<user_xmas15_calendar>(e => e.HasNoKey().ToTable("user_xmas15_calendar"));
    }
}
