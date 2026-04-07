# Task 9: Add Structured Logging with Serilog

## Status

Pending

## Current Situation

- Logging exists through default Microsoft logging providers.
- Redis cache behavior and errors are logged with ILogger.
- Serilog integration is not configured.

## Evidence

- src/Presentation/Program.cs
- src/Presentation/Middleware/GlobalExceptionMiddleware.cs
- src/Infrastructure/Caching/RedisStudentCacheService.cs

## Remaining Work

1. Add Serilog packages and bootstrap configuration.
2. Add request logging middleware with correlation/context fields.
3. Configure structured sinks (console/file and optional external sink).
4. Verify structured output shape for request and error logs.
