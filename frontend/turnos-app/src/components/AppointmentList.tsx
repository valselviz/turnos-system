import { confirmAppointment, cancelAppointment } from '../api'
import { formatDate, formatTime, formatNationalId, formatStatus } from '../format'
import { SERVICE_TYPES } from '../services'
import type { Appointment } from '../types'

interface AppointmentListProps {
  appointments: Appointment[]
  loading: boolean
  statusFilter: string
  onStatusFilterChange: (status: string) => void
  serviceTypeFilter: string
  onServiceTypeFilterChange: (serviceType: string) => void
  search: string
  onSearchChange: (search: string) => void
  onChanged: () => void
  onError: (message: string) => void
}

export default function AppointmentList({
  appointments,
  loading,
  statusFilter,
  onStatusFilterChange,
  serviceTypeFilter,
  onServiceTypeFilterChange,
  search,
  onSearchChange,
  onChanged,
  onError,
}: AppointmentListProps) {
  async function handleConfirm(id: number) {
    try {
      await confirmAppointment(id)
      onChanged()
    } catch (err) {
      onError(err instanceof Error ? err.message : 'Error desconocido')
    }
  }

  async function handleCancel(id: number) {
    try {
      await cancelAppointment(id)
      onChanged()
    } catch (err) {
      onError(err instanceof Error ? err.message : 'Error desconocido')
    }
  }

  return (
    <div className="appointment-list">
      <div className="appointment-list-header">
        <h2>Turnos</h2>

        <div className="appointment-list-filters">
          <input
            type="search"
            placeholder="Buscar"
            value={search}
            onChange={(e) => onSearchChange(e.target.value)}
          />

          <select value={serviceTypeFilter} onChange={(e) => onServiceTypeFilterChange(e.target.value)}>
            <option value="">Todos los trámites</option>
            {SERVICE_TYPES.map((type) => (
              <option key={type} value={type}>{type}</option>
            ))}
          </select>

          <select value={statusFilter} onChange={(e) => onStatusFilterChange(e.target.value)}>
            <option value="">Todos los estados</option>
            <option value="Pending">Pendiente</option>
            <option value="Confirmed">Confirmado</option>
            <option value="Cancelled">Cancelado</option>
          </select>
        </div>
      </div>

      {loading && <p>Cargando...</p>}

      {!loading && appointments.length === 0 && <p>No hay turnos para mostrar.</p>}

      {!loading && appointments.length > 0 && (
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
            {appointments.map((a) => (
              <tr key={a.id}>
                <td data-label="Ciudadano">{a.citizenName}</td>
                <td data-label="DNI">{formatNationalId(a.nationalId)}</td>
                <td data-label="Fecha">{formatDate(a.scheduledAt)}</td>
                <td data-label="Hora">{formatTime(a.scheduledAt)}</td>
                <td data-label="Trámite">{a.serviceType}</td>
                <td data-label="Estado">
                  <span className={`badge badge-${a.status.toLowerCase()}`}>
                    {formatStatus(a.status)}
                  </span>
                </td>
                <td data-label="Acciones" className="actions">
                  <button
                    disabled={a.status !== 'Pending'}
                    onClick={() => handleConfirm(a.id)}
                  >
                    Confirmar
                  </button>
                  <button
                    disabled={a.status === 'Cancelled'}
                    onClick={() => handleCancel(a.id)}
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
