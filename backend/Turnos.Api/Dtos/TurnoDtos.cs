using System.ComponentModel.DataAnnotations;
using Turnos.Api.Models;

namespace Turnos.Api.Dtos;

/// <summary>Datos que llegan al crear un turno (POST /turnos).</summary>
public class CrearTurnoDto
{
    [Required, MaxLength(200)]
    public string NombreCiudadano { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Dni { get; set; } = string.Empty;

    [Required]
    public DateTime FechaHora { get; set; }

    [Required, MaxLength(100)]
    public string TipoTramite { get; set; } = string.Empty;
}

/// <summary>Forma en la que devolvemos un turno al frontend.</summary>
public class TurnoResponseDto
{
    public int Id { get; set; }
    public string NombreCiudadano { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string TipoTramite { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static TurnoResponseDto FromEntity(Turno t) => new()
    {
        Id = t.Id,
        NombreCiudadano = t.NombreCiudadano,
        Dni = t.Dni,
        FechaHora = t.FechaHora,
        TipoTramite = t.TipoTramite,
        Estado = t.Estado.ToString(),
        CreatedAt = t.CreatedAt
    };
}

/// <summary>Respuesta de error uniforme para toda la API.</summary>
public class ErrorResponseDto
{
    public string Error { get; set; } = string.Empty;

    public ErrorResponseDto(string error) => Error = error;
}
