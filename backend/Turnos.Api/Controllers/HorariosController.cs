using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Data;
using Turnos.Api.Dtos;
using Turnos.Api.Models;
using Turnos.Api.Services;

namespace Turnos.Api.Controllers;

/// <summary>
/// Controller separado de TurnosController a propósito: esto no expone ni
/// modifica turnos, solo responde "¿qué horarios existen y cuáles están
/// libres tal día?". Es una consulta de disponibilidad, no un recurso turno.
/// </summary>
[ApiController]
[Route("horarios-disponibles")]
public class HorariosController : ControllerBase
{
    private readonly TurnosDbContext _db;
    private readonly IHorarioService _horarios;

    public HorariosController(TurnosDbContext db, IHorarioService horarios)
    {
        _db = db;
        _horarios = horarios;
    }

    // GET /horarios-disponibles?fecha=2026-08-01
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HorarioDisponibleDto>>> Listar([FromQuery] DateOnly fecha)
    {
        var slots = _horarios.ObtenerSlotsDelDia(fecha);

        var inicioDia = fecha.ToDateTime(TimeOnly.MinValue);
        var finDia = inicioDia.AddDays(1);

        // Solo nos importan los turnos Confirmados: un Pendiente no bloquea
        // que se muestre el slot como disponible (misma regla que ya usamos
        // al crear y al confirmar turnos).
        var horasConfirmadas = await _db.Turnos
            .Where(t => t.FechaHora >= inicioDia && t.FechaHora < finDia && t.Estado == EstadoTurno.Confirmado)
            .Select(t => t.FechaHora)
            .ToListAsync();

        var horasConfirmadasSet = horasConfirmadas
            .Select(TimeOnly.FromDateTime)
            .ToHashSet();

        var resultado = slots.Select(slot => new HorarioDisponibleDto
        {
            HoraInicio = slot,
            Disponible = !horasConfirmadasSet.Contains(slot)
        });

        return Ok(resultado);
    }
}
