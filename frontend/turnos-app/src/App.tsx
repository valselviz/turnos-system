import { useCallback, useEffect, useState } from 'react'
import { listarTurnos } from './api'
import TurnoForm from './components/TurnoForm'
import TurnoList from './components/TurnoList'
import type { Turno } from './types'

export default function App() {
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
    <div className="app">
      <header>
        <h1>Sistema de Turnos — Dirección Nacional de Migraciones</h1>
      </header>

      {error && <p className="error global-error">{error}</p>}

      <main>
        <TurnoForm onCreated={cargarTurnos} />
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
      </main>
    </div>
  )
}
