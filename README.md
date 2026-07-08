# Sistema de Turnos — Dirección Nacional de Migraciones

Mini sistema de gestión de turnos: API REST en .NET 8 + SPA en React + PostgreSQL.

## Estructura

```
turnos-system/
├── backend/Turnos.Api/     API REST (.NET 8, EF Core, PostgreSQL)
└── frontend/turnos-app/    SPA (React + TypeScript + Vite)
```

## Reglas de negocio implementadas

1. **No puede existir más de un turno confirmado en el mismo horario.** Se valida en `PUT /turnos/{id}/confirmar` antes de guardar, y además hay un índice único parcial en la base de datos (`IX_Turnos_FechaHora_SoloConfirmados`, sólo sobre filas con `Estado = 'Confirmado'`) como red de seguridad ante condiciones de carrera.
2. **Un turno sólo puede cancelarse si su estado es pendiente o confirmado.** `PUT /turnos/{id}/cancelar` rechaza con 400 si ya está cancelado.
3. **La fecha/hora debe ser futura al crear el turno.** `POST /turnos` valida `fechaHora > DateTime.Now` y rechaza con 400 si no lo es.

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- PostgreSQL corriendo localmente (o accesible por red)

## 1. Base de datos

Crear la base de datos (ajustar usuario/contraseña según tu instalación local):

```bash
createdb turnos_db
```

La connection string por defecto (en `backend/Turnos.Api/appsettings.json`) es:

```
Host=localhost;Port=5432;Database=turnos_db;Username=postgres;Password=postgres
```

Si tu Postgres local usa otro usuario/contraseña, editá `appsettings.Development.json` (no versionado si lo agregás a `.gitignore`, o usá `dotnet user-secrets`) con tu propia `ConnectionStrings:TurnosDb`.

## 2. Backend

```bash
cd backend/Turnos.Api
dotnet restore
dotnet tool install --global dotnet-ef   # si no lo tenés instalado
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

La API queda en `http://localhost:5080` (ver `Properties/launchSettings.json`). Documentación interactiva (Swagger) en `http://localhost:5080/swagger`.

Hay un `global.json` en la raíz del repo que fija el SDK a 8.0.x, por si tenés otras versiones de .NET instaladas en tu máquina.

### Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/turnos` | Crea un turno. Body: `nombreCiudadano`, `dni`, `fechaHora`, `tipoTramite` |
| GET | `/turnos` | Lista turnos. Filtros opcionales: `?estado=Pendiente` y/o `?fecha=2026-08-01` |
| PUT | `/turnos/{id}/confirmar` | Confirma un turno pendiente |
| PUT | `/turnos/{id}/cancelar` | Cancela un turno pendiente o confirmado |

## 3. Frontend

```bash
cd frontend/turnos-app
cp .env.example .env   # ajustar VITE_API_URL si la API no corre en localhost:5080
npm install
npm run dev
```

La SPA queda en `http://localhost:5173`.

## Notas

- No requiere autenticación (fuera de alcance según el enunciado).
- Estilos mínimos, sin librería de UI.
- Sin Docker ni tests automatizados por ahora (mencionados como plus, no como requisito).
