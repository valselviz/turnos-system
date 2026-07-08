namespace Turnos.Api.Models;

/// <summary>
/// Estados posibles de un turno. Se persiste como texto en la base de datos
/// (ver TurnosDbContext) para que sea legible directamente en la tabla.
/// </summary>
public enum EstadoTurno
{
    Pendiente,
    Confirmado,
    Cancelado
}
