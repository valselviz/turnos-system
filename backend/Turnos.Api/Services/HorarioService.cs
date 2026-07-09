using Microsoft.Extensions.Options;
using Turnos.Api.Options;

namespace Turnos.Api.Services;

/// <summary>
/// Calcula la grilla de slots habilitados a partir de HorarioLaboralOptions.
/// Deliberadamente no sabe nada de turnos ni de la base de datos: solo
/// responde "¿qué slots existen en teoría?" y "¿esta fecha/hora es uno de
/// ellos?". Quién ya está ocupado es responsabilidad de quien la use
/// (TurnosController), consultando la base de datos por separado. Esto la
/// hace fácil de testear de forma aislada.
/// </summary>
public class HorarioService : IHorarioService
{
    private readonly HorarioLaboralOptions _options;

    public HorarioService(IOptions<HorarioLaboralOptions> options)
    {
        _options = options.Value;
    }

    public bool EsSlotValido(DateTime fechaHora)
    {
        // Un slot válido no tiene segundos ni milisegundos sueltos: siempre
        // empieza en un punto exacto de la grilla (ej. 09:00, 09:15, 09:30...).
        if (fechaHora.Second != 0 || fechaHora.Millisecond != 0)
            return false;

        if (!_options.DiasHabiles.Contains(fechaHora.DayOfWeek))
            return false;

        var horaSolicitada = TimeOnly.FromDateTime(fechaHora);
        return EsHoraDeInicioDeSlot(horaSolicitada);
    }

    public IReadOnlyList<TimeOnly> ObtenerSlotsDelDia(DateOnly fecha)
    {
        if (!_options.DiasHabiles.Contains(fecha.DayOfWeek))
            return Array.Empty<TimeOnly>();

        var slots = new List<TimeOnly>();
        var actual = _options.HoraInicio;

        while (actual.AddMinutes(_options.DuracionSlotMinutos) <= _options.HoraFin)
        {
            slots.Add(actual);
            actual = actual.AddMinutes(_options.DuracionSlotMinutos);
        }

        return slots;
    }

    private bool EsHoraDeInicioDeSlot(TimeOnly hora)
    {
        var minutosDesdeInicio = (int)(hora.ToTimeSpan() - _options.HoraInicio.ToTimeSpan()).TotalMinutes;

        if (minutosDesdeInicio < 0 || minutosDesdeInicio % _options.DuracionSlotMinutos != 0)
            return false;

        var horaFinSlot = hora.AddMinutes(_options.DuracionSlotMinutos);
        return horaFinSlot <= _options.HoraFin;
    }
}
