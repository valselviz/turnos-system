// Formateo manual y determinista en vez de toLocaleString()/toLocaleDateString():
// esos métodos dependen del locale del navegador/sistema operativo, y pueden
// mostrar fechas en formato mm/dd/aaaa si el navegador está en inglés — algo
// que no queremos en un sistema pensado para Uruguay.

export function formatearFecha(fechaHoraIso: string): string {
  const d = new Date(fechaHoraIso)
  const dd = String(d.getDate()).padStart(2, '0')
  const mm = String(d.getMonth() + 1).padStart(2, '0')
  const yyyy = d.getFullYear()
  return `${dd}/${mm}/${yyyy}`
}

export function formatearHora(fechaHoraIso: string): string {
  const d = new Date(fechaHoraIso)
  const hh = String(d.getHours()).padStart(2, '0')
  const mm = String(d.getMinutes()).padStart(2, '0')
  return `${hh}:${mm}`
}

// Guardamos el DNI como dígitos sin puntos ni guión (más simple de validar),
// y lo formateamos solo al mostrarlo: x.xxx.xxx-x, con el último dígito como
// verificador. Funciona con 7 u 8 dígitos (no asume una longitud fija).
export function formatearDni(dni: string): string {
  const soloDigitos = dni.replace(/\D/g, '')
  if (soloDigitos.length < 2) return dni

  const verificador = soloDigitos.slice(-1)
  let base = soloDigitos.slice(0, -1)

  const grupos: string[] = []
  while (base.length > 3) {
    grupos.unshift(base.slice(-3))
    base = base.slice(0, -3)
  }
  if (base.length > 0) grupos.unshift(base)

  return `${grupos.join('.')}-${verificador}`
}
