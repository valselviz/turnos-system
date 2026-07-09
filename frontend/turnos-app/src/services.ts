export interface Service {
  type: string
  description: string
}

// Single source of truth for service types — used by the booking form, the
// admin filter, and the citizen menu, so we don't repeat the same four
// strings in three different places.
export const SERVICES: Service[] = [
  {
    type: 'Pasaporte',
    description: 'Solicitá tu pasaporte por primera vez o renovalo si está por vencer.',
  },
  {
    type: 'Cédula de identidad',
    description: 'Tramitá tu cédula de identidad por primera vez, por vencimiento o por extravío.',
  },
  {
    type: 'Renovación de documento',
    description: 'Renová un documento de identidad o de viaje próximo a vencer.',
  },
  {
    type: 'Radicación',
    description: 'Iniciá el trámite de radicación para residir legalmente en el país.',
  },
]

export const SERVICE_TYPES = SERVICES.map((s) => s.type)
