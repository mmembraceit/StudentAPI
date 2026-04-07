# Task 4: Implement JWT Authentication

## Status

Completed

## What Is Implemented

- JWT authentication configured in Program.cs.
- Token validation for issuer, audience, signature, and lifetime.
- Protected endpoints require token and Admin role policy.
- Auth controller supports login and refresh token rotation.
- Claims include identity and role.

## Evidence

- src/Presentation/Program.cs
- src/Presentation/Controllers/AuthController.cs
- src/Presentation/Controllers/StudentsController.cs
- src/Presentation/Authentication/JwtOptions.cs
- src/Presentation/Authentication/JwtTokenService.cs
- src/Presentation/Authentication/RefreshTokenService.cs
- src/Application/Interfaces/IUserAuthRepository.cs
- src/Application/Interfaces/IRefreshTokenRepository.cs
- src/Infrastructure/Repositories/UserAuthRepository.cs
- src/Infrastructure/Repositories/RefreshTokenRepository.cs

## Related Guide

- ../JWTAuthorization.md
