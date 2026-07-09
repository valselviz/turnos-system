import type { Turno, CrearTurnoInput, FiltrosTurnos, HorarioDisponible } from './types'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5080'

interface ErrorResponse {
  error?: string
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const res = await fetch(`${API_URL}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  })

  // La API devuelve { error: "..." } en las respuestas de error (ver
  // ErrorResponseDto en el backend), así que lo desempaquetamos acá una
  // sola vez para que los componentes solo necesiten un try/catch simple.
  if (!res.ok) {
    let message = `Error ${res.status}`
    try {
      const body: ErrorResponse = await res.json()
      if (body?.error) message = body.error
    } catch {
      // sin body JSON, usamos el mensaje generico
    }
    throw new Error(message)
  }

  if (res.status === 204) return null as T
  return res.json() as Promise<T>
}

export function listarTurnos(filtros: FiltrosTurnos = {}): Promise<Turno[]> {
  const params = new URLSearchParams()
  if (filtros.estado) params.set('estado', filtros.estado)
  if (filtros.fecha) params.set('fecha', filtros.fecha)
  const query = params.toString() ? `?${params.toString()}` : ''
  return request<Turno[]>(`/turnos${query}`)
}

export function crearTurno(turno: CrearTurnoInput): Promise<Turno> {
  return request<Turno>('/turnos', {
    method: 'POST',
    body: JSON.stringify(turno),
  })
}

export function listarHorariosDisponibles(fecha: string): Promise<HorarioDisponible[]> {
  const params = new URLSearchParams({ fecha })
  return request<HorarioDisponible[]>(`/horarios-disponibles?${params.toString()}`)
}

export function confirmarTurno(id: number): Promise<Turno> {
  return request<Turno>(`/turnos/${id}/confirmar`, { method: 'PUT' })
}

export function cancelarTurno(id: number): Promise<Turno> {
  return request<Turno>(`/turnos/${id}/cancelar`, { method: 'PUT' })
}
