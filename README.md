# HR System

End-to-end recruitment platform with:
- `HrSystem.Api`: ASP.NET Core Web API
- `HrSystem.Core`: business logic/services layer
- `HrSystem.Data`: Entity Framework Core data access layer
- `hr-system-web`: Angular frontend

The system supports candidate and admin workflows for jobs, applications, CV analysis, interview scheduling, notifications, snapshots/audit trail, dashboards, and user preferences.

## System Overview

### Main capabilities
- JWT authentication (register/login/me/profile update)
- Role-based access (`Admin`, `Candidate`)
- Job posting lifecycle (create/update/close/delete)
- Candidate applications with automatic CV/job matching score
- Application stage management and follow-up notes
- Interview scheduling and status updates
- In-app notifications with optional SMTP/SMS delivery
- Snapshot timeline (auditable activity events)
- Admin and candidate dashboards
- User theme/sidebar preferences

### Architecture
- API controllers in `HrSystem.Api/Controllers`
- Service layer in `HrSystem.Core/Services`
- DTO contracts in `HrSystem.Core/Dtos`
- EF Core DbContext and entities in `HrSystem.Data`
- Angular app in `hr-system-web/src/app`

## Tech Stack

- .NET SDK `10.0` (all backend projects target `net10.0`)
- ASP.NET Core Web API
- Entity Framework Core + SQL Server provider
- SQL Server LocalDB (default local connection)
- Angular `21`
- Node.js + npm (`packageManager` is `npm@11.9.0`)

## Repository Structure

```text
HrSystem/
  HrSystem.Api/        # API host, controllers, appsettings
  HrSystem.Core/       # Services, interfaces, DTOs, options
  HrSystem.Data/       # Entities, DbContext, migrations
  hr-system-web/       # Angular frontend
  scripts/             # Start/stop/validate helpers
```

## Clone Instructions

```powershell
git clone <your-repository-url> HrSystem
cd HrSystem
```

If you are contributing, create your branch after cloning:

```powershell
git checkout -b feature/<short-name>
```

## Prerequisites

Install before first startup:
- Git
- .NET SDK 10
- SQL Server LocalDB (or SQL Server and then update connection string)
- Node.js and npm

## Configuration

Backend config file:
- `HrSystem.Api/appsettings.json`

Important sections:
- `ConnectionStrings:DefaultConnection`
- `Jwt` (`Issuer`, `Audience`, `SecretKey`, `AccessTokenMinutes`, `AdminInviteCode`)
- `Smtp` (`Enabled`, host/port/credentials/from)
- `Sms` (`Enabled`, `ProviderName`, `FromNumber`)
- `Storage:CvFolder` (defaults to `Storage/Cvs`)

### SMTP noreply setup (`noreply@hrsystem.com`)

Configure `HrSystem.Api/appsettings.json` (or environment-specific settings) under `Smtp`:

- `Enabled`: `true`
- `Host`: your SMTP server host
- `Port`: your SMTP port (usually `587` for STARTTLS or `465` for SSL)
- `UseSsl`: `true` for TLS-protected delivery
- `UserName`: `noreply@hrsystem.com`
- `Password`: the mailbox/app password
- `FromAddress`: `noreply@hrsystem.com`
- `FromName`: display sender name (for example `HR System`)

After this is configured, admins can send campaigns from Settings:
- `Include all active users` sends to all active admins and candidates.
- `Include all active candidates` sends to all active candidates.
- Selected checkboxes in the users table sends only chosen users.

Frontend API base URL:
- `hr-system-web/src/environments/environment.ts`
- Current value: `http://localhost:55330/api`

## First Startup (Recommended)

### 1) Start backend

From repo root:

```powershell
dotnet restore
dotnet run --project HrSystem.Api --urls http://localhost:55330
```

Notes:
- On startup, the app applies EF Core migrations automatically.
- Seed data is also applied automatically.

### 2) Start frontend

In a second terminal:

```powershell
cd hr-system-web
npm install
npm start -- --host 0.0.0.0 --port 4200
```

### 3) Open app and docs

- Frontend: `http://localhost:4200`
- Swagger: `http://localhost:55330/swagger`
- Health: `http://localhost:55330/health`

## Startup via Scripts (Windows PowerShell)

From repo root:

Backend:
```powershell
dotnet build HrSystem.Api/HrSystem.Api.csproj
.\scripts\start-backend.ps1
```

Frontend:
```powershell
.\scripts\start-frontend.ps1
```

Stop processes:
```powershell
.\scripts\stop-system.ps1
```

API validation smoke test:
```powershell
.\scripts\validate-system.ps1
```

## Docker Desktop Startup

This repository also includes a full Docker Desktop setup for the API, frontend, SQL Server, and CV storage.

### Docker files included

