using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Data;
using Turnos.Api.Dtos;
using Turnos.Api.Models;
using Turnos.Api.Services;

namespace Turnos.Api.Controllers;

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
        if (dto.ScheduledAt <= DateTime.Now)
        {
            return BadRequest(new ErrorResponseDto(
                "The appointment's date and time must be in the future."));
        }

        if (!_schedule.IsSlotValid(dto.ScheduledAt))
        {
            return BadRequest(new ErrorResponseDto(
                "The chosen time doesn't correspond to an available slot."));
        }

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
