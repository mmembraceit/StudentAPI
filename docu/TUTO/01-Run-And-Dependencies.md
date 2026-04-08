# 01 — Running the Project & Dependencies

This document explains how to get StudentAPI running on your machine, what each dependency does, and how the configuration files work together.

---

## Prerequisites

| Tool | Minimum Version | Purpose |
|------|----------------|---------|
| **.NET SDK** | 10.0 | Build and run the API |
| **Docker Desktop** | Any recent | Run SQL Server and Redis containers |
| **Postman** (optional) | Any | Test API endpoints manually |

---

## Project Structure (Solution Level)

```
StudentAPI/
├── docker-compose.yml            # Docker services (SQL Server, Redis, API)
├── StudentAPI.sln                # Solution file
├── src/
│   ├── Presentation/             # ASP.NET Core host (entry point)
│   ├── Application/              # Business logic layer
│   ├── Infrastructure/           # Data access + caching layer
│   └── Domain/                   # Entity definitions
├── tests/
│   ├── StudentApi.UnitTests/
│   └── StudentApi.IntegrationTests/
└── docu/                         # Documentation
```

---

## Step 1 — Start Docker Services

The project needs two external services: **SQL Server 2022** and **Redis 7**. Both are defined in `docker-compose.yml`.

```bash
# From the repository root
docker compose up -d redis sqlserver
```

This starts:

| Service | Image | Port | Purpose |
|---------|-------|------|---------|
| `redis` | `redis:7-alpine` | `6379` | Distributed cache |
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest` | `14333 → 1433` | Primary database |

The SQL Server container has a **healthcheck** that runs `SELECT 1` — Docker marks it healthy only when it can accept queries.

### Verify the services are running

```bash
docker compose ps
```

You should see both `redis` and `sqlserver` with status `Up`.

---

## Step 2 — Understand the Configuration

The API reads its settings from `appsettings.json` files and environment variables.

### appsettings.json (defaults / production)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;..."
  },
  "Jwt": {
    "Issuer": "StudentApi",
    "Audience": "StudentApiClients",
    "ExpirationMinutes": 60
  },
  "Redis": {
    "ConnectionString": "",
    "InstanceName": "StudentApi:"
  }
}
```

> **Key point**: `Redis.ConnectionString` is empty by default. When empty, the API uses `NoOpStudentCacheService` (cache is disabled, all reads go to the database).

### appsettings.Development.json (local dev)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,14333;Database=StudentApiDb;User Id=sa;Password=StudentApi#2026!Db;..."
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "StudentApi:"
  }
}
```

> **Key point**: The Development override points to the Docker containers (SQL on port `14333`, Redis on `6379`).

### JWT Key — a special case

The JWT signing key is **not** stored in `appsettings.json` for security reasons. It is provided via environment variable:

```
Jwt__Key=SuperLongLocalDevJwtSecretKey_ChangeMe_123456
```

In `launchSettings.json`, this is preconfigured for the `http` and `https` profiles. When running manually, you must set it yourself.

> **Why?** Secrets should never live in source-controlled config files. In production, use Azure Key Vault, Docker secrets, or environment variables set by your CI/CD pipeline.

---

## Step 3 — Apply Database Migrations

Before the first run, the database schema must be created. The project has three EF Core migrations:

| Migration | What It Creates |
|-----------|----------------|
| `InitialCreate` | `Students` table with `TenantId` composite index |
| `AddUserAccounts` | `UserAccounts` table + unique `Username` index + seeded admin user |
| `AddRefreshTokens` | `RefreshTokens` table + unique `TokenHash` index |

```bash
# From the repository root
dotnet ef database update --project src/Infrastructure --startup-project src/Presentation
```

> If `dotnet ef` is not installed: `dotnet tool install --global dotnet-ef`

---

## Step 4 — Run the API

### Option A: Using dotnet CLI (recommended for learning)

```powershell
$env:Jwt__Key='SuperLongLocalDevJwtSecretKey_ChangeMe_123456'
$env:ASPNETCORE_URLS='http://localhost:5173'
dotnet run --project .\src\Presentation\StudentApi.Presentation.csproj
```

### Option B: Using Docker Compose (full stack)

```bash
# Set your JWT key in the environment first
export JWT_KEY=SuperLongLocalDevJwtSecretKey_ChangeMe_123456

docker compose up -d
```

This builds and starts the API container alongside SQL Server and Redis. The API is available at `http://localhost:8080`.

### Option C: Using Visual Studio / Rider

Open `StudentAPI.sln`, set `StudentApi.Presentation` as the startup project, and press F5. The `launchSettings.json` handles the JWT key and URL configuration automatically.

---

## Step 5 — Verify It Works

### Check the OpenAPI spec

```
GET http://localhost:5173/openapi/v1.json
```

### Login to get a token

```bash
curl -X POST http://localhost:5173/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

Expected response:

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbG...",
    "expiresAtUtc": "2026-04-08T...",
    "refreshToken": "YWJjZGVm...",
    "refreshTokenExpiresAtUtc": "2026-04-15T..."
  },
  "errors": []
}
```

---

## Dependencies Explained

### NuGet Packages (by layer)

#### Presentation (`StudentApi.Presentation.csproj`)

| Package | Purpose |
|---------|---------|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Validates JWT tokens on incoming requests |
| `Microsoft.AspNetCore.OpenApi` | Generates OpenAPI specification |
| `FluentValidation` | Validation engine (validators registered here) |

#### Application (`StudentApi.Application.csproj`)

| Package | Purpose |
|---------|---------|
| `FluentValidation` | Validator base classes (`AbstractValidator<T>`) |

#### Infrastructure (`StudentApi.Infrastructure.csproj`)

| Package | Purpose |
|---------|---------|
| `Microsoft.EntityFrameworkCore.SqlServer` | SQL Server database provider |
| `Microsoft.EntityFrameworkCore.Design` | Migration tooling (`dotnet ef`) |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | Redis distributed cache provider |

#### Domain (`StudentApi.Domain.csproj`)

No external packages — the domain layer is dependency-free by design.

---

## Environment Variables Summary

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | No | `Production` | Set to `Development` for dev config |
| `Jwt__Key` | **Yes** | *(none)* | JWT signing key (min 32 characters) |
| `ConnectionStrings__DefaultConnection` | No | From appsettings | SQL Server connection string |
| `Redis__ConnectionString` | No | `""` | Redis connection (empty = cache disabled) |

---

## Common Issues

| Problem | Cause | Solution |
|---------|-------|----------|
| `JWT key must be configured and be at least 32 characters long` | Missing `Jwt__Key` env variable | Set the variable before running |
| `Connection string 'DefaultConnection' was not found` | Missing connection string | Ensure `ASPNETCORE_ENVIRONMENT=Development` or set the connection string |
| Cannot connect to SQL Server | Docker container not running or not healthy | Run `docker compose up -d sqlserver` and wait for healthcheck |
| Redis cache misses on every request | Redis not running or connection string empty | Check `docker compose ps redis` and `appsettings.Development.json` |
