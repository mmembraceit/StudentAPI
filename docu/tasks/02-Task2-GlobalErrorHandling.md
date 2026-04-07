# Task 2: Implement Global Error Handling

## Status

Completed

## What Is Implemented

- Centralized exception middleware is implemented.
- Standardized error contract returned from middleware.
- Proper status mapping implemented:
  - NotFoundException -> 404
  - ValidationException -> 400
  - Unhandled -> 500
- Errors are logged with level selection.

## Evidence

- src/Presentation/Middleware/GlobalExceptionMiddleware.cs
- src/Presentation/Common/ApiResponse.cs
- src/Presentation/Program.cs
