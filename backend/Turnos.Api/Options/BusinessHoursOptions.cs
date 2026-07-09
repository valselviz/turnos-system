namespace Turnos.Api.Options;

public class BusinessHoursOptions
{
    public const string SectionName = "BusinessHours";

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
    public DayOfWeek[] BusinessDays { get; set; } = Array.Empty<DayOfWeek>();
}
