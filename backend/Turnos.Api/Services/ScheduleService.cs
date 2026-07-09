using Microsoft.Extensions.Options;
using Turnos.Api.Options;

namespace Turnos.Api.Services;

public class ScheduleService : IScheduleService
{
    private readonly BusinessHoursOptions _options;

    public ScheduleService(IOptions<BusinessHoursOptions> options)
    {
        _options = options.Value;
    }

    public bool IsSlotValid(DateTime dateTime)
    {
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
