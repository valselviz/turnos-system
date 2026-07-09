using Microsoft.Extensions.Options;
using Turnos.Api.Options;
using Turnos.Api.Services;
using Xunit;

namespace Turnos.Api.Tests.Services;

public class HorarioServiceTests
{
    private static IHorarioService CrearServicio()
    {
        var options = new HorarioLaboralOptions
        {
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(16, 0),
            DuracionSlotMinutos = 15,
            DiasHabiles = new[]
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday,
            },
        };

        return new HorarioService(Microsoft.Extensions.Options.Options.Create(options));
    }

    [Fact]
    public void EsSlotValido_HorarioAlineadoEnDiaHabil_EsValido()
    {
        var servicio = CrearServicio();

        // 2026-07-09 es jueves.
        var fechaHora = new DateTime(2026, 7, 9, 9, 15, 0);

        Assert.True(servicio.EsSlotValido(fechaHora));
    }

    [Fact]
    public void EsSlotValido_FinDeSemana_NoEsValido()
    {
        var servicio = CrearServicio();

        // 2026-07-11 es sábado.
        var fechaHora = new DateTime(2026, 7, 11, 9, 0, 0);

        Assert.False(servicio.EsSlotValido(fechaHora));
    }

    [Fact]
    public void EsSlotValido_NoAlineadoALaGrilla_NoEsValido()
    {
        var servicio = CrearServicio();

        // 9:05 no es múltiplo de 15 minutos desde las 9:00.
        var fechaHora = new DateTime(2026, 7, 9, 9, 5, 0);

        Assert.False(servicio.EsSlotValido(fechaHora));
    }

    [Fact]
    public void EsSlotValido_AntesDelHorarioLaboral_NoEsValido()
    {
        var servicio = CrearServicio();

        var fechaHora = new DateTime(2026, 7, 9, 8, 45, 0);

        Assert.False(servicio.EsSlotValido(fechaHora));
    }

    [Fact]
    public void EsSlotValido_UltimoSlotValidoEmpiezaQuinceMinutosAntesDeHoraFin()
    {
        var servicio = CrearServicio();

        // 15:45 + 15 min = 16:00 exacto: entra justo dentro del horario laboral.
        var fechaHora = new DateTime(2026, 7, 9, 15, 45, 0);

        Assert.True(servicio.EsSlotValido(fechaHora));
    }

    [Fact]
    public void EsSlotValido_SlotQueEmpiezaJustoEnHoraFin_NoEsValido()
    {
        var servicio = CrearServicio();

        // El slot de 16:00 terminaría a las 16:15, después de HoraFin.
        var fechaHora = new DateTime(2026, 7, 9, 16, 0, 0);

        Assert.False(servicio.EsSlotValido(fechaHora));
    }

    [Fact]
    public void ObtenerSlotsDelDia_DiaHabil_DevuelveLaGrillaCompleta()
    {
        var servicio = CrearServicio();

        // 2026-07-09 es jueves. De 09:00 a 16:00 en pasos de 15 min: 28 slots.
        var slots = servicio.ObtenerSlotsDelDia(new DateOnly(2026, 7, 9));

        Assert.Equal(28, slots.Count);
        Assert.Equal(new TimeOnly(9, 0), slots[0]);
        Assert.Equal(new TimeOnly(15, 45), slots[^1]);
    }

    [Fact]
    public void ObtenerSlotsDelDia_FinDeSemana_DevuelveVacio()
    {
        var servicio = CrearServicio();

        // 2026-07-11 es sábado.
        var slots = servicio.ObtenerSlotsDelDia(new DateOnly(2026, 7, 11));

        Assert.Empty(slots);
    }
}
