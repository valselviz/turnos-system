// Ícono genérico tipo escudo, inspirado en la estética de los sitios .gub.uy
// (no es una reproducción del Escudo Nacional — es un diseño simple propio,
// pensado solo para dar identidad visual de "organismo público").
export default function EscudoIcono({ size = 32 }: { size?: number }) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 32 32"
      xmlns="http://www.w3.org/2000/svg"
      aria-hidden="true"
    >
      <path
        d="M16 2 L28 6 V15 C28 22 23 27 16 30 C9 27 4 22 4 15 V6 Z"
        fill="#ffffff"
      />
      <path
        d="M16 4.3 L26 7.6 V15 C26 21 21.8 25.3 16 27.8 C10.2 25.3 6 21 6 15 V7.6 Z"
        fill="#0a3d62"
      />
      <path d="M16 9 L20.5 21 L16 18 L11.5 21 Z" fill="#f2b705" />
    </svg>
  )
}
