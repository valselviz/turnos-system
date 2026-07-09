namespace Turnos.Api.Dtos;

/// <summary>Un slot de la grilla del día, con si está o no disponible para agendar.</summary>
public class HorarioDisponibleDto
{
    public TimeOnly HoraInicio { get; set; }
    public bool Disponible { get; set; }
}
