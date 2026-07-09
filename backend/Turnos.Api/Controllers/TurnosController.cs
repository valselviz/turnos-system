using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Data;
using Turnos.Api.Dtos;
using Turnos.Api.Models;
using Turnos.Api.Services;

namespace Turnos.Api.Controllers;

[ApiController]
[Route("turnos")]
public class TurnosController : ControllerBase
{
    private readonly TurnosDbContext _db;
    private readonly IHorarioService _horarios;

    public TurnosController(TurnosDbContext db, IHorarioService horarios)
    {
        _db = db;
        _horarios = horarios;
    }

    // GET /turnos?estado=Pendiente&fecha=2026-08-01
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TurnoResponseDto>>> Listar(
        [FromQuery] string? estado,
        [FromQuery] DateOnly? fecha)
    {
        var query = _db.Turnos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            if (!Enum.TryParse<EstadoTurno>(estado, ignoreCase: true, out var estadoParsed))
                return BadRequest(new ErrorResponseDto(
                    $"Estado inválido '{estado}'. Valores permitidos: Pendiente, Confirmado, Cancelado."));

            query = query.Where(t => t.Estado == estadoParsed);
        }

        if (fecha is not null)
        {
            var desde = fecha.Value.ToDateTime(TimeOnly.MinValue);
            var hasta = desde.AddDays(1);
            query = query.Where(t => t.FechaHora >= desde && t.FechaHora < hasta);
        }

        var turnos = await query
            .OrderBy(t => t.FechaHora)
            .Select(t => TurnoResponseDto.FromEntity(t))
            .ToListAsync();

        return Ok(turnos);
    }

    // POST /turnos
    [HttpPost]
    public async Task<ActionResult<TurnoResponseDto>> Crear([FromBody] CrearTurnoDto dto)
    {
        // Regla: la fecha y hora del turno debe ser en el futuro al momento de crearlo.
        if (dto.FechaHora <= DateTime.Now)
        {
            return BadRequest(new ErrorResponseDto(
                "La fecha y hora del turno debe ser en el futuro."));
        }

        // Regla: el turno tiene que caer en un slot habilitado (día hábil,
        // dentro del horario laboral, alineado a la grilla de 15 minutos).
        if (!_horarios.EsSlotValido(dto.FechaHora))
        {
            return BadRequest(new ErrorResponseDto(
                "El horario elegido no corresponde a un turno habilitado."));
        }

        // Regla: si ese slot ya tiene un turno Confirmado del MISMO trámite, ni
        // siquiera dejamos crear un Pendiente nuevo ahí (evita que alguien agende
        // algo que ya sabemos que va a fallar al intentar confirmarlo). Distintos
        // trámites no compiten por el mismo horario — los atiende otra ventanilla.
        var slotYaConfirmado = await _db.Turnos.AnyAsync(t =>
            t.FechaHora == dto.FechaHora &&
            t.TipoTramite == dto.TipoTramite &&
            t.Estado == EstadoTurno.Confirmado);

        if (slotYaConfirmado)
        {
            return Conflict(new ErrorResponseDto(
                "Ese horario ya tiene un turno confirmado. Elegí otro."));
        }

        var turno = new Turno
        {
            NombreCiudadano = dto.NombreCiudadano,
            Dni = dto.Dni,
            FechaHora = dto.FechaHora,
            TipoTramite = dto.TipoTramite,
            Estado = EstadoTurno.Pendiente,
            CreatedAt = DateTime.UtcNow
        };

        _db.Turnos.Add(turno);
        await _db.SaveChangesAsync();

        var response = TurnoResponseDto.FromEntity(turno);
        return CreatedAtAction(nameof(Listar), new { id = turno.Id }, response);
    }

    // PUT /turnos/{id}/confirmar
    [HttpPut("{id:int}/confirmar")]
    public async Task<ActionResult<TurnoResponseDto>> Confirmar(int id)
    {
        var turno = await _db.Turnos.FindAsync(id);
        if (turno is null)
            return NotFound(new ErrorResponseDto($"No existe un turno con id {id}."));

        if (turno.Estado != EstadoTurno.Pendiente)
        {
            return BadRequest(new ErrorResponseDto(
                $"Sólo se pueden confirmar turnos en estado 'Pendiente'. Estado actual: '{turno.Estado}'."));
        }

        // Regla: no puede existir más de un turno confirmado en el mismo horario
        // para el mismo trámite (distintos trámites usan ventanillas distintas).
        var existeConflicto = await _db.Turnos.AnyAsync(t =>
            t.Id != id &&
            t.FechaHora == turno.FechaHora &&
            t.TipoTramite == turno.TipoTramite &&
            t.Estado == EstadoTurno.Confirmado);

        if (existeConflicto)
        {
            return Conflict(new ErrorResponseDto(
                "Ya existe otro turno confirmado en ese mismo horario."));
        }

        turno.Estado = EstadoTurno.Confirmado;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Red de seguridad ante condiciones de carrera: si dos requests confirman
            // el mismo horario al mismo tiempo, el índice único parcial de la base de
            // datos rechaza el segundo INSERT/UPDATE aunque la validación de arriba
            // no lo haya detectado a tiempo.
            return Conflict(new ErrorResponseDto(
                "Ya existe otro turno confirmado en ese mismo horario."));
        }

        return Ok(TurnoResponseDto.FromEntity(turno));
    }

    // PUT /turnos/{id}/cancelar
    [HttpPut("{id:int}/cancelar")]
    public async Task<ActionResult<TurnoResponseDto>> Cancelar(int id)
    {
        var turno = await _db.Turnos.FindAsync(id);
        if (turno is null)
            return NotFound(new ErrorResponseDto($"No existe un turno con id {id}."));

        // Regla: un turno sólo puede cancelarse si su estado actual es pendiente o confirmado.
        if (turno.Estado == EstadoTurno.Cancelado)
        {
            return BadRequest(new ErrorResponseDto(
                "El turno ya está cancelado."));
        }

        turno.Estado = EstadoTurno.Cancelado;
        await _db.SaveChangesAsync();

        return Ok(TurnoResponseDto.FromEntity(turno));
    }
}
