import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { createAppointment, listAvailableSlots } from '../api'
import type { CreateAppointmentInput, AvailableSlot, Appointment } from '../types'

interface AppointmentFormProps {
  serviceType: string
  onCreated: (appointment: Appointment) => void
}

function todayIso(): string {
  const today = new Date()
  const yyyy = today.getFullYear()
  const mm = String(today.getMonth() + 1).padStart(2, '0')
  const dd = String(today.getDate()).padStart(2, '0')
  return `${yyyy}-${mm}-${dd}`
}

// The backend returns "09:00:00" (TimeOnly) for each slot; to display it in
// the <select> we only take HH:mm.
function formatSlotTime(startTime: string): string {
  return startTime.slice(0, 5)
}

// scheduledAt is a wall-clock time, not a universal instant (there's no
// multiple time zones in play here). That's why we build the string by hand
// instead of using Date.toISOString(), which would convert to UTC and shift
// the value by a few hours.
function buildNaiveDateTime(year: number, month: number, day: number, hour: number, minute: number): string {
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${year}-${pad(month)}-${pad(day)}T${pad(hour)}:${pad(minute)}:00`
}

export default function AppointmentForm({ serviceType, onCreated }: AppointmentFormProps) {
  const [citizenName, setCitizenName] = useState('')
  const [nationalId, setNationalId] = useState('')
  const [date, setDate] = useState('')
  const [selectedTime, setSelectedTime] = useState('')
  const [slots, setSlots] = useState<AvailableSlot[]>([])
  const [loadingSlots, setLoadingSlots] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  // Availability depends on the date and the service type (each service type
  // has its own desk) — we recompute it whenever the date changes.
  useEffect(() => {
    if (!date) {
      setSlots([])
      return
    }

    let cancelled = false
    setLoadingSlots(true)
    setSelectedTime('')

    listAvailableSlots(date, serviceType)
      .then((data) => {
        if (!cancelled) setSlots(data)
      })
      .catch((err) => {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Error desconocido')
      })
      .finally(() => {
        if (!cancelled) setLoadingSlots(false)
      })

    return () => {
      cancelled = true
    }
  }, [date, serviceType])

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)

    if (!date || !selectedTime) {
      setError('Elegí una fecha y un horario.')
      return
    }

    setLoading(true)
    try {
      const [hourStr, minuteStr] = selectedTime.split(':')
      const [year, month, day] = date.split('-').map(Number)

      const payload: CreateAppointmentInput = {
        citizenName,
        nationalId,
        serviceType,
        scheduledAt: buildNaiveDateTime(year, month, day, Number(hourStr), Number(minuteStr)),
      }

      const appointment = await createAppointment(payload)
      onCreated(appointment)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error desconocido')
    } finally {
      setLoading(false)
    }
  }

  return (
    <form className="appointment-form" onSubmit={handleSubmit}>
      <h2>Nuevo turno</h2>

      <p className="fixed-service">
        Trámite: <strong>{serviceType}</strong>
      </p>

      <label>
        Nombre del ciudadano
        <input
          value={citizenName}
          onChange={(e) => setCitizenName(e.target.value)}
          required
        />
      </label>

      <label>
        DNI
        <input value={nationalId} onChange={(e) => setNationalId(e.target.value)} required />
      </label>

      <label>
        Fecha
        <input
          type="date"
          min={todayIso()}
          value={date}
          onChange={(e) => setDate(e.target.value)}
          required
        />
      </label>

      <label>
        Horario
        <select
          value={selectedTime}
          onChange={(e) => setSelectedTime(e.target.value)}
          disabled={!date || loadingSlots}
          required
        >
          <option value="" disabled>
            {!date ? 'Elegí una fecha primero' : loadingSlots ? 'Cargando...' : 'Elegí un horario'}
          </option>
          {slots
            .filter((s) => s.available)
            .map((s) => (
              <option key={s.startTime} value={s.startTime}>
                {formatSlotTime(s.startTime)}
              </option>
            ))}
        </select>
      </label>

      {date && !loadingSlots && slots.length === 0 && (
        <p className="error">No hay turnos habilitados ese día (solo de lunes a viernes).</p>
      )}

      {date && !loadingSlots && slots.length > 0 && slots.every((s) => !s.available) && (
        <p className="error">No quedan horarios libres ese día. Probá con otra fecha.</p>
      )}

      {error && <p className="error">{error}</p>}

      <button type="submit" disabled={loading}>
        {loading ? 'Agendando...' : 'Agendar turno'}
      </button>
    </form>
  )
}
