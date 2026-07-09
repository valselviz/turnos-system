using Microsoft.Extensions.Options;
using Turnos.Api.Options;
using Turnos.Api.Services;
using Xunit;

namespace Turnos.Api.Tests.Services;

public class ScheduleServiceTests
{
    private static IScheduleService CreateService()
    {
        var options = new BusinessHoursOptions
        {
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(16, 0),
            SlotDurationMinutes = 15,
            BusinessDays = new[]
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday,
            },
        };

        return new ScheduleService(Microsoft.Extensions.Options.Options.Create(options));
    }

    [Fact]
    public void IsSlotValid_AlignedTimeOnBusinessDay_IsValid()
    {
        var service = CreateService();

        // 2026-07-09 is a Thursday.
        var dateTime = new DateTime(2026, 7, 9, 9, 15, 0);

        Assert.True(service.IsSlotValid(dateTime));
    }

    [Fact]
    public void IsSlotValid_Weekend_IsNotValid()
    {
        var service = CreateService();

        // 2026-07-11 is a Saturday.
        var dateTime = new DateTime(2026, 7, 11, 9, 0, 0);

        Assert.False(service.IsSlotValid(dateTime));
    }

    [Fact]
    public void IsSlotValid_NotAlignedToGrid_IsNotValid()
    {
        var service = CreateService();

        // 9:05 isn't a multiple of 15 minutes from 9:00.
        var dateTime = new DateTime(2026, 7, 9, 9, 5, 0);

        Assert.False(service.IsSlotValid(dateTime));
    }

    [Fact]
    public void IsSlotValid_BeforeBusinessHours_IsNotValid()
    {
        var service = CreateService();

        var dateTime = new DateTime(2026, 7, 9, 8, 45, 0);

        Assert.False(service.IsSlotValid(dateTime));
    }

    [Fact]
    public void IsSlotValid_LastValidSlotStartsFifteenMinutesBeforeEndTime()
    {
        var service = CreateService();

        // 15:45 + 15 min = 16:00 exactly: fits right within business hours.
        var dateTime = new DateTime(2026, 7, 9, 15, 45, 0);

        Assert.True(service.IsSlotValid(dateTime));
    }

    [Fact]
    public void IsSlotValid_SlotStartingExactlyAtEndTime_IsNotValid()
    {
        var service = CreateService();

        // The 16:00 slot would end at 16:15, after EndTime.
        var dateTime = new DateTime(2026, 7, 9, 16, 0, 0);

        Assert.False(service.IsSlotValid(dateTime));
    }

    [Fact]
    public void GetDaySlots_BusinessDay_ReturnsFullGrid()
    {
        var service = CreateService();

        // 2026-07-09 is a Thursday. From 09:00 to 16:00 in 15-min steps: 28 slots.
        var slots = service.GetDaySlots(new DateOnly(2026, 7, 9));

        Assert.Equal(28, slots.Count);
        Assert.Equal(new TimeOnly(9, 0), slots[0]);
        Assert.Equal(new TimeOnly(15, 45), slots[^1]);
    }

    [Fact]
    public void GetDaySlots_Weekend_ReturnsEmpty()
    {
        var service = CreateService();

        // 2026-07-11 is a Saturday.
        var slots = service.GetDaySlots(new DateOnly(2026, 7, 11));

        Assert.Empty(slots);
    }
}
