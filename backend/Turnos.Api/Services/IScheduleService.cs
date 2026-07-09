namespace Turnos.Api.Services;

public interface IScheduleService
{
    bool IsSlotValid(DateTime dateTime);

    IReadOnlyList<TimeOnly> GetDaySlots(DateOnly date);
}
