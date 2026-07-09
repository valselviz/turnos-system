using Microsoft.EntityFrameworkCore;
using Turnos.Api.Models;

namespace Turnos.Api.Data;

public class TurnosDbContext : DbContext
{
    public TurnosDbContext(DbContextOptions<TurnosDbContext> options) : base(options) { }

    public DbSet<Turno> Turnos => Set<Turno>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Turno>(entity =>
        {
            // Guardamos el enum como texto ("Pendiente", "Confirmado", "Cancelado")
            // para que la tabla sea legible directamente en psql / cualquier cliente SQL.
            entity.Property(t => t.Estado)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            // FechaHora es una hora de pared (el ciudadano tiene el turno "a las
            // 9am en Montevideo"), no un instante universal — acá no hay múltiples
            // zonas horarias en juego. La mapeamos explícitamente a "timestamp
            // without time zone" en vez del default de Npgsql ("timestamp with
            // time zone", que exige DateTime.Kind=Utc en cada valor). Así evitamos
            // tener que convertir a/desde UTC en todo el sistema; el valor que
            // manda el frontend, se guarda y se compara siempre "tal cual".
            entity.Property(t => t.FechaHora)
                  .HasColumnType("timestamp without time zone");

            // Regla de negocio "no puede existir más de un turno confirmado en el
            // mismo horario" reforzada a nivel de base de datos con un índice único
            // parcial: solo aplica a filas cuyo Estado sea 'Confirmado'. Esto evita
            // condiciones de carrera que la validación en el controller sola no cubre.
            entity.HasIndex(t => t.FechaHora)
                  .IsUnique()
                  .HasFilter("\"Estado\" = 'Confirmado'")
                  .HasDatabaseName("IX_Turnos_FechaHora_SoloConfirmados");
        });
    }
}
