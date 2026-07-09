using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Data;
using Turnos.Api.Dtos;
using Turnos.Api.Models;
using Turnos.Api.Services;

namespace Turnos.Api.Controllers;

[ApiController]
[Route("available-slots")]
public class AvailableSlotsController : ControllerBase
{
    private readonly AppointmentsDbContext _db;
    private readonly IScheduleService _schedule;

    public AvailableSlotsController(AppointmentsDbContext db, IScheduleService schedule)
    {
        _db = db;
        _schedule = schedule;
    }

    // GET /available-slots?date=2026-08-01&serviceType=Pasaporte
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AvailableSlotDto>>> List(
        [FromQuery] DateOnly date,
        [FromQuery] string serviceType)
    {
        var slots = _schedule.GetDaySlots(date);

        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);

        var confirmedTimes = await _db.Appointments
            .Where(a => a.ScheduledAt >= dayStart
                     && a.ScheduledAt < dayEnd
                     && a.ServiceType == serviceType
                     && a.Status == AppointmentStatus.Confirmed)
            .Select(a => a.ScheduledAt)
            .ToListAsync();

        var confirmedTimesSet = confirmedTimes
            .Select(TimeOnly.FromDateTime)
            .ToHashSet();

        var result = slots.Select(slot => new AvailableSlotDto
        {
            StartTime = slot,
            Available = !confirmedTimesSet.Contains(slot)
        });

        return Ok(result);
    }
}
