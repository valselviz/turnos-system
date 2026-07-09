// These types mirror the backend DTOs exactly
// (Turnos.Api/Dtos/AppointmentDtos.cs). If a field changes in the backend,
// this file is the first place to update.

export type AppointmentStatus = 'Pending' | 'Confirmed' | 'Cancelled'

/** Shape in which the API returns an appointment (AppointmentResponseDto). */
export interface Appointment {
  id: number
  citizenName: string
  nationalId: string
  scheduledAt: string // ISO 8601, as serialized by System.Text.Json
  serviceType: string
  status: AppointmentStatus
  createdAt: string
}

/** Body expected by POST /turnos (CreateAppointmentDto). */
export interface CreateAppointmentInput {
  citizenName: string
  nationalId: string
  scheduledAt: string // ISO 8601
  serviceType: string
}

/** Optional filters accepted by GET /turnos. */
export interface AppointmentFilters {
  status?: string
  date?: string // YYYY-MM-DD
  serviceType?: string
  search?: string // name or national ID, partial match
}

/**
 * A slot in a day's time grid (GET /available-slots).
 * startTime arrives as "HH:mm:ss" — that's how System.Text.Json serializes
 * the backend's TimeOnly (see AvailableSlotDto).
 */
export interface AvailableSlot {
  startTime: string
  available: boolean
}
