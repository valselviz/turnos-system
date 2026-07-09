import { useState } from 'react'
import AppointmentForm from './AppointmentForm'
import { SERVICES } from '../services'
import { formatDate, formatTime, formatNationalId, formatStatus } from '../format'
import type { Appointment } from '../types'

type Step =
  | { type: 'menu' }
  | { type: 'form'; serviceType: string }
  | { type: 'confirmation'; appointment: Appointment }

export default function CitizenView() {
  const [step, setStep] = useState<Step>({ type: 'menu' })

  if (step.type === 'confirmation') {
    const appointment = step.appointment
    return (
      <div className="appointment-confirmation">
        <h2>Turno agendado</h2>
        <dl>
          <dt>Ciudadano</dt>
          <dd>{appointment.citizenName}</dd>
          <dt>DNI</dt>
          <dd>{formatNationalId(appointment.nationalId)}</dd>
          <dt>Trámite</dt>
          <dd>{appointment.serviceType}</dd>
          <dt>Fecha</dt>
          <dd>{formatDate(appointment.scheduledAt)}</dd>
          <dt>Hora</dt>
          <dd>{formatTime(appointment.scheduledAt)}</dd>
          <dt>Estado</dt>
          <dd>{formatStatus(appointment.status)}</dd>
        </dl>
        <button onClick={() => setStep({ type: 'menu' })}>Agendar otro turno</button>
      </div>
    )
  }

  if (step.type === 'form') {
    return (
      <div>
        <button
          type="button"
          className="link-button back-to-menu"
          onClick={() => setStep({ type: 'menu' })}
        >
          ‹ Volver al listado de trámites
        </button>
        <AppointmentForm
          serviceType={step.serviceType}
          onCreated={(appointment) => setStep({ type: 'confirmation', appointment })}
        />
      </div>
    )
  }

  return (
    <div className="services-menu">
      <h2>¿Qué trámite querés agendar?</h2>
      <ul>
        {SERVICES.map((s) => (
          <li key={s.type} className="service-menu-item">
            <div>
              <h3>{s.type}</h3>
              <p>{s.description}</p>
            </div>
            <button type="button" onClick={() => setStep({ type: 'form', serviceType: s.type })}>
              Agendar
            </button>
          </li>
        ))}
      </ul>
    </div>
  )
}
