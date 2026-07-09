namespace Turnos.Api.Options;

/// <summary>
/// Business hours configuration: within what time range, on what days, and in
/// blocks of what size appointments can be booked. Loaded from the
/// "BusinessHours" section of appsettings.json (see Program.cs).
///
/// This is deliberately a fixed configuration and not a database table: the
/// exercise doesn't ask for different hours per day or an admin panel, so
/// handling it as configuration avoids one more migration and entity for
/// something that today is a constant value. If it were ever needed to vary
/// hours per day (holidays, exceptions), this class is the only place that
/// would need to be replaced by a database query — the rest of the system
/// doesn't know where this information comes from.
/// </summary>
public class BusinessHoursOptions
{
    public const string SectionName = "BusinessHours";

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
    public DayOfWeek[] BusinessDays { get; set; } = Array.Empty<DayOfWeek>();
}
