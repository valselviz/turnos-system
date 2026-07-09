using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Turnos.Api.Controllers;
using Turnos.Api.Data;
using Turnos.Api.Dtos;
using Turnos.Api.Options;
using Turnos.Api.Services;
using Xunit;

namespace Turnos.Api.Tests.Controllers;

public class TurnosControllerTests
{
    private static TurnosController CrearController()
    {
        var dbOptions = new DbContextOptionsBuilder<TurnosDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new TurnosDbContext(dbOptions);

        var horarioOptions = Microsoft.Extensions.Options.Options.Create(new HorarioLaboralOptions
        {
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(16, 0),
            DuracionSlotMinutos = 15,
            DiasHabiles = new[]
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday,
            },
        });

        return new TurnosController(db, new HorarioService(horarioOptions));
    }

    // Próximo jueves a la hora pedida — siempre en el futuro y siempre día hábil.
    private static DateTime ProximoJuevesA(int hora, int minuto)
    {
        var hoy = DateTime.Today;
        var diasHastaJueves = ((int)DayOfWeek.Thursday - (int)hoy.DayOfWeek + 7) % 7;
        diasHastaJueves = diasHastaJueves == 0 ? 7 : diasHastaJueves;
        var jueves = hoy.AddDays(diasHastaJueves);
        return new DateTime(jueves.Year, jueves.Month, jueves.Day, hora, minuto, 0);
    }

    private static CrearTurnoDto DtoValido(DateTime fechaHora, string tipoTramite = "Pasaporte") => new()
    {
        NombreCiudadano = "Valeria Selviz",
        Dni = "12345678",
        FechaHora = fechaHora,
        TipoTramite = tipoTramite,
    };

    private static TurnoResponseDto ExtraerCreado(ActionResult<TurnoResponseDto> resultado)
        => (TurnoResponseDto)((CreatedAtActionResult)resultado.Result!).Value!;

    [Fact]
    public async Task Crear_TurnoFuturoEnSlotValido_DevuelveCreated()
    {
        var controller = CrearController();
        var fechaHora = ProximoJuevesA(9, 0);

        var resultado = await controller.Crear(DtoValido(fechaHora));

        var creado = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        var turno = Assert.IsType<TurnoResponseDto>(creado.Value);
        Assert.Equal("Pendiente", turno.Estado);
    }

    [Fact]
    public async Task Crear_FechaEnElPasado_DevuelveBadRequest()
    {
        var controller = CrearController();
        var fechaHora = DateTime.Now.AddDays(-1);

        var resultado = await controller.Crear(DtoValido(fechaHora));

        Assert.IsType<BadRequestObjectResult>(resultado.Result);
    }

    [Fact]
    public async Task Crear_FueraDeUnSlotHabilitado_DevuelveBadRequest()
    {
        var controller = CrearController();
        var fechaHora = ProximoJuevesA(9, 5); // no alineado a la grilla de 15 min

        var resultado = await controller.Crear(DtoValido(fechaHora));

        Assert.IsType<BadRequestObjectResult>(resultado.Result);
    }

    [Fact]
    public async Task Crear_SlotYaConfirmado_DevuelveConflict()
    {
        var controller = CrearController();
        var fechaHora = ProximoJuevesA(10, 0);

        var creado = ExtraerCreado(await controller.Crear(DtoValido(fechaHora)));
        await controller.Confirmar(creado.Id);

        var resultado = await controller.Crear(DtoValido(fechaHora));

        Assert.IsType<ConflictObjectResult>(resultado.Result);
    }

    [Fact]
    public async Task Crear_MismoHorarioDistintoTramite_NoGeneraConflicto()
    {
        // Pasaporte y Cédula son ventanillas distintas: un Pasaporte confirmado
        // a las 10am no debería impedir agendar una Cédula a esa misma hora.
        var controller = CrearController();
        var fechaHora = ProximoJuevesA(10, 15);

        var pasaporte = ExtraerCreado(await controller.Crear(DtoValido(fechaHora, "Pasaporte")));
        await controller.Confirmar(pasaporte.Id);

        var resultado = await controller.Crear(DtoValido(fechaHora, "Cédula de identidad"));

        Assert.IsType<CreatedAtActionResult>(resultado.Result);
    }

    [Fact]
    public async Task Confirmar_TurnoPendiente_QuedaConfirmado()
    {
        var controller = CrearController();
        var fechaHora = ProximoJuevesA(11, 0);

        var creado = ExtraerCreado(await controller.Crear(DtoValido(fechaHora)));

        var resultado = await controller.Confirmar(creado.Id);

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var turno = Assert.IsType<TurnoResponseDto>(ok.Value);
        Assert.Equal("Confirmado", turno.Estado);
    }

    [Fact]
    public async Task Confirmar_TurnoQueNoEstaPendiente_DevuelveBadRequest()
    {
        var controller = CrearController();
        var fechaHora = ProximoJuevesA(12, 0);

        var creado = ExtraerCreado(await controller.Crear(DtoValido(fechaHora)));
        await controller.Confirmar(creado.Id); // ya queda Confirmado

        var resultado = await controller.Confirmar(creado.Id); // intento de nuevo

        Assert.IsType<BadRequestObjectResult>(resultado.Result);
    }

    [Fact]
    public async Task Confirmar_YaHayOtroConfirmadoALaMismaHora_DevuelveConflict()
    {
        // OJO: este test crea dos turnos Pendiente en el mismo horario (algo
        // permitido hoy) y depende de que el índice único parcial de FechaHora
        // se respete correctamente bajo el proveedor InMemory de EF Core. Si
        // este test específico revienta con una excepción (no un simple fallo
        // de assert), es casi seguro que el proveedor InMemory no está
        // respetando el filtro parcial del índice — avisame y lo resolvemos
        // cambiando la infraestructura del test, no la lógica de negocio.
        var controller = CrearController();
        var fechaHora = ProximoJuevesA(9, 30);

        var turnoA = ExtraerCreado(await controller.Crear(DtoValido(fechaHora)));
        var turnoB = ExtraerCreado(await controller.Crear(DtoValido(fechaHora)));

        await controller.Confirmar(turnoA.Id);

        var resultado = await controller.Confirmar(turnoB.Id);

        Assert.IsType<ConflictObjectResult>(resultado.Result);
    }

    [Fact]
    public async Task Cancelar_TurnoPendiente_QuedaCancelado()
    {
        var controller = CrearController();
        var fechaHora = ProximoJuevesA(13, 0);

        var creado = ExtraerCreado(await controller.Crear(DtoValido(fechaHora)));

        var resultado = await controller.Cancelar(creado.Id);

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var turno = Assert.IsType<TurnoResponseDto>(ok.Value);
        Assert.Equal("Cancelado", turno.Estado);
    }

    [Fact]
    public async Task Cancelar_TurnoYaCancelado_DevuelveBadRequest()
    {
        var controller = CrearController();
        var fechaHora = ProximoJuevesA(14, 0);

        var creado = ExtraerCreado(await controller.Crear(DtoValido(fechaHora)));
        await controller.Cancelar(creado.Id);

        var resultado = await controller.Cancelar(creado.Id);

        Assert.IsType<BadRequestObjectResult>(resultado.Result);
    }
}
