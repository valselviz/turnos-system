import { useCallback, useEffect, useState } from 'react'
import { listAppointments } from '../api'
import AppointmentList from './AppointmentList'
import type { Appointment } from '../types'

/**
 * Internal view: the full appointment list with status and actions. There's
 * no authentication in this project (out of scope), so this is purely a UI
 * separation, not real access control.
 */
export default function AdminView() {
  const [appointments, setAppointments] = useState<Appointment[]>([])
  const [statusFilter, setStatusFilter] = useState('')
  const [serviceTypeFilter, setServiceTypeFilter] = useState('')
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  // Small debounce: we wait for the person to stop typing before firing the
  // request, instead of requesting the list on every keystroke.
  useEffect(() => {
    const id = setTimeout(() => setSearch(searchInput), 300)
    return () => clearTimeout(id)
  }, [searchInput])

  const loadAppointments = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await listAppointments({
        status: statusFilter || undefined,
        serviceType: serviceTypeFilter || undefined,
        search: search || undefined,
      })
      setAppointments(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error desconocido')
    } finally {
      setLoading(false)
    }
  }, [statusFilter, serviceTypeFilter, search])

  useEffect(() => {
    loadAppointments()
  }, [loadAppointments])

  return (
    <div>
      {error && <p className="error global-error">{error}</p>}
      {/* AppointmentList (and its filter inputs) stays mounted at all times —
          we never swap it out for a loading message, because that would
          unmount the search <input> and make you lose focus on every
          keystroke. */}
      <AppointmentList
        appointments={appointments}
        loading={loading}
        statusFilter={statusFilter}
        onStatusFilterChange={setStatusFilter}
        serviceTypeFilter={serviceTypeFilter}
        onServiceTypeFilterChange={setServiceTypeFilter}
        search={searchInput}
        onSearchChange={setSearchInput}
        onChanged={loadAppointments}
        onError={setError}
      />
    </div>
  )
}
