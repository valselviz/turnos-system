// Manual, deterministic formatting instead of toLocaleString()/toLocaleDateString():
// those methods depend on the browser/OS locale, and could show dates as
// mm/dd/yyyy if the browser is set to English — not something we want in a
// system built for Uruguay.

export function formatDate(scheduledAtIso: string): string {
  const d = new Date(scheduledAtIso)
  const dd = String(d.getDate()).padStart(2, '0')
  const mm = String(d.getMonth() + 1).padStart(2, '0')
  const yyyy = d.getFullYear()
  return `${dd}/${mm}/${yyyy}`
}

export function formatTime(scheduledAtIso: string): string {
  const d = new Date(scheduledAtIso)
  const hh = String(d.getHours()).padStart(2, '0')
  const mm = String(d.getMinutes()).padStart(2, '0')
  return `${hh}:${mm}`
}

// Backend status values are in English (Pending/Confirmed/Cancelled — see
// AppointmentStatus in the backend); this is the single place that maps them
// to the Spanish label shown in the UI.
const STATUS_LABELS: Record<string, string> = {
  Pending: 'Pendiente',
  Confirmed: 'Confirmado',
  Cancelled: 'Cancelado',
}

export function formatStatus(status: string): string {
  return STATUS_LABELS[status] ?? status
}

// We store the national ID as digits with no dots or dash (simpler to
// validate), and format it only when displaying: x.xxx.xxx-x, with the last
// digit as a check digit. Works with 7 or 8 digits (doesn't assume a fixed length).
export function formatNationalId(nationalId: string): string {
  const digitsOnly = nationalId.replace(/\D/g, '')
  if (digitsOnly.length < 2) return nationalId

  const checkDigit = digitsOnly.slice(-1)
  let base = digitsOnly.slice(0, -1)

  const groups: string[] = []
  while (base.length > 3) {
    groups.unshift(base.slice(-3))
    base = base.slice(0, -3)
  }
  if (base.length > 0) groups.unshift(base)

  return `${groups.join('.')}-${checkDigit}`
}
