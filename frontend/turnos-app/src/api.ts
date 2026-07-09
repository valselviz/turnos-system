import type { Appointment, CreateAppointmentInput, AppointmentFilters, AvailableSlot } from './types'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5080'

interface ErrorResponse {
  error?: string
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const res = await fetch(`${API_URL}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  })

  // The API returns { error: "..." } on error responses (see ErrorResponseDto
  // in the backend), so we unwrap it here once so components only need a
  // simple try/catch.
  if (!res.ok) {
    let message = `Error ${res.status}`
    try {
      const body: ErrorResponse = await res.json()
      if (body?.error) message = body.error
    } catch {
      // no JSON body, use the generic message
    }
    throw new Error(message)
  }

  if (res.status === 204) return null as T
  return res.json() as Promise<T>
}

export function listAppointments(filters: AppointmentFilters = {}): Promise<Appointment[]> {
  const params = new URLSearchParams()
  if (filters.status) params.set('status', filters.status)
  if (filters.date) params.set('date', filters.date)
  if (filters.serviceType) params.set('serviceType', filters.serviceType)
  if (filters.search) params.set('search', filters.search)
  const query = params.toString() ? `?${params.toString()}` : ''
  return request<Appointment[]>(`/turnos${query}`)
}

export function createAppointment(appointment: CreateAppointmentInput): Promise<Appointment> {
  return request<Appointment>('/turnos', {
    method: 'POST',
    body: JSON.stringify(appointment),
  })
}

export function listAvailableSlots(date: string, serviceType: string): Promise<AvailableSlot[]> {
  const params = new URLSearchParams({ date, serviceType })
  return request<AvailableSlot[]>(`/available-slots?${params.toString()}`)
}

export function confirmAppointment(id: number): Promise<Appointment> {
  return request<Appointment>(`/turnos/${id}/confirmar`, { method: 'PUT' })
}

export function cancelAppointment(id: number): Promise<Appointment> {
  return request<Appointment>(`/turnos/${id}/cancelar`, { method: 'PUT' })
}
