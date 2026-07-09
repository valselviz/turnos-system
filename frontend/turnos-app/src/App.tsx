import { useState } from 'react'
import CitizenView from './components/CitizenView'
import AdminView from './components/AdminView'
import ShieldIcon from './components/ShieldIcon'

type View = 'citizen' | 'admin'

export default function App() {
  const [view, setView] = useState<View>('citizen')

  return (
    <div className="app">
      <header className="site-header">
        <div className="site-header-inner">
          <div className="site-header-brand">
            <ShieldIcon />
            <div>
              <p className="site-eyebrow">Ministerio del Interior · Dirección Nacional de Migraciones</p>
              <h1>Sistema de Turnos</h1>
            </div>
          </div>

          <div className="site-header-actions">
            <button
              type="button"
              className="link-button"
              onClick={() => setView(view === 'citizen' ? 'admin' : 'citizen')}
            >
              {view === 'citizen' ? 'Vista administrador' : 'Volver a vista ciudadano'}
            </button>
          </div>
        </div>
      </header>

      <div className="page-content">
        <main>
          {view === 'citizen' ? <CitizenView /> : <AdminView />}
        </main>
      </div>
    </div>
  )
}
