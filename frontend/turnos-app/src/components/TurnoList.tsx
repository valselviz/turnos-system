import { confirmarTurno, cancelarTurno } from '../api'
import { formatearFecha, formatearHora, formatearDni } from '../formato'
import { TIPOS_TRAMITE } from '../tramites'
import type { Turno, EstadoTurno } from '../types'

const ESTADO_LABEL: Record<EstadoTurno, string> = {
  Pendiente: 'Pendiente',
  Confirmado: 'Confirmado',
  Cancelado: 'Cancelado',
}

interface TurnoListProps {
  turnos: Turno[]
  loading: boolean
  filtroEstado: string
  onFiltroEstadoChange: (estado: string) => void
  filtroTipoTramite: string
  onFiltroTipoTramiteChange: (tipoTramite: string) => void
  busqueda: string
  onBusquedaChange: (busqueda: string) => void
  onChanged: () => void
  onError: (message: string) => void
}

export default function TurnoList({
  turnos,
  loading,
  filtroEstado,
  onFiltroEstadoChange,
  filtroTipoTramite,
  onFiltroTipoTramiteChange,
  busqueda,
  onBusquedaChange,
  onChanged,
  onError,
}: TurnoListProps) {
  async function handleConfirmar(id: number) {
    try {
      await confirmarTurno(id)
      onChanged()
    } catch (err) {
      onError(err instanceof Error ? err.message : 'Error desconocido')
    }
  }

  async function handleCancelar(id: number) {
    try {
      await cancelarTurno(id)
      onChanged()
    } catch (err) {
      onError(err instanceof Error ? err.message : 'Error desconocido')
    }
  }

  return (
    <div className="turno-list">
      <div className="turno-list-header">
        <h2>Turnos</h2>

        <div className="turno-list-filtros">
          <input
            type="search"
            placeholder="Buscar por nombre o DNI..."
            value={busqueda}
            onChange={(e) => onBusquedaChange(e.target.value)}
          />

          <select value={filtroTipoTramite} onChange={(e) => onFiltroTipoTramiteChange(e.target.value)}>
            <option value="">Todos los trámites</option>
            {TIPOS_TRAMITE.map((tipo) => (
              <option key={tipo} value={tipo}>{tipo}</option>
            ))}
          </select>

          <select value={filtroEstado} onChange={(e) => onFiltroEstadoChange(e.target.value)}>
            <option value="">Todos los estados</option>
            <option value="Pendiente">Pendiente</option>
            <option value="Confirmado">Confirmado</option>
            <option value="Cancelado">Cancelado</option>
          </select>
        </div>
      </div>

      {loading && <p>Cargando...</p>}

      {!loading && turnos.length === 0 && <p>No hay turnos para mostrar.</p>}

      {!loading && turnos.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>Ciudadano</th>
              <th>DNI</th>
              <th>Fecha</th>
              <th>Hora</th>
              <th>Trámite</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {turnos.map((t) => (
              <tr key={t.id}>
                <td>{t.nombreCiudadano}</td>
                <td>{formatearDni(t.dni)}</td>
                <td>{formatearFecha(t.fechaHora)}</td>
                <td>{formatearHora(t.fechaHora)}</td>
                <td>{t.tipoTramite}</td>
                <td>
                  <span className={`badge badge-${t.estado.toLowerCase()}`}>
                    {ESTADO_LABEL[t.estado] ?? t.estado}
                  </span>
                </td>
                <td className="actions">
                  <button
                    disabled={t.estado !== 'Pendiente'}
                    onClick={() => handleConfirmar(t.id)}
                  >
                    Confirmar
                  </button>
                  <button
                    disabled={t.estado === 'Cancelado'}
                    onClick={() => handleCancelar(t.id)}
                  >
                    Cancelar
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
