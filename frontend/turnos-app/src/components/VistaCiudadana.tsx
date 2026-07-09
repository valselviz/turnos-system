import { useState } from 'react'
import TurnoForm from './TurnoForm'
import type { Turno } from '../types'

/**
 * Vista pública: solo el formulario para agendar, y al confirmar, los datos
 * de ESE turno nada más. A propósito no muestra la lista completa ni el
 * estado de ocupación de otros horarios — eso es información de gestión
 * interna, no algo que un ciudadano necesite ver.
 */
export default function VistaCiudadana() {
  const [turnoCreado, setTurnoCreado] = useState<Turno | null>(null)

  if (turnoCreado) {
    return (
      <div className="turno-confirmacion">
        <h2>Turno agendado</h2>
        <dl>
          <dt>Ciudadano</dt>
          <dd>{turnoCreado.nombreCiudadano}</dd>
          <dt>DNI</dt>
          <dd>{turnoCreado.dni}</dd>
          <dt>Trámite</dt>
          <dd>{turnoCreado.tipoTramite}</dd>
          <dt>Fecha y hora</dt>
          <dd>{new Date(turnoCreado.fechaHora).toLocaleString()}</dd>
          <dt>Estado</dt>
          <dd>{turnoCreado.estado}</dd>
        </dl>
        <button onClick={() => setTurnoCreado(null)}>Agendar otro turno</button>
      </div>
    )
  }

  return <TurnoForm onCreated={setTurnoCreado} />
}
