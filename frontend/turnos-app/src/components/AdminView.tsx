import { useCallback, useEffect, useState } from 'react'
import { listAppointments } from '../api'
import AppointmentList from './AppointmentList'
import type { Appointment } from '../types'

export default function AdminView() {
  const [appointments, setAppointments] = useState<Appointment[]>([])
  const [statusFilter, setStatusFilter] = useState('')
  const [serviceTypeFilter, setServiceTypeFilter] = useState('')
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

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
