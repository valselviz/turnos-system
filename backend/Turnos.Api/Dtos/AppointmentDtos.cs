using System.ComponentModel.DataAnnotations;
using Turnos.Api.Models;

namespace Turnos.Api.Dtos;

/// <summary>Data received when creating an appointment (POST /turnos).</summary>
public class CreateAppointmentDto
{
    [Required, MaxLength(200)]
    public string CitizenName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string NationalId { get; set; } = string.Empty;

    [Required]
    public DateTime ScheduledAt { get; set; }

    [Required, MaxLength(100)]
    public string ServiceType { get; set; } = string.Empty;
}

/// <summary>Shape in which we return an appointment to the frontend.</summary>
public class AppointmentResponseDto
{
    public int Id { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static AppointmentResponseDto FromEntity(Appointment a) => new()
    {
        Id = a.Id,
        CitizenName = a.CitizenName,
        NationalId = a.NationalId,
        ScheduledAt = a.ScheduledAt,
        ServiceType = a.ServiceType,
        Status = a.Status.ToString(),
        CreatedAt = a.CreatedAt
    };
}

/// <summary>Uniform error response for the whole API.</summary>
public class ErrorResponseDto
{
    public string Error { get; set; } = string.Empty;

    public ErrorResponseDto(string error) => Error = error;
}
