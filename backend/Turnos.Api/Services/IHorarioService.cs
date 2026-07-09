namespace Turnos.Api.Services;

public interface IHorarioService
{
    /// <summary>
    /// True si fechaHora cae exactamente en el inicio de un slot habilitado
    /// (día hábil, dentro del horario laboral, alineado a la grilla de slots).
    /// </summary>
    bool EsSlotValido(DateTime fechaHora);

    /// <summary>
    /// Lista de horarios de inicio de cada slot habilitado para el día dado.
    /// Vacía si ese día no es hábil.
    /// </summary>
    IReadOnlyList<TimeOnly> ObtenerSlotsDelDia(DateOnly fecha);
}
