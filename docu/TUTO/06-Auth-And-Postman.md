# 06 — Authentication, Authorization & Postman Testing

This document covers the complete JWT authentication system: how tokens are generated, validated, and refreshed — and how to test it all using Postman (or `curl`).

---

## Authentication Architecture Overview

```
        ┌──────────────────────────────────────────────────┐
        │                  Client (Postman)                 │
        └───────────┬──────────────────────┬───────────────┘
                    │ 1. POST /auth/login  │ 3. GET /students (Bearer token)
                    ▼                      ▼
        ┌───────────────────┐   ┌──────────────────────────┐
        │   AuthController  │   │  JWT Middleware (auto)    │
        │   (anonymous)     │   │  Validates token → claims │
        └─────┬─────────────┘   └──────┬───────────────────┘
              │                        │
              │ 2. Returns tokens      │ 4. [Authorize(Policy="AdminOnly")]
              ▼                        ▼
        ┌─────────────────┐   ┌──────────────────────────┐
        │  Client stores  │   │  StudentsController      │
        │  access + refresh│   │  executes with User ctx  │
        └─────────────────┘   └──────────────────────────┘
```

---

## Components Involved

| Component | Layer | File | Responsibility |
|-----------|-------|------|---------------|
| `JwtOptions` | Presentation | `Authentication/JwtOptions.cs` | Strongly-typed JWT settings |
| `JwtTokenService` | Presentation | `Authentication/JwtTokenService.cs` | Creates signed JWT access tokens |
| `Pbkdf2PasswordHasher` | Presentation | `Authentication/Pbkdf2PasswordHasher.cs` | Verifies passwords against stored hashes |
| `RefreshTokenService` | Presentation | `Authentication/RefreshTokenService.cs` | Generates + hashes refresh tokens |
| `AuthController` | Presentation | `Controllers/AuthController.cs` | Login and refresh endpoints |
| `IUserAuthRepository` | Application | `Interfaces/IUserAuthRepository.cs` | User lookup contract |
| `IRefreshTokenRepository` | Application | `Interfaces/IRefreshTokenRepository.cs` | Token lifecycle contract |
| `UserAccount` | Domain | `Entities/UserAccount.cs` | User entity |
| `RefreshToken` | Domain | `Entities/RefreshToken.cs` | Token tracking entity |

---

## JWT Access Token — How It Works

### Generation (`JwtTokenService.GenerateToken`)

```csharp
var claims = new[]
{
    new Claim(JwtRegisteredClaimNames.Sub, username),      // "sub" = subject
    new Claim(JwtRegisteredClaimNames.UniqueName, username),// unique_name
    new Claim(ClaimTypes.Name, username),                   // name
    new Claim(ClaimTypes.Role, role)                        // role (used by [Authorize])
};

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

var token = new JwtSecurityToken(
    issuer: jwtOptions.Issuer,       // "StudentApi"
    audience: jwtOptions.Audience,   // "StudentApiClients"
    claims: claims,
    expires: DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationMinutes),  // 60 min
    signingCredentials: credentials);

return new JwtSecurityTokenHandler().WriteToken(token);
```

**The JWT contains**:
- **Header**: Algorithm (`HS256`) + token type (`JWT`)
- **Payload**: Claims (subject, name, role) + issuer + audience + expiration
- **Signature**: HMAC-SHA256 hash of header+payload using the secret key

> A JWT is **not encrypted** — anyone can decode the payload. The signature only proves it hasn't been tampered with.

### Validation (configured in `Program.cs`)

When a request hits a `[Authorize]` endpoint, the JWT middleware automatically:

1. Extracts the token from the `Authorization: Bearer {token}` header
2. Verifies the signature using the same key
3. Checks that the issuer matches `"StudentApi"`
4. Checks that the audience matches `"StudentApiClients"`
5. Checks that the token hasn't expired
6. Sets `HttpContext.User` with the decoded claims

If any check fails → **401 Unauthorized** is returned automatically.

### Authorization Policies

```csharp
options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
```

After authentication, the `[Authorize(Policy = "AdminOnly")]` attribute checks if the user's `role` claim equals `"Admin"`. If not → **403 Forbidden**.

---

## Password Hashing — PBKDF2-SHA256

### Stored Format

