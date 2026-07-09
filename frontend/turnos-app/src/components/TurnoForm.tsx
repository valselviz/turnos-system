import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { crearTurno, listarHorariosDisponibles } from '../api'
import type { CrearTurnoInput, HorarioDisponible, Turno } from '../types'

const TIPOS_TRAMITE = [
  'Pasaporte',
  'Cédula de identidad',
  'Renovación de documento',
  'Radicación',
] as const

interface TurnoFormProps {
  onCreated: (turno: Turno) => void
}

function hoyISO(): string {
  const hoy = new Date()
  const yyyy = hoy.getFullYear()
  const mm = String(hoy.getMonth() + 1).padStart(2, '0')
  const dd = String(hoy.getDate()).padStart(2, '0')
  return `${yyyy}-${mm}-${dd}`
}

// El backend devuelve "09:00:00" (TimeOnly); para mostrar solo tomamos HH:mm.
function formatearHora(horaInicio: string): string {
  return horaInicio.slice(0, 5)
}

// FechaHora es una hora de pared, no un instante universal (no hay múltiples
// zonas horarias en juego). Por eso armamos el string a mano en vez de usar
// Date.toISOString(), que convertiría a UTC y correría el valor unas horas.
function formatearFechaHoraNaive(anio: number, mes: number, dia: number, hora: number, minuto: number): string {
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${anio}-${pad(mes)}-${pad(dia)}T${pad(hora)}:${pad(minuto)}:00`
}

export default function TurnoForm({ onCreated }: TurnoFormProps) {
  const [nombreCiudadano, setNombreCiudadano] = useState('')
  const [dni, setDni] = useState('')
  const [tipoTramite, setTipoTramite] = useState<string>(TIPOS_TRAMITE[0])
  const [fecha, setFecha] = useState('')
  const [horaSeleccionada, setHoraSeleccionada] = useState('')
  const [horarios, setHorarios] = useState<HorarioDisponible[]>([])
  const [cargandoHorarios, setCargandoHorarios] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  // La disponibilidad depende de la fecha Y del trámite (cada trámite tiene
  // su propia ventanilla) — recalculamos cada vez que cambia cualquiera de
  // los dos, e invalidamos el horario elegido previamente.
  useEffect(() => {
    if (!fecha) {
      setHorarios([])
      return
    }

    let cancelado = false
    setCargandoHorarios(true)
    setHoraSeleccionada('')

    listarHorariosDisponibles(fecha, tipoTramite)
      .then((data) => {
        if (!cancelado) setHorarios(data)
      })
      .catch((err) => {
        if (!cancelado) setError(err instanceof Error ? err.message : 'Error desconocido')
      })
      .finally(() => {
        if (!cancelado) setCargandoHorarios(false)
      })

    return () => {
      cancelado = true
    }
  }, [fecha, tipoTramite])

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)

    if (!fecha || !horaSeleccionada) {
      setError('Elegí una fecha y un horario.')
      return
    }

    setLoading(true)
    try {
      const [horaStr, minutoStr] = horaSeleccionada.split(':')
      const [anio, mes, dia] = fecha.split('-').map(Number)

      const payload: CrearTurnoInput = {
        nombreCiudadano,
        dni,
        tipoTramite,
        fechaHora: formatearFechaHoraNaive(anio, mes, dia, Number(horaStr), Number(minutoStr)),
      }

      const turno = await crearTurno(payload)

      setNombreCiudadano('')
      setDni('')
      setTipoTramite(TIPOS_TRAMITE[0])
      setFecha('')
      setHoraSeleccionada('')
      onCreated(turno)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error desconocido')
    } finally {
      setLoading(false)
    }
  }

  return (
    <form className="turno-form" onSubmit={handleSubmit}>
      <h2>Nuevo turno</h2>

      <label>
        Nombre del ciudadano
        <input
          value={nombreCiudadano}
          onChange={(e) => setNombreCiudadano(e.target.value)}
          required
        />
      </label>

      <label>
        DNI
        <input value={dni} onChange={(e) => setDni(e.target.value)} required />
      </label>

      <label>
        Tipo de trámite
        <select value={tipoTramite} onChange={(e) => setTipoTramite(e.target.value)}>
          {TIPOS_TRAMITE.map((tipo) => (
            <option key={tipo} value={tipo}>{tipo}</option>
          ))}
        </select>
      </label>

      <label>
        Fecha
        <input
          type="date"
          min={hoyISO()}
          value={fecha}
          onChange={(e) => setFecha(e.target.value)}
          required
        />
      </label>

      <label>
        Horario
        <select
          value={horaSeleccionada}
          onChange={(e) => setHoraSeleccionada(e.target.value)}
          disabled={!fecha || cargandoHorarios}
          required
        >
          <option value="" disabled>
            {!fecha ? 'Elegí una fecha primero' : cargandoHorarios ? 'Cargando...' : 'Elegí un horario'}
          </option>
          {horarios
            .filter((h) => h.disponible)
            .map((h) => (
              <option key={h.horaInicio} value={h.horaInicio}>
                {formatearHora(h.horaInicio)}
              </option>
            ))}
        </select>
      </label>

      {fecha && !cargandoHorarios && horarios.length === 0 && (
        <p className="error">No hay turnos habilitados ese día (solo de lunes a viernes).</p>
      )}

      {fecha && !cargandoHorarios && horarios.length > 0 && horarios.every((h) => !h.disponible) && (
        <p className="error">No quedan horarios libres ese día. Probá con otra fecha.</p>
      )}

      {error && <p className="error">{error}</p>}

      <button type="submit" disabled={loading}>
        {loading ? 'Agendando...' : 'Agendar turno'}
      </button>
    </form>
  )
}
