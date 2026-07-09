// Estos tipos reflejan exactamente los DTOs del backend
// (Turnos.Api/Dtos/TurnoDtos.cs). Si un campo cambia en el backend,
// este archivo es el primer lugar que hay que actualizar.

export type EstadoTurno = 'Pendiente' | 'Confirmado' | 'Cancelado'

/** Forma en la que la API devuelve un turno (TurnoResponseDto). */
export interface Turno {
  id: number
  nombreCiudadano: string
  dni: string
  fechaHora: string // ISO 8601, tal como lo serializa System.Text.Json
  tipoTramite: string
  estado: EstadoTurno
  createdAt: string
}

/** Body que espera POST /turnos (CrearTurnoDto). */
export interface CrearTurnoInput {
  nombreCiudadano: string
  dni: string
  fechaHora: string // ISO 8601
  tipoTramite: string
}

/** Filtros opcionales que acepta GET /turnos. */
export interface FiltrosTurnos {
  estado?: string
  fecha?: string // YYYY-MM-DD
}

/**
 * Un slot de la grilla horaria de un día (GET /horarios-disponibles).
 * horaInicio llega como "HH:mm:ss" — así serializa System.Text.Json el
 * TimeOnly del backend (ver HorarioDisponibleDto).
 */
export interface HorarioDisponible {
  horaInicio: string
  disponible: boolean
}