```
{iterations}.{base64_salt}.{base64_hash}
```

Example (the seeded admin password for `admin123`):

```
100000.YgFY3Gm4EwL1lz+uDGx69g==.w8AMSr3pbnGTpY5ZDgOD+9gmwWknPiOYO4q512LezBE=
```

- `100000` — number of PBKDF2 iterations
- `YgFY3Gm4EwL1lz+uDGx69g==` — random salt (base64)
- `w8AMSr3pbnGTpY5ZDgOD+9gmwWknPiOYO4q512LezBE=` — derived hash (base64)

### Verification (`Pbkdf2PasswordHasher.Verify`)

```csharp
public bool Verify(string password, string storedHash)
{
    var parts = storedHash.Split('.', StringSplitOptions.RemoveEmptyEntries);
    // parts[0] = iterations, parts[1] = salt, parts[2] = expected hash

    var calculatedHash = Rfc2898DeriveBytes.Pbkdf2(
        Encoding.UTF8.GetBytes(password),   // User's input
        salt,                                // From stored hash
        iterations,                          // From stored hash
        HashAlgorithmName.SHA256,
        expectedHash.Length);

    return CryptographicOperations.FixedTimeEquals(calculatedHash, expectedHash);
}
```

**Security details**:
- **`CryptographicOperations.FixedTimeEquals`**: Prevents timing attacks. A regular `==` comparison exits early on first mismatch — an attacker could measure response times to guess bytes of the hash. Fixed-time comparison always takes the same amount of time regardless of where the mismatch occurs.
- **100,000 iterations**: Makes brute-force attacks computationally expensive.
- **Random salt**: Prevents rainbow table attacks.

---

## Refresh Tokens — Token Rotation

### Why refresh tokens?

Access tokens are short-lived (60 minutes). Instead of asking the user to log in every hour, the client uses a long-lived refresh token (7 days) to get new access tokens silently.

### Generation (`RefreshTokenService`)

```csharp
public string GenerateToken()
{
    var randomBytes = RandomNumberGenerator.GetBytes(64);  // 64 random bytes
    return WebEncoders.Base64UrlEncode(randomBytes);       // URL-safe Base64
}

public string HashToken(string token)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
    return Convert.ToHexString(bytes);  // Uppercase hex string
}
```

**Critical**: The raw refresh token is returned to the client. Only the **hash** is stored in the database. If the database is compromised, attackers can't use the hashes to authenticate.

### Token Rotation Flow

```
1. Client sends refresh token
          │
          ▼
2. Server hashes it → looks up in DB
          │
   ┌──────┴──────┐
   │ Not found    │ Found + active
   ▼              ▼
 401 error    3. Revoke old token (set RevokedAtUtc + ReplacedByTokenHash)
              4. Generate new access + refresh tokens
              5. Store new refresh token hash in DB
              6. Return new token pair
```

**Why rotation?** If an attacker steals a refresh token and uses it before the legitimate user:
- The attacker gets new tokens (old token revoked)
- When the legitimate user tries their token → it's already revoked → **the system detects the theft**
- At that point, all tokens for the user should ideally be revoked (not yet implemented in this codebase)

### Database Record Lifecycle

```
Token A is created:
  { TokenHash: "AAA", RevokedAtUtc: null, ReplacedByTokenHash: null }

Client refreshes with Token A:
  Token A → { RevokedAtUtc: "2026-04-08T...", ReplacedByTokenHash: "BBB" }
  Token B → { TokenHash: "BBB", RevokedAtUtc: null, ReplacedByTokenHash: null }

Client refreshes with Token B:
  Token B → { RevokedAtUtc: "2026-04-08T...", ReplacedByTokenHash: "CCC" }
  Token C → { TokenHash: "CCC", RevokedAtUtc: null, ReplacedByTokenHash: null }
```

This creates a chain of tokens that can be traced for auditing.

---

## Postman Testing Guide

### 1. Login

| Field | Value |
|-------|-------|
| Method | `POST` |
| URL | `http://localhost:5173/api/auth/login` |
| Body (JSON) | `{"username": "admin", "password": "admin123"}` |

