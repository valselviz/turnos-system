namespace Turnos.Api.Models;

/// <summary>
/// Possible states of an appointment. Persisted as text in the database
/// (see AppointmentsDbContext) so it's directly readable in the table.
/// </summary>
public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Cancelled
}