- `docker-compose.yml`
- `.env.docker.example`
- `.dockerignore`
- `HrSystem.Api/Dockerfile`
- `HrSystem.Api/appsettings.Docker.json`
- `hr-system-web/Dockerfile`
- `hr-system-web/docker/nginx.conf`
- `hr-system-web/docker/app-config.js`

### Docker prerequisites

- Docker Desktop
- Docker Compose v2
- Docker Desktop running in Linux containers mode

### 1. Create a local Docker env file

From repo root:

```powershell
Copy-Item .env.docker.example .env
```

You can run Docker without `.env` because `docker-compose.yml` has local defaults, but copying `.env.docker.example` to `.env` is recommended if you want to change ports, the SQL password, or the JWT secret.

### 2. Build the Docker images

```powershell
docker compose build
```

### 3. Start the full system

```powershell
docker compose up -d
docker compose ps
```

Or build and start in one command:

```powershell
docker compose up -d --build
```

### 4. Open the running system

- Frontend: `http://localhost:4200`
- API: `http://localhost:55330`
- Swagger: `http://localhost:55330/swagger`
- Health: `http://localhost:55330/health`
- SQL Server from host tools: `localhost,14333`

### Docker service names / containers

- `sqlserver` / `hrsystem-sqlserver`
- `api` / `hrsystem-api`
- `web` / `hrsystem-web`

### Docker logs and restart commands

```powershell
docker compose logs -f sqlserver
docker compose logs -f api
docker compose logs -f web

docker compose restart sqlserver
docker compose restart api
docker compose restart web
```

### Stop or reset Docker containers

Stop and remove containers:

```powershell
docker compose down
```

Stop and also remove database/CV volumes:

```powershell
docker compose down -v
```

### What Docker persists

- SQL database data is stored in the `hrsystem_sql_data` named volume.
- Uploaded CV files are stored in the `hrsystem_cv_data` named volume.

## Seeded Accounts (Development)

Created automatically by `DataSeeder`:

- Admin
  - Email: `admin@hrsystem.com`
  - Password: `Admin@HrSystem2026!`
- Candidate
  - Email: `john.candidate@hrsytem.com`
  - Password: `User@12345`
- Candidate
  - Email: `mary.candidate@hrsytem.com`
  - Password: `User@12345`

Admin registration invite code (for role `Admin` during register):
- `HRADMIN2026`

## API Surface (High Level)

- `api/auth`: register, login, me/profile
- `api/jobs`: open/all/detail + admin CRUD
- `api/applications`: apply, mine, admin review/stage/follow-up
- `api/cv`: structured upload, text upload, mine
- `api/interviews`: admin/candidate views, admin schedule/update
- `api/notifications`: list/unread/read actions
- `api/snapshots`: admin latest and user mine
- `api/dashboard`: admin and candidate metrics
- `api/preferences`: user UI preferences
- `api/admin/management`: users/companies/admin email

## CV Upload And Review Flow

Candidate CV uploads now support these file types:
- `.json` using the required structured template
- `.txt`
- `.docx`

Required template file:
- `hr-system-web/public/templates/required-cv-template.json`

Candidate flow:
- Download the required template from the Jobs page.
- Complete the CV in the required structure, or upload a `.txt` or `.docx` CV.
- Upload the CV before applying for jobs so matching and review data can be generated.

Admin flow:
- Open the Admin Dashboard and use the `Candidate CV Reviews` panel.
- Select a candidate application to view the uploaded CV content and score breakdown.
- Enter a test score, choose the next application stage, and write a reply.
- Save the review to update the application and notify the candidate.

Notification behavior:
- When an admin posts a new job, active candidates receive a notification.
- When an admin reviews an application, the candidate receives a notification with the review outcome.

## Development Notes

- CORS allows frontend on `localhost:4200` and `127.0.0.1:4200`.
- HTTPS redirection is disabled in Development and enabled outside Development.
- SMTP and SMS are disabled by default and simulated as success when disabled.
- CV uploads are stored under `HrSystem.Api/Storage/Cvs` by default.

## Troubleshooting

- Backend fails to connect to DB:
  - Ensure LocalDB is installed, or change `ConnectionStrings:DefaultConnection`.
- Frontend cannot call API:
  - Confirm API is running at `http://localhost:55330`.
  - Confirm `hr-system-web/src/environments/environment.ts` matches API URL.
- `start-backend.ps1` fails initially:
  - Run `dotnet build HrSystem.Api/HrSystem.Api.csproj` first (script uses `--no-build`).
- Docker containers fail to start:
  - Run `docker compose ps` and `docker compose logs -f sqlserver`.
  - If SQL Server password was changed, ensure `.env` and `docker-compose.yml` match.
  - Reset the stack with `docker compose down -v` and then `docker compose up -d --build`.

## Security Note

Default secrets and seeded credentials are for local development only. Rotate JWT secret, invite codes, and account passwords before any non-local deployment.
