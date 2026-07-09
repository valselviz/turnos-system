namespace Turnos.Api.Services;

public interface IScheduleService
{
    /// <summary>
    /// True if dateTime falls exactly on the start of an enabled slot
    /// (business day, within business hours, aligned to the slot grid).
    /// </summary>
    bool IsSlotValid(DateTime dateTime);

    /// <summary>
    /// List of start times for each enabled slot on the given day.
    /// Empty if that day is not a business day.
    /// </summary>
    IReadOnlyList<TimeOnly> GetDaySlots(DateOnly date);
}
