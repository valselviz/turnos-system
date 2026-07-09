using System.ComponentModel.DataAnnotations;

namespace Turnos.Api.Models;

public class Appointment
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string CitizenName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string NationalId { get; set; } = string.Empty;

    [Required]
    public DateTime ScheduledAt { get; set; }

    [Required, MaxLength(100)]
    public string ServiceType { get; set; } = string.Empty;

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
