using Microsoft.EntityFrameworkCore;
using Turnos.Api.Models;

namespace Turnos.Api.Data;

public class AppointmentsDbContext : DbContext
{
    public AppointmentsDbContext(DbContextOptions<AppointmentsDbContext> options) : base(options) { }

    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.Property(a => a.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(a => a.ScheduledAt)
                  .HasColumnType("timestamp without time zone");

            entity.HasIndex(a => new { a.ScheduledAt, a.ServiceType })
                  .IsUnique()
                  .HasFilter("\"Status\" = 'Confirmed'")
                  .HasDatabaseName("IX_Appointments_ScheduledAt_ServiceType_ConfirmedOnly");
        });
    }
}
