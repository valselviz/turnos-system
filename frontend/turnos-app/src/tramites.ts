export interface Tramite {
  tipo: string
  descripcion: string
}

// Lista única de trámites — la usan tanto el formulario de agendar como el
// filtro de la vista admin y el menú de la vista ciudadano, para no repetir
// los mismos cuatro strings en tres lugares distintos.
export const TRAMITES: Tramite[] = [
  {
    tipo: 'Pasaporte',
    descripcion: 'Solicitá tu pasaporte por primera vez o renovalo si está por vencer.',
  },
  {
    tipo: 'Cédula de identidad',
    descripcion: 'Tramitá tu cédula de identidad por primera vez, por vencimiento o por extravío.',
  },
  {
    tipo: 'Renovación de documento',
    descripcion: 'Renová un documento de identidad o de viaje próximo a vencer.',
  },
  {
    tipo: 'Radicación',
    descripcion: 'Iniciá el trámite de radicación para residir legalmente en el país.',
  },
]

export const TIPOS_TRAMITE = TRAMITES.map((t) => t.tipo)
