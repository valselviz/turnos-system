namespace Turnos.Api.Options;

/// <summary>
/// Configuración del horario laboral: dentro de qué rango horario, en qué días
/// y en bloques de qué tamaño se pueden agendar turnos. Se carga desde la
/// sección "HorarioLaboral" de appsettings.json (ver Program.cs).
///
/// Es a propósito una configuración fija y no una tabla en la base de datos:
/// el enunciado no pide horarios distintos por día ni un panel de administración,
/// así que resolverlo como configuración evita una migración y una entidad de
/// más para algo que hoy es un valor constante. Si en el futuro hiciera falta
/// variar el horario por día (feriados, excepciones), esta clase es el único
/// lugar que habría que reemplazar por una consulta a base de datos — el resto
/// del sistema no sabe de dónde sale esta información.
/// </summary>
public class HorarioLaboralOptions
{
    public const string SectionName = "HorarioLaboral";

    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public int DuracionSlotMinutos { get; set; }
    public DayOfWeek[] DiasHabiles { get; set; } = Array.Empty<DayOfWeek>();
}
