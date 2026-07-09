import { useCallback, useEffect, useState } from 'react'
import { listarTurnos } from '../api'
import TurnoList from './TurnoList'
import type { Turno } from '../types'

/**
 * Vista interna: la lista completa de turnos con estado y acciones. No hay
 * autenticación en este proyecto (fuera de alcance), así que esto es
 * únicamente una separación de interfaz, no un control de acceso real.
 */
export default function VistaAdmin() {
  const [turnos, setTurnos] = useState<Turno[]>([])
  const [filtroEstado, setFiltroEstado] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  const cargarTurnos = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await listarTurnos({ estado: filtroEstado || undefined })
      setTurnos(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error desconocido')
    } finally {
      setLoading(false)
    }
  }, [filtroEstado])

  useEffect(() => {
    cargarTurnos()
  }, [cargarTurnos])

  return (
    <div>
      {error && <p className="error global-error">{error}</p>}
      {loading ? (
        <p>Cargando turnos...</p>
      ) : (
        <TurnoList
          turnos={turnos}
          filtroEstado={filtroEstado}
          onFiltroChange={setFiltroEstado}
          onChanged={cargarTurnos}
          onError={setError}
        />
      )}
    </div>
  )
}
