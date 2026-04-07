# JWT Authorization Guide

This document explains how JWT authentication and authorization are implemented in StudentAPI and how to test them in Postman.

## 1. What Is Implemented

- Access tokens with JWT Bearer authentication.
- Role-based authorization with policy `AdminOnly`.
- User validation against DB table `UserAccounts`.
- Password verification with PBKDF2 + SHA256.
- Refresh tokens stored in DB table `RefreshTokens`.
- Refresh-token rotation (old token revoked after successful refresh).

## 2. Source Files Involved

### Presentation

- `src/Presentation/Program.cs`
  - Registers JWT authentication and token validation.
  - Registers authorization policy `AdminOnly`.
  - Enforces startup validation for `Jwt__Key` (required, min 32 chars).
- `src/Presentation/Controllers/AuthController.cs`
  - `POST /api/auth/login`
  - `POST /api/auth/refresh`
- `src/Presentation/Authentication/JwtTokenService.cs`
  - Creates access token with name and role claims.
- `src/Presentation/Authentication/Pbkdf2PasswordHasher.cs`
  - Verifies hashed password from DB.
- `src/Presentation/Controllers/StudentsController.cs`
  - Protected by `[Authorize(Policy = "AdminOnly")]`.

### Application

- `src/Application/Interfaces/IUserAuthRepository.cs`
- `src/Application/Interfaces/IRefreshTokenRepository.cs`

### Domain

- `src/Domain/Entities/UserAccount.cs`
- `src/Domain/Entities/RefreshToken.cs`

### Infrastructure

- `src/Infrastructure/Repositories/UserAuthRepository.cs`
- `src/Infrastructure/Repositories/RefreshTokenRepository.cs`
- `src/Infrastructure/Configurations/UserAccountConfiguration.cs`
- `src/Infrastructure/Configurations/RefreshTokenConfiguration.cs`
- `src/Infrastructure/Persistence/ApplicationDbContext.cs`

## 3. Database and Seed

Migrations:

- `src/Infrastructure/Persistence/Migrations/20260407072549_AddUserAccounts.cs`
- `src/Infrastructure/Persistence/Migrations/20260407074609_AddRefreshTokens.cs`

Local seeded user:

- Username: `admin`
- Role: `Admin`
- Password: PBKDF2 hash for `admin123`

## 4. Configuration

Required environment variable:

- `Jwt__Key`

JWT options from appsettings:

- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:ExpirationMinutes`

Local run example (PowerShell):

```powershell
$env:Jwt__Key="SuperLongLocalDevJwtSecretKey_ChangeMe_123456"
$env:ASPNETCORE_URLS="http://localhost:5173"
dotnet run --project .\src\Presentation\StudentApi.Presentation.csproj
```

## 5. Endpoint Contracts

### 5.1 Login

- Method: `POST`
- URL: `/api/auth/login`
- Full local URL: `http://localhost:5173/api/auth/login`

Request:

```json
{
  "username": "admin",
  "password": "admin123"
}
```

Success response (shape):

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

### 5.2 Refresh

- Method: `POST`
- URL: `/api/auth/refresh`
- Full local URL: `http://localhost:5173/api/auth/refresh`

Request:

```json
{
  "refreshToken": "<refresh-token>"
}
```

Behavior:

- Valid active refresh token returns new access token and new refresh token.
- Old refresh token is revoked and cannot be reused.

### 5.3 Protected Students Endpoints

- `GET /api/students?tenantId=<guid>`
- `GET /api/students/{id}?tenantId=<guid>`
- `POST /api/students`
- `PUT /api/students/{id}?tenantId=<guid>`
- `DELETE /api/students/{id}?tenantId=<guid>`

Requirements:

- Header `Authorization: Bearer <access-token>`
- Token must contain role `Admin` to satisfy policy `AdminOnly`

## 6. Postman Step-By-Step

Create Postman environment variables:

- `baseUrl = http://localhost:5173`
- `tenantId = 11111111-1111-1111-1111-111111111111`
- `accessToken` (empty initially)
- `refreshToken` (empty initially)

### Step 1: Login

Request:

- `POST {{baseUrl}}/api/auth/login`
- Body:

```json
{
  "username": "admin",
  "password": "admin123"
}
```

Expected status: `200`

Optional Tests tab script to save tokens:

```javascript
pm.test("Login status is 200", function () {
  pm.response.to.have.status(200);
});

const json = pm.response.json();
pm.environment.set("accessToken", json.data.accessToken);
pm.environment.set("refreshToken", json.data.refreshToken);
```

### Step 2: Protected GET without token

Request:

- `GET {{baseUrl}}/api/students?tenantId={{tenantId}}`
- No Authorization header

Expected status: `401`

### Step 3: Protected GET with token

Request:

- Same URL as Step 2
- Authorization: Bearer Token -> `{{accessToken}}`

Expected status: `200`

### Step 4: Refresh token

Request:

- `POST {{baseUrl}}/api/auth/refresh`
- Body:

```json
{
  "refreshToken": "{{refreshToken}}"
}
```

Expected status: `200`

Optional Tests script to rotate locally stored tokens:

```javascript
pm.test("Refresh status is 200", function () {
  pm.response.to.have.status(200);
});

const json = pm.response.json();
pm.environment.set("accessToken", json.data.accessToken);
pm.environment.set("refreshToken", json.data.refreshToken);
```

### Step 5: Try old refresh token again

Request:

- Re-send refresh request with previous already-used token

Expected status: `401` (or `400`, depending on global handling)

## 7. Expected Auth Outcomes

- Missing access token -> `401 Unauthorized`
- Invalid/expired access token -> `401 Unauthorized`
- Valid token but insufficient role -> `403 Forbidden`
- Invalid login credentials -> `401 Unauthorized`
- Invalid or reused refresh token -> `401 Unauthorized`

## 8. Troubleshooting

### 404 on login

Use this exact local URL:

- `http://localhost:5173/api/auth/login`

Common causes:

- Missing `/api` prefix (`/auth/login` is not mapped)
- Wrong port (for Docker API use container-mapped port)
- Different instance running than expected

### Startup failure for JWT

If app fails at startup with JWT configuration error:

- Ensure `Jwt__Key` is present
- Ensure key length is at least 32 chars

### 401 on protected endpoints after login

- Confirm `Authorization` type is Bearer Token
- Confirm token value is current access token
- Confirm no extra quotes/spaces around token

## 9. Security Notes

- Do not store real secrets in `appsettings.json`.
- Keep `Jwt__Key` in environment variables or a secret manager.
- Recommended production additions:
  - key rotation,
  - login rate-limiting / brute-force controls,
  - audit logging for auth events,
  - logout / revoke-all-sessions endpoint.
