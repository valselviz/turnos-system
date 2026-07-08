import { useState } from 'react'
import type { FormEvent, ChangeEvent } from 'react'
import { crearTurno } from '../api'
import type { CrearTurnoInput } from '../types'

const TIPOS_TRAMITE = [
  'Pasaporte',
  'Cédula de identidad',
  'Renovación de documento',
  'Radicación',
] as const

const initialForm: CrearTurnoInput = {
  nombreCiudadano: '',
  dni: '',
  fechaHora: '',
  tipoTramite: TIPOS_TRAMITE[0],
}

interface TurnoFormProps {
  onCreated: () => void
}

export default function TurnoForm({ onCreated }: TurnoFormProps) {
  const [form, setForm] = useState<CrearTurnoInput>(initialForm)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  function handleChange(e: ChangeEvent<HTMLInputElement | HTMLSelectElement>) {
    const { name, value } = e.target
    setForm((prev) => ({ ...prev, [name]: value }))
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      // datetime-local no incluye zona horaria; Date lo interpreta en hora
      // local, que es justo lo que el backend compara contra DateTime.Now.
      const payload: CrearTurnoInput = {
        ...form,
        fechaHora: new Date(form.fechaHora).toISOString(),
      }
      await crearTurno(payload)
      setForm(initialForm)
      onCreated()
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
          name="nombreCiudadano"
          value={form.nombreCiudadano}
          onChange={handleChange}
          required
        />
      </label>

      <label>
        DNI
        <input name="dni" value={form.dni} onChange={handleChange} required />
      </label>

      <label>
        Fecha y hora
        <input
          type="datetime-local"
          name="fechaHora"
          value={form.fechaHora}
          onChange={handleChange}
          required
        />
      </label>

      <label>
        Tipo de trámite
        <select name="tipoTramite" value={form.tipoTramite} onChange={handleChange}>
          {TIPOS_TRAMITE.map((tipo) => (
            <option key={tipo} value={tipo}>{tipo}</option>
          ))}
        </select>
      </label>

      {error && <p className="error">{error}</p>}

      <button type="submit" disabled={loading}>
        {loading ? 'Agendando...' : 'Agendar turno'}
      </button>
    </form>
  )
}
