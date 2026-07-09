import { useState } from 'react'
import TurnoForm from './TurnoForm'
import { TRAMITES } from '../tramites'
import { formatearFecha, formatearHora, formatearDni } from '../formato'
import type { Turno } from '../types'

type Paso =
  | { tipo: 'menu' }
  | { tipo: 'formulario'; tramite: string }
  | { tipo: 'confirmacion'; turno: Turno }

/**
 * Vista pública. Arranca en un menú de trámites (como el listado de
 * gub.uy/tramites) — elegir uno y tocar "Agendar" abre el formulario con ese
 * trámite ya fijo, sin tener que elegirlo de nuevo en un dropdown. A
 * propósito no muestra la lista completa de turnos ni la ocupación de otros
 * horarios: eso es información de gestión interna, no algo que un ciudadano
 * necesite ver.
 */
export default function VistaCiudadana() {
  const [paso, setPaso] = useState<Paso>({ tipo: 'menu' })

  if (paso.tipo === 'confirmacion') {
    const turno = paso.turno
    return (
      <div className="turno-confirmacion">
        <h2>Turno agendado</h2>
        <dl>
          <dt>Ciudadano</dt>
          <dd>{turno.nombreCiudadano}</dd>
          <dt>DNI</dt>
          <dd>{formatearDni(turno.dni)}</dd>
          <dt>Trámite</dt>
          <dd>{turno.tipoTramite}</dd>
          <dt>Fecha</dt>
          <dd>{formatearFecha(turno.fechaHora)}</dd>
          <dt>Hora</dt>
          <dd>{formatearHora(turno.fechaHora)}</dd>
          <dt>Estado</dt>
          <dd>{turno.estado}</dd>
        </dl>
        <button onClick={() => setPaso({ tipo: 'menu' })}>Agendar otro turno</button>
      </div>
    )
  }

  if (paso.tipo === 'formulario') {
    return (
      <div>
        <button
          type="button"
          className="link-button volver-al-menu"
          onClick={() => setPaso({ tipo: 'menu' })}
        >
          ‹ Volver al listado de trámites
        </button>
        <TurnoForm
          tipoTramite={paso.tramite}
          onCreated={(turno) => setPaso({ tipo: 'confirmacion', turno })}
        />
      </div>
    )
  }

  return (
    <div className="menu-tramites">
      <h2>¿Qué trámite querés agendar?</h2>
      <ul>
        {TRAMITES.map((t) => (
          <li key={t.tipo} className="menu-tramite-item">
            <div>
              <h3>{t.tipo}</h3>
              <p>{t.descripcion}</p>
            </div>
            <button type="button" onClick={() => setPaso({ tipo: 'formulario', tramite: t.tipo })}>
              Agendar
            </button>
          </li>
        ))}
      </ul>
    </div>
  )
}
