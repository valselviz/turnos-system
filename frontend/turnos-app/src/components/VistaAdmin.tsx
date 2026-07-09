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
  const [filtroTipoTramite, setFiltroTipoTramite] = useState('')
  const [busquedaInput, setBusquedaInput] = useState('')
  const [busqueda, setBusqueda] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  // Pequeño debounce: esperamos a que la persona deje de tipear antes de
  // disparar el request, en vez de pedir la lista en cada tecla.
  useEffect(() => {
    const id = setTimeout(() => setBusqueda(busquedaInput), 300)
    return () => clearTimeout(id)
  }, [busquedaInput])

  const cargarTurnos = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await listarTurnos({
        estado: filtroEstado || undefined,
        tipoTramite: filtroTipoTramite || undefined,
        busqueda: busqueda || undefined,
      })
      setTurnos(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error desconocido')
    } finally {
      setLoading(false)
    }
  }, [filtroEstado, filtroTipoTramite, busqueda])

  useEffect(() => {
    cargarTurnos()
  }, [cargarTurnos])

  return (
    <div>
      {error && <p className="error global-error">{error}</p>}
      {/* TurnoList (y sus inputs de filtro) queda siempre montado — nunca lo
          reemplazamos por un mensaje de carga, porque eso desmontaría el
          <input> de búsqueda y te haría perder el foco en cada tecla. */}
      <TurnoList
        turnos={turnos}
        loading={loading}
        filtroEstado={filtroEstado}
        onFiltroEstadoChange={setFiltroEstado}
        filtroTipoTramite={filtroTipoTramite}
        onFiltroTipoTramiteChange={setFiltroTipoTramite}
        busqueda={busquedaInput}
        onBusquedaChange={setBusquedaInput}
        onChanged={cargarTurnos}
        onError={setError}
      />
    </div>
  )
}
