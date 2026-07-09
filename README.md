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
4. **El turno debe caer en un horario habilitado.** Solo se pueden agendar turnos de lunes a viernes, de 09:00 a 16:00, en slots fijos de 15 minutos (configurable en `appsettings.json`, sección `HorarioLaboral`). Además, si el slot elegido ya tiene un turno Confirmado, se rechaza la creación con 409 (no tiene sentido dejar crear un Pendiente que ya sabemos que no se va a poder confirmar). Ver `Services/HorarioService.cs`.

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

Ojo: en Postgres instalado vía Homebrew en Mac no existe el rol `postgres` por defecto — el superusuario se llama igual que tu usuario del sistema operativo, y no requiere contraseña (autenticación `trust` local). Si es tu caso, ajustá `Username` a tu usuario de macOS y sacá `Password` de la connection string. Si tu Postgres usa otro usuario/contraseña (por ejemplo, corriendo en Docker), editá `appsettings.Development.json` (no versionado si lo agregás a `.gitignore`, o usá `dotnet user-secrets`) con tu propia `ConnectionStrings:TurnosDb`.

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
| GET | `/horarios-disponibles?fecha=2026-08-01` | Lista los slots del día (lunes a viernes, 09:00-16:00, cada 15 min) marcando cuáles ya tienen un turno Confirmado |

## 3. Frontend

```bash
cd frontend/turnos-app
cp .env.example .env   # ajustar VITE_API_URL si la API no corre en localhost:5080
npm install
npm run dev
```

La SPA queda en `http://localhost:5173`.

## Correr todo con Docker (alternativa a los pasos 1-3)

Requiere [Docker](https://www.docker.com/) y Docker Compose (viene incluido en Docker Desktop).

```bash
docker compose up --build
```

Esto levanta tres servicios: Postgres, el backend (aplica las migraciones pendientes solo al arrancar, no hace falta correr `dotnet ef` a mano) y el frontend servido con nginx. Quedan en los mismos puertos que en desarrollo local: API en `http://localhost:5080` (Swagger incluido) y SPA en `http://localhost:5173`.

Los datos de Postgres quedan en un volumen (`turnos_db_data`), así que sobreviven a un `docker compose down`. Para arrancar de cero: `docker compose down -v`.

Esto no reemplaza los pasos 1-3 — son dos formas independientes de correr el proyecto, usá la que te resulte más cómoda.

## Tests

```bash
cd backend/Turnos.Api.Tests
dotnet test
```

Cubren las cuatro reglas de negocio: `HorarioServiceTests` prueba la grilla de slots de forma aislada (sin base de datos), y `TurnosControllerTests` prueba los endpoints usando una base de datos en memoria (EF Core InMemory), sin tocar Postgres real.

## Notas

- No requiere autenticación (fuera de alcance según el enunciado).
- Estilos mínimos, sin librería de UI.
