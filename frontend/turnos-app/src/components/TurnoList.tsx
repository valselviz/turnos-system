import { confirmarTurno, cancelarTurno } from '../api'
import type { Turno, EstadoTurno } from '../types'

const ESTADO_LABEL: Record<EstadoTurno, string> = {
  Pendiente: 'Pendiente',
  Confirmado: 'Confirmado',
  Cancelado: 'Cancelado',
}

interface TurnoListProps {
  turnos: Turno[]
  filtroEstado: string
  onFiltroChange: (estado: string) => void
  onChanged: () => void
  onError: (message: string) => void
}

export default function TurnoList({
  turnos,
  filtroEstado,
  onFiltroChange,
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
        <select value={filtroEstado} onChange={(e) => onFiltroChange(e.target.value)}>
          <option value="">Todos los estados</option>
          <option value="Pendiente">Pendiente</option>
          <option value="Confirmado">Confirmado</option>
          <option value="Cancelado">Cancelado</option>
        </select>
      </div>

      {turnos.length === 0 && <p>No hay turnos para mostrar.</p>}

      {turnos.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>Ciudadano</th>
              <th>DNI</th>
              <th>Fecha y hora</th>
              <th>Trámite</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {turnos.map((t) => (
              <tr key={t.id}>
                <td>{t.nombreCiudadano}</td>
                <td>{t.dni}</td>
                <td>{new Date(t.fechaHora).toLocaleString()}</td>
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
