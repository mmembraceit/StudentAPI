# StudentAPI
Fully functional RESTful API in .NET that follows Clean Architecture principles. 

## Quick Start (Local)

1. Start dependencies:

```powershell
docker compose up -d redis sqlserver
```

2. Run the API:

```powershell
dotnet run --project .\src\Presentation\StudentApi.Presentation.csproj
```

The app listens on `http://localhost:5173` in Development profile.

## Postman Smoke Test

1. Login:
- `POST http://localhost:5173/api/auth/login`
- Body:

```json
{
	"username": "admin",
	"password": "admin123"
}
```

2. Use the returned `accessToken` as Bearer token.

3. Call:
- `GET http://localhost:5173/api/students?tenantId=11111111-1111-1111-1111-111111111111`

Expected:
- `200 OK` with Bearer token.
- `401 Unauthorized` without Bearer token.

## Troubleshooting

- `Error: connect ECONNREFUSED 127.0.0.1:5173`
	- API is not running. Start it with the command above.
	- Verify port is open:

```powershell
Test-NetConnection localhost -Port 5173
```

	- Ensure URL in Postman uses `http://localhost:5173` (not `https`).

## Testing

Run tests from the repository root.

### One-command: run all tests

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\run-all-tests.ps1
```

This runs:
- Unit tests
- Integration tests
- Redis/log smoke check

By default, the script auto-starts the API for the smoke check if it is not already running.
Use `-AutoStartApi:$false` if you prefer to manage API startup manually.
The script fails fast if any step fails.

### Unit tests

```powershell
dotnet test .\tests\StudentApi.UnitTests\StudentApi.UnitTests.csproj
```

Detailed per-test output plus TRX file:

```powershell
dotnet test .\tests\StudentApi.UnitTests\StudentApi.UnitTests.csproj --logger "console;verbosity=detailed" --logger "trx;LogFileName=unit-tests.trx"
```

TRX output is written to:

```text
tests/StudentApi.UnitTests/TestResults/unit-tests.trx
```

### Integration tests

```powershell
dotnet test .\tests\StudentApi.IntegrationTests\StudentApi.IntegrationTests.csproj
```

This suite validates:
- Auth flow (`login` success/failure, refresh rotation, invalid refresh rejection).
- Student endpoints (`401` without token, authorized paths, tenant isolation behavior, response envelope assertions).
- Repository behavior (`tenant-filtered reads`, `delete scoped by tenant + id`).

### Observability smoke check (Redis + logs)

1. Ensure the API is running on `http://localhost:5173`.

2. Run the smoke script:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tests\StudentApi.IntegrationTests\smoke\redis-log-smoke.ps1 -BaseUrl 'http://localhost:5173' -TenantId '11111111-1111-1111-1111-111111111111' -LogPath 'src/Presentation/logs/studentapi-20260409.log'
```

The script checks Redis miss/hit flow from real HTTP calls and validates key log patterns.

## Documentation

- Task Index (copilot-instructions): [docu/tasks/00-TaskDocumentationIndex.md](docu/tasks/00-TaskDocumentationIndex.md)
- Azure Services Scope: [docu/tasks/11-AzureServicesScope.md](docu/tasks/11-AzureServicesScope.md)
- JWT Authorization: [docu/JWTAuthorization.md](docu/JWTAuthorization.md)
- Redis Code References: [docu/RedisCodeReferences.md](docu/RedisCodeReferences.md)
- Service Bus Tutorial: [docu/TUTO/08-Azure-Service-Bus.md](docu/TUTO/08-Azure-Service-Bus.md)
- Serilog Tutorial: [docu/TUTO/09-Serilog.md](docu/TUTO/09-Serilog.md)
