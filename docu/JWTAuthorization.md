# JWT Authorization Guide

This document describes the JWT authorization implementation added to StudentAPI, including architecture, configuration, role policy, refresh-token flow, and testing steps.

## 1. What Is Implemented

- Access-token authentication using JWT Bearer.
- Role-based authorization policy (`AdminOnly`).
- DB-backed users (`UserAccounts`) with hashed passwords (PBKDF2 + SHA256).
- Refresh tokens persisted in DB (`RefreshTokens`) with token rotation.
- Seeded admin user for local development.

## 2. Main Components

### Presentation

- `src/StudentApi.Presentation/Program.cs`
  - JWT authentication and token validation.
  - Authorization policy registration (`AdminOnly`).
- `src/StudentApi.Presentation/Controllers/AuthController.cs`
  - `POST /api/auth/login`
  - `POST /api/auth/refresh`
- `src/StudentApi.Presentation/Authentication/JwtTokenService.cs`
  - Creates access tokens with `Name` and `Role` claims.
- `src/StudentApi.Presentation/Authentication/Pbkdf2PasswordHasher.cs`
  - Verifies password hashes from DB.
- `src/StudentApi.Presentation/Controllers/StudentsController.cs`
  - Protected using `[Authorize(Policy = "AdminOnly")]`.

### Application

- `src/StudentApi.Application/Interfaces/IUserAuthRepository.cs`
- `src/StudentApi.Application/Interfaces/IRefreshTokenRepository.cs`

### Domain

- `src/StudentApi.Domain/Entities/UserAccount.cs`
- `src/StudentApi.Domain/Entities/RefreshToken.cs`

### Infrastructure

- `src/StudentApi.Infrastructure/Repositories/UserAuthRepository.cs`
- `src/StudentApi.Infrastructure/Repositories/RefreshTokenRepository.cs`
- `src/StudentApi.Infrastructure/Configurations/UserAccountConfiguration.cs`
- `src/StudentApi.Infrastructure/Configurations/RefreshTokenConfiguration.cs`
- `src/StudentApi.Infrastructure/Persistence/ApplicationDbContext.cs`

## 3. Database Migrations

- `src/StudentApi.Infrastructure/Persistence/Migrations/20260407072549_AddUserAccounts.cs`
- `src/StudentApi.Infrastructure/Persistence/Migrations/20260407074609_AddRefreshTokens.cs`

The `UserAccounts` migration seeds an admin user:

- Username: `admin`
- Role: `Admin`
- Password: stored as PBKDF2 hash (not plaintext)

## 4. Required Configuration

JWT key is required from environment variable (recommended for security):

- `Jwt__Key`

Other JWT settings are loaded from appsettings:

- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:ExpirationMinutes`

### Local Run Example (PowerShell)

```powershell
$env:Jwt__Key="SuperLongLocalDevJwtSecretKey_ChangeMe_123456"
$env:ASPNETCORE_URLS="http://localhost:5173"
dotnet run --project .\src\StudentApi.Presentation\StudentApi.Presentation.csproj
```

## 5. API Endpoints

### Login

- `POST /api/auth/login`

Request body:

```json
{
  "username": "admin",
  "password": "admin123"
}
```

Response body (simplified):

```json
{
  "success": true,
  "data": {
    "accessToken": "...",
    "expiresAtUtc": "...",
    "refreshToken": "...",
    "refreshTokenExpiresAtUtc": "..."
  },
  "errors": []
}
```

### Refresh

- `POST /api/auth/refresh`

Request body:

```json
{
  "refreshToken": "<refresh-token>"
}
```

If the refresh token is valid and active, the API returns a new access token and a new refresh token.
Old refresh token is revoked (rotation).

### Protected Students CRUD

- `GET /api/students?tenantId=<guid>`
- `GET /api/students/{id}?tenantId=<guid>`
- `POST /api/students`
- `PUT /api/students/{id}?tenantId=<guid>`
- `DELETE /api/students/{id}?tenantId=<guid>`

Requires:

- `Authorization: Bearer <access-token>`
- User role `Admin` (via policy)

## 6. Authorization Behavior

- Missing/invalid access token -> `401 Unauthorized`
- User without required role -> `403 Forbidden`
- Invalid/reused refresh token -> `401 Unauthorized`

## 7. Token Rotation Rules

- Every successful refresh request issues a new refresh token.
- Previous token is revoked and cannot be reused.
- Reuse attempt returns `401 Unauthorized`.

## 8. Postman Verification Flow

1. `POST /api/auth/login`
2. Save `accessToken` and `refreshToken`.
3. Call Students endpoint with Bearer access token.
4. `POST /api/auth/refresh` using current refresh token.
5. Replace tokens with returned ones.
6. Try refreshing with old refresh token -> expect `401`.

## 9. Common Errors

- `404 Not Found`
  - Wrong URL (missing `/api`), wrong port, or wrong running instance.
- `401 Unauthorized`
  - Missing/expired/invalid access token, invalid login, or invalid refresh token.
- Startup error about JWT key
  - `Jwt__Key` environment variable is missing or too short.

## 10. Security Notes

- Do not commit real secrets into `appsettings.json`.
- Use environment variables or secret manager for `Jwt__Key`.
- For production, add:
  - key rotation strategy,
  - rate limiting / brute-force protection,
  - audit logging for auth events,
  - logout/revoke-all-sessions endpoint.
