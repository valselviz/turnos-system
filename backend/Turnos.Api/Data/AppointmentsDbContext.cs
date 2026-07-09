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
            // Store the enum as text ("Pending", "Confirmed", "Cancelled") so the
            // table is directly readable in psql / any SQL client.
            entity.Property(a => a.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            // ScheduledAt is a wall-clock time (the citizen has the appointment
            // "at 9am in Montevideo"), not a universal instant — there are no
            // multiple time zones in play here. We map it explicitly to
            // "timestamp without time zone" instead of Npgsql's default
            // ("timestamp with time zone", which requires DateTime.Kind=Utc on
            // every value). This avoids having to convert to/from UTC anywhere
            // in the system; the value sent by the frontend is stored and
            // compared exactly as-is.
            entity.Property(a => a.ScheduledAt)
                  .HasColumnType("timestamp without time zone");

            // Business rule "no more than one confirmed appointment in the same
            // slot" enforced at the database level with a partial unique index:
            // it only applies to rows whose Status is 'Confirmed'. This avoids
            // race conditions that controller-level validation alone can't cover.
            //
            // The index is composite (ScheduledAt + ServiceType), not just
            // ScheduledAt: different service types are handled by a different
            // desk within the office, so a confirmed Passport appointment at
            // 9am shouldn't block an ID Card appointment at that same time.
            entity.HasIndex(a => new { a.ScheduledAt, a.ServiceType })
                  .IsUnique()
                  .HasFilter("\"Status\" = 'Confirmed'")
                  .HasDatabaseName("IX_Appointments_ScheduledAt_ServiceType_ConfirmedOnly");
        });
    }
}
