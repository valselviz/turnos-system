using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Data;
using Turnos.Api.Dtos;
using Turnos.Api.Models;
using Turnos.Api.Services;

namespace Turnos.Api.Controllers;

// Route path kept as "turnos" (Spanish) on purpose: it's the exact endpoint
// contract given by the exercise ("POST /turnos", "PUT /turnos/{id}/confirmar",
// "PUT /turnos/{id}/cancelar"). Only the C# code (class/method/variable names,
// comments) is translated to English.
[ApiController]
[Route("turnos")]
public class AppointmentsController : ControllerBase
{
    private readonly AppointmentsDbContext _db;
    private readonly IScheduleService _schedule;

    public AppointmentsController(AppointmentsDbContext db, IScheduleService schedule)
    {
        _db = db;
        _schedule = schedule;
    }

    // GET /turnos?status=Pending&date=2026-08-01&serviceType=Pasaporte&search=valeria
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> List(
        [FromQuery] string? status,
        [FromQuery] DateOnly? date,
        [FromQuery] string? serviceType,
        [FromQuery] string? search)
    {
        var query = _db.Appointments.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<AppointmentStatus>(status, ignoreCase: true, out var statusParsed))
                return BadRequest(new ErrorResponseDto(
                    $"Invalid status '{status}'. Allowed values: Pending, Confirmed, Cancelled."));

            query = query.Where(a => a.Status == statusParsed);
        }

        if (date is not null)
        {
            var from = date.Value.ToDateTime(TimeOnly.MinValue);
            var to = from.AddDays(1);
            query = query.Where(a => a.ScheduledAt >= from && a.ScheduledAt < to);
        }

        if (!string.IsNullOrWhiteSpace(serviceType))
        {
            query = query.Where(a => a.ServiceType == serviceType);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            // ToLower() on both sides instead of EF.Functions.ILike: this way the
            // same code works the same against Postgres (production) and against
            // the InMemory provider used by the tests.
            var term = search.ToLower();
            query = query.Where(a =>
                a.CitizenName.ToLower().Contains(term) ||
                a.NationalId.ToLower().Contains(term));
        }

        var appointments = await query
            .OrderBy(a => a.ScheduledAt)
            .Select(a => AppointmentResponseDto.FromEntity(a))
            .ToListAsync();

        return Ok(appointments);
    }

    // POST /turnos
    [HttpPost]
    public async Task<ActionResult<AppointmentResponseDto>> Create([FromBody] CreateAppointmentDto dto)
    {
        // Rule: the appointment's date and time must be in the future when created.
        if (dto.ScheduledAt <= DateTime.Now)
        {
            return BadRequest(new ErrorResponseDto(
                "The appointment's date and time must be in the future."));
        }

        // Rule: the appointment must fall on an enabled slot (business day,
        // within business hours, aligned to the 15-minute grid).
        if (!_schedule.IsSlotValid(dto.ScheduledAt))
        {
            return BadRequest(new ErrorResponseDto(
                "The chosen time doesn't correspond to an available slot."));
        }

        // Rule: if that slot already has a Confirmed appointment for the SAME
        // service type, we don't even let a new Pending one be created there
        // (avoids someone booking something we already know will fail when
        // trying to confirm it). Different service types don't compete for the
        // same slot — they're handled by a different desk.
        var slotAlreadyConfirmed = await _db.Appointments.AnyAsync(a =>
            a.ScheduledAt == dto.ScheduledAt &&
            a.ServiceType == dto.ServiceType &&
            a.Status == AppointmentStatus.Confirmed);

        if (slotAlreadyConfirmed)
        {
            return Conflict(new ErrorResponseDto(
                "That time slot already has a confirmed appointment. Choose another."));
        }

        var appointment = new Appointment
        {
            CitizenName = dto.CitizenName,
            NationalId = dto.NationalId,
            ScheduledAt = dto.ScheduledAt,
            ServiceType = dto.ServiceType,
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        var response = AppointmentResponseDto.FromEntity(appointment);
        return CreatedAtAction(nameof(List), new { id = appointment.Id }, response);
    }

    // PUT /turnos/{id}/confirmar
    [HttpPut("{id:int}/confirmar")]
    public async Task<ActionResult<AppointmentResponseDto>> Confirm(int id)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment is null)
            return NotFound(new ErrorResponseDto($"No appointment exists with id {id}."));

        if (appointment.Status != AppointmentStatus.Pending)
        {
            return BadRequest(new ErrorResponseDto(
                $"Only appointments in 'Pending' status can be confirmed. Current status: '{appointment.Status}'."));
        }

        // Rule: no more than one confirmed appointment can exist in the same
        // slot for the same service type (different service types use
        // different desks).
        var hasConflict = await _db.Appointments.AnyAsync(a =>
            a.Id != id &&
            a.ScheduledAt == appointment.ScheduledAt &&
            a.ServiceType == appointment.ServiceType &&
            a.Status == AppointmentStatus.Confirmed);

        if (hasConflict)
        {
            return Conflict(new ErrorResponseDto(
                "Another appointment is already confirmed for that same time slot."));
        }

        appointment.Status = AppointmentStatus.Confirmed;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Safety net for race conditions: if two requests confirm the same
            // slot at the same time, the database's partial unique index
            // rejects the second INSERT/UPDATE even if the validation above
            // didn't catch it in time.
            return Conflict(new ErrorResponseDto(
                "Another appointment is already confirmed for that same time slot."));
        }

        return Ok(AppointmentResponseDto.FromEntity(appointment));
    }

    // PUT /turnos/{id}/cancelar
    [HttpPut("{id:int}/cancelar")]
    public async Task<ActionResult<AppointmentResponseDto>> Cancel(int id)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment is null)
            return NotFound(new ErrorResponseDto($"No appointment exists with id {id}."));

        // Rule: an appointment can only be cancelled if its current status is
        // pending or confirmed.
        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return BadRequest(new ErrorResponseDto(
                "The appointment is already cancelled."));
        }

        appointment.Status = AppointmentStatus.Cancelled;
        await _db.SaveChangesAsync();

        return Ok(AppointmentResponseDto.FromEntity(appointment));
    }
}
