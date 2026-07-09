using Microsoft.Extensions.Options;
using Turnos.Api.Options;

namespace Turnos.Api.Services;

/// <summary>
/// Computes the grid of enabled slots from BusinessHoursOptions.
/// Deliberately knows nothing about appointments or the database: it only
/// answers "what slots exist in theory?" and "is this date/time one of
/// them?". Who's already booked is the responsibility of whoever uses it
/// (AppointmentsController), querying the database separately. This makes
/// it easy to test in isolation.
/// </summary>
public class ScheduleService : IScheduleService
{
    private readonly BusinessHoursOptions _options;

    public ScheduleService(IOptions<BusinessHoursOptions> options)
    {
        _options = options.Value;
    }

    public bool IsSlotValid(DateTime dateTime)
    {
        // A valid slot has no stray seconds or milliseconds: it always starts
        // at an exact point on the grid (e.g. 09:00, 09:15, 09:30...).
        if (dateTime.Second != 0 || dateTime.Millisecond != 0)
            return false;

        if (!_options.BusinessDays.Contains(dateTime.DayOfWeek))
            return false;

        var requestedTime = TimeOnly.FromDateTime(dateTime);
        return IsSlotStartTime(requestedTime);
    }

    public IReadOnlyList<TimeOnly> GetDaySlots(DateOnly date)
    {
        if (!_options.BusinessDays.Contains(date.DayOfWeek))
            return Array.Empty<TimeOnly>();

        var slots = new List<TimeOnly>();
        var current = _options.StartTime;

        while (current.AddMinutes(_options.SlotDurationMinutes) <= _options.EndTime)
        {
            slots.Add(current);
            current = current.AddMinutes(_options.SlotDurationMinutes);
        }

        return slots;
    }

    private bool IsSlotStartTime(TimeOnly time)
    {
        var minutesFromStart = (int)(time.ToTimeSpan() - _options.StartTime.ToTimeSpan()).TotalMinutes;

        if (minutesFromStart < 0 || minutesFromStart % _options.SlotDurationMinutes != 0)
            return false;

        var slotEndTime = time.AddMinutes(_options.SlotDurationMinutes);
        return slotEndTime <= _options.EndTime;
    }
}
