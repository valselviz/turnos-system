import { useState } from 'react'
import VistaCiudadana from './components/VistaCiudadana'
import VistaAdmin from './components/VistaAdmin'
import EscudoIcono from './components/EscudoIcono'

type Vista = 'ciudadano' | 'admin'

export default function App() {
  const [vista, setVista] = useState<Vista>('ciudadano')

  return (
    <div className="app">
      <header className="site-header">
        <div className="site-header-inner">
          <div className="site-header-brand">
            <EscudoIcono />
            <div>
              <p className="site-eyebrow">Ministerio del Interior · Dirección Nacional de Migraciones</p>
              <h1>Sistema de Turnos</h1>
            </div>
          </div>

          <div className="site-header-actions">
            <button
              type="button"
              className="link-button"
              onClick={() => setVista(vista === 'ciudadano' ? 'admin' : 'ciudadano')}
            >
              {vista === 'ciudadano' ? 'Vista administrador' : 'Volver a vista ciudadano'}
            </button>
            <span className="gub-badge">gub.uy</span>
          </div>
        </div>
      </header>

      <div className="page-content">
        <main>
          {vista === 'ciudadano' ? <VistaCiudadana /> : <VistaAdmin />}
        </main>
      </div>
    </div>
  )
}
