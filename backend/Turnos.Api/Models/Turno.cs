using System.ComponentModel.DataAnnotations;

namespace Turnos.Api.Models;

public class Turno
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string NombreCiudadano { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Dni { get; set; } = string.Empty;

    [Required]
    public DateTime FechaHora { get; set; }

    [Required, MaxLength(100)]
    public string TipoTramite { get; set; } = string.Empty;

    public EstadoTurno Estado { get; set; } = EstadoTurno.Pendiente;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