**Response** (200 OK):

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAtUtc": "2026-04-08T15:30:00Z",
    "refreshToken": "dGhpcyBpcyBhIHJhbmRvbSByZWZyZXNoIHRva2VuLi4u...",
    "refreshTokenExpiresAtUtc": "2026-04-15T14:30:00Z"
  },
  "errors": []
}
```

**Postman tip**: Save the `accessToken` value. You'll need it for all other requests.

### 2. Call a Protected Endpoint

| Field | Value |
|-------|-------|
| Method | `GET` |
| URL | `http://localhost:5173/api/students?tenantId=11111111-1111-1111-1111-111111111111` |
| Auth tab | Type: `Bearer Token` → paste the `accessToken` |

Or use the header directly:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

**Response** (200 OK):

```json
{
  "success": true,
  "data": [],
  "errors": []
}
```

### 3. Create a Student

| Field | Value |
|-------|-------|
| Method | `POST` |
| URL | `http://localhost:5173/api/students` |
| Auth | Bearer Token |
| Body (JSON) | See below |

```json
{
  "tenantId": "11111111-1111-1111-1111-111111111111",
  "name": "Alice Johnson",
  "dateOfBirth": "2000-05-15"
}
```

**Response** (201 Created):

```json
{
  "success": true,
  "data": {
    "id": "a1b2c3d4-...",
    "tenantId": "11111111-1111-1111-1111-111111111111",
    "name": "Alice Johnson",
    "dateOfBirth": "2000-05-15"
  },
  "errors": []
}
```

### 4. Test Validation Errors

Send a create request with invalid data:

```json
{
  "tenantId": "00000000-0000-0000-0000-000000000000",
  "name": "",
  "dateOfBirth": "2030-01-01"
}
```

**Response** (400 Bad Request):

```json
{
  "success": false,
  "data": null,
  "errors": [
    "TenantId is required.",
    "'Name' must not be empty.",
    "DateOfBirth must be in the past."
  ]
}
```

### 5. Test Without Token

Call any `/api/students` endpoint without the `Authorization` header.

**Response** (401 Unauthorized) — empty body, HTTP status indicates the issue.

### 6. Refresh the Token

| Field | Value |
|-------|-------|
| Method | `POST` |
| URL | `http://localhost:5173/api/auth/refresh` |
| Body (JSON) | `{"refreshToken": "<your-refresh-token>"}` |

**Response** (200 OK): Same structure as login — new access token + new refresh token.

> After refreshing, the old refresh token is revoked. Using it again returns 401.

### 7. Test Token Expiration

Tokens expire after 60 minutes. After expiration, any request returns 401. Use the refresh endpoint to get a new access token.

---

## Common Authentication Errors

| Error | Status | Cause |
|-------|--------|-------|
| `Invalid username or password.` | 401 | Wrong credentials or user doesn't exist |
| `Invalid refresh token.` | 401 | Token expired, revoked, or doesn't exist |
| Empty 401 response | 401 | Missing or malformed `Authorization` header |
| 403 Forbidden | 403 | Valid token but wrong role (not "Admin") |

---

## curl Examples

```bash
# Login
curl -X POST http://localhost:5173/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'

# Get all students (replace TOKEN with actual access token)
curl http://localhost:5173/api/students?tenantId=11111111-1111-1111-1111-111111111111 \
  -H "Authorization: Bearer TOKEN"

# Create a student
curl -X POST http://localhost:5173/api/students \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN" \
  -d '{"tenantId":"11111111-1111-1111-1111-111111111111","name":"Bob","dateOfBirth":"1999-03-20"}'

# Refresh token
curl -X POST http://localhost:5173/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"YOUR_REFRESH_TOKEN"}'
```

---

## Security Summary

| Aspect | Implementation |
|--------|---------------|
| **Token signing** | HMAC-SHA256 symmetric key (min 32 chars) |
| **Password storage** | PBKDF2-SHA256, 100K iterations, random salt |
| **Timing attacks** | `CryptographicOperations.FixedTimeEquals` |
| **Refresh token storage** | Only SHA-256 hash stored — never the raw token |
| **Token rotation** | Old tokens revoked on refresh, chain tracked |
| **Key management** | JWT key via environment variable, never in appsettings |
| **Clock skew** | `TimeSpan.Zero` — no grace period |
| **Startup validation** | App crashes if JWT key is missing or too short |
