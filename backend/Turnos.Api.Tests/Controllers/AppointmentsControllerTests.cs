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

public class AppointmentsControllerTests
{
    private static AppointmentsController CreateController()
    {
        var dbOptions = new DbContextOptionsBuilder<AppointmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new AppointmentsDbContext(dbOptions);

        var businessHoursOptions = Microsoft.Extensions.Options.Options.Create(new BusinessHoursOptions
        {
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(16, 0),
            SlotDurationMinutes = 15,
            BusinessDays = new[]
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday,
            },
        });

        return new AppointmentsController(db, new ScheduleService(businessHoursOptions));
    }

    // Next Thursday at the requested time — always in the future and always a business day.
    private static DateTime NextThursdayAt(int hour, int minute)
    {
        var today = DateTime.Today;
        var daysUntilThursday = ((int)DayOfWeek.Thursday - (int)today.DayOfWeek + 7) % 7;
        daysUntilThursday = daysUntilThursday == 0 ? 7 : daysUntilThursday;
        var thursday = today.AddDays(daysUntilThursday);
        return new DateTime(thursday.Year, thursday.Month, thursday.Day, hour, minute, 0);
    }

    private static CreateAppointmentDto ValidDto(DateTime scheduledAt, string serviceType = "Pasaporte") => new()
    {
        CitizenName = "Valeria Selviz",
        NationalId = "12345678",
        ScheduledAt = scheduledAt,
        ServiceType = serviceType,
    };

    private static AppointmentResponseDto ExtractCreated(ActionResult<AppointmentResponseDto> result)
        => (AppointmentResponseDto)((CreatedAtActionResult)result.Result!).Value!;

    [Fact]
    public async Task Create_FutureAppointmentOnValidSlot_ReturnsCreated()
    {
        var controller = CreateController();
        var scheduledAt = NextThursdayAt(9, 0);

        var result = await controller.Create(ValidDto(scheduledAt));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var appointment = Assert.IsType<AppointmentResponseDto>(created.Value);
        Assert.Equal("Pending", appointment.Status);
    }

    [Fact]
    public async Task Create_DateInThePast_ReturnsBadRequest()
    {
        var controller = CreateController();
        var scheduledAt = DateTime.Now.AddDays(-1);

        var result = await controller.Create(ValidDto(scheduledAt));

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_OutsideAnEnabledSlot_ReturnsBadRequest()
    {
        var controller = CreateController();
        var scheduledAt = NextThursdayAt(9, 5); // not aligned to the 15-min grid

        var result = await controller.Create(ValidDto(scheduledAt));

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_SlotAlreadyConfirmed_ReturnsConflict()
    {
        var controller = CreateController();
        var scheduledAt = NextThursdayAt(10, 0);

        var created = ExtractCreated(await controller.Create(ValidDto(scheduledAt)));
        await controller.Confirm(created.Id);

        var result = await controller.Create(ValidDto(scheduledAt));

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_SameTimeSlotDifferentServiceType_DoesNotConflict()
    {
        // Pasaporte and Cédula are different desks: a confirmed Pasaporte
        // appointment at 10am shouldn't prevent booking a Cédula at that same time.
        var controller = CreateController();
        var scheduledAt = NextThursdayAt(10, 15);

        var passport = ExtractCreated(await controller.Create(ValidDto(scheduledAt, "Pasaporte")));
        await controller.Confirm(passport.Id);

        var result = await controller.Create(ValidDto(scheduledAt, "Cédula de identidad"));

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Confirm_PendingAppointment_BecomesConfirmed()
    {
        var controller = CreateController();
        var scheduledAt = NextThursdayAt(11, 0);

        var created = ExtractCreated(await controller.Create(ValidDto(scheduledAt)));

        var result = await controller.Confirm(created.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var appointment = Assert.IsType<AppointmentResponseDto>(ok.Value);
        Assert.Equal("Confirmed", appointment.Status);
    }

    [Fact]
    public async Task Confirm_AppointmentNotPending_ReturnsBadRequest()
    {
        var controller = CreateController();
        var scheduledAt = NextThursdayAt(12, 0);

        var created = ExtractCreated(await controller.Create(ValidDto(scheduledAt)));
        await controller.Confirm(created.Id); // already Confirmed

        var result = await controller.Confirm(created.Id); // trying again

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Confirm_AnotherAlreadyConfirmedAtSameTime_ReturnsConflict()
    {
        // NOTE: this test creates two Pending appointments in the same slot
        // (allowed today) and depends on the partial unique index on
        // ScheduledAt being correctly enforced under EF Core's InMemory
        // provider. If this specific test blows up with an exception (not a
        // simple assert failure), it's almost certainly because the InMemory
        // provider isn't respecting the index's partial filter — flag it and
        // we'll fix the test infrastructure, not the business logic.
        var controller = CreateController();
        var scheduledAt = NextThursdayAt(9, 30);

        var appointmentA = ExtractCreated(await controller.Create(ValidDto(scheduledAt)));
        var appointmentB = ExtractCreated(await controller.Create(ValidDto(scheduledAt)));

        await controller.Confirm(appointmentA.Id);

        var result = await controller.Confirm(appointmentB.Id);

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task Cancel_PendingAppointment_BecomesCancelled()
    {
        var controller = CreateController();
        var scheduledAt = NextThursdayAt(13, 0);

        var created = ExtractCreated(await controller.Create(ValidDto(scheduledAt)));

        var result = await controller.Cancel(created.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var appointment = Assert.IsType<AppointmentResponseDto>(ok.Value);
        Assert.Equal("Cancelled", appointment.Status);
    }

    [Fact]
    public async Task Cancel_AlreadyCancelledAppointment_ReturnsBadRequest()
    {
        var controller = CreateController();
        var scheduledAt = NextThursdayAt(14, 0);

        var created = ExtractCreated(await controller.Create(ValidDto(scheduledAt)));
        await controller.Cancel(created.Id);

        var result = await controller.Cancel(created.Id);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
