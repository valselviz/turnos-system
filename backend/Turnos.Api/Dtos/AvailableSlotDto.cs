namespace Turnos.Api.Dtos;

/// <summary>A slot in the day's grid, with whether it's available to book.</summary>
public class AvailableSlotDto
{
    public TimeOnly StartTime { get; set; }
    public bool Available { get; set; }
}
