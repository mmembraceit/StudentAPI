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

## Documentation

- Task Index (copilot-instructions): [docu/tasks/00-TaskDocumentationIndex.md](docu/tasks/00-TaskDocumentationIndex.md)
- Azure Services Scope: [docu/tasks/11-AzureServicesScope.md](docu/tasks/11-AzureServicesScope.md)
- JWT Authorization: [docu/JWTAuthorization.md](docu/JWTAuthorization.md)
- Redis Code References: [docu/RedisCodeReferences.md](docu/RedisCodeReferences.md)
- Service Bus Tutorial: [docu/TUTO/08-Azure-Service-Bus.md](docu/TUTO/08-Azure-Service-Bus.md)
- Serilog Tutorial: [docu/TUTO/09-Serilog.md](docu/TUTO/09-Serilog.md)
