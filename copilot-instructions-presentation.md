# Project Presentation Guide

This document is a presentation-ready copy of the requirements from `copilot-instructions.md`, expanded with:

- what each point means
- where it is implemented in the project
- an example you can say during the presentation
- how the feature works end to end
- whether the requirement is complete, partial, or missing

## How To Present The Project

Use this high-level flow first:

1. A request enters the API through the Presentation layer.
2. The controller delegates the use case to the Application layer.
3. The Application layer contains the business flow and orchestration.
4. The Infrastructure layer handles EF Core, Redis, SQL Server, and Azure Service Bus.
5. The Domain layer contains the core entity model.

Concrete request path example:

- `POST /api/students` reaches [src/Presentation/Controllers/StudentsController.cs](src/Presentation/Controllers/StudentsController.cs)
- the controller calls [src/Application/Students/Services/StudentService.cs](src/Application/Students/Services/StudentService.cs)
- the service writes through [src/Infrastructure/Repositories/StudentRepository.cs](src/Infrastructure/Repositories/StudentRepository.cs)
- after saving, the service updates cache through [src/Infrastructure/Caching/RedisStudentCacheService.cs](src/Infrastructure/Caching/RedisStudentCacheService.cs)
- the service also publishes an integration event through [src/Infrastructure/Messaging/AzureServiceBusStudentEventPublisher.cs](src/Infrastructure/Messaging/AzureServiceBusStudentEventPublisher.cs)

## Task 1

### Title

Build a RESTful CRUD API with Clean Architecture

### Description

Using the schema:

`Student (id, name, dateOfBirth)`

Create a fully functional RESTful API in .NET that follows Clean Architecture principles.

### Acceptance Criteria

#### Solution is structured into Domain, Application, Infrastructure, Presentation

Status: Complete

Where it is in the project:

- [src/Domain](src/Domain)
- [src/Application](src/Application)
- [src/Infrastructure](src/Infrastructure)
- [src/Presentation](src/Presentation)

How it works:

- Domain contains the core business entity, for example [src/Domain/Entities/Student.cs](src/Domain/Entities/Student.cs).
- Application contains use cases, DTOs, validators, mappings, and service contracts.
- Infrastructure contains technical implementations such as EF Core, repositories, Redis cache, and Azure Service Bus.
- Presentation contains controllers, middleware, authentication configuration, and the API startup pipeline.

What to say in the presentation:

- The project is separated by responsibility, not by technical shortcut.
- Controllers do not talk directly to EF Core.
- Business flow lives in the Application layer, while external concerns live in Infrastructure.

#### CRUD endpoints exist

Status: Complete

Where it is in the project:

- [src/Presentation/Controllers/StudentsController.cs](src/Presentation/Controllers/StudentsController.cs)

Endpoints to mention:

- `GET /api/students?tenantId={tenantId}`
- `GET /api/students/{id}?tenantId={tenantId}`
- `POST /api/students`
- `PUT /api/students/{id}?tenantId={tenantId}`
- `DELETE /api/students/{id}?tenantId={tenantId}`

How it works:

- The controller receives the HTTP request.
- It delegates the work to `IStudentService`.
- The service applies the use-case flow.
- The repository executes the SQL operation through EF Core.
- Responses are returned using the generic `ApiResponse<T>` wrapper.

Examples you can explain:

- `GetAll` reads all students for one tenant and returns `ApiResponse<IReadOnlyList<StudentDto>>`.
- `Create` receives a `CreateStudentRequest`, saves a new student, invalidates tenant list cache, stores the individual cache entry, and publishes a `student.created` event.
- `Delete` first verifies that the student exists in the tenant scope and then removes it.

Supporting files:

- [src/Application/Students/Interfaces/IStudentService.cs](src/Application/Students/Interfaces/IStudentService.cs)
- [src/Application/Students/Services/StudentService.cs](src/Application/Students/Services/StudentService.cs)
- [src/Infrastructure/Repositories/StudentRepository.cs](src/Infrastructure/Repositories/StudentRepository.cs)

#### Database is SQL-based and managed via EF Core migrations

Status: Complete

Where it is in the project:

- [src/Infrastructure/Persistence/ApplicationDbContext.cs](src/Infrastructure/Persistence/ApplicationDbContext.cs)
- [src/Infrastructure/Persistence/Migrations](src/Infrastructure/Persistence/Migrations)
- [src/Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs](src/Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs)

How it works:

- The DbContext defines `DbSet<Student>`, `DbSet<UserAccount>`, and `DbSet<RefreshToken>`.
- Infrastructure registers SQL Server with `UseSqlServer(connectionString)`.
- Database schema changes are versioned with migrations.

Concrete examples:

- [src/Infrastructure/Persistence/Migrations/20260326111013_InitialCreate.cs](src/Infrastructure/Persistence/Migrations/20260326111013_InitialCreate.cs) creates the `Students` table.
- Later migrations add authentication-related tables.

What to say in the presentation:

- Persistence is not handwritten SQL.
- EF Core gives a controlled schema evolution process through migrations.

#### DTOs are used

Status: Complete

Where it is in the project:

- [src/Application/Students/DTOs](src/Application/Students/DTOs)
- [src/Application/Students/Mappings/StudentMappings.cs](src/Application/Students/Mappings/StudentMappings.cs)
- [src/Application/Students/CreateStudentRequest.cs](src/Application/Students/CreateStudentRequest.cs)
- [src/Application/Students/UpdateStudentRequest.cs](src/Application/Students/UpdateStudentRequest.cs)

How it works:

- The API does not expose the domain entity directly.
- The controller accepts request records and returns DTO records.
- Mapping is done in the Application layer with the `ToDto()` extension.

Concrete example:

- [src/Domain/Entities/Student.cs](src/Domain/Entities/Student.cs) is the domain model.
- It is converted to `StudentDto` by [src/Application/Students/Mappings/StudentMappings.cs](src/Application/Students/Mappings/StudentMappings.cs).

What to say in the presentation:

- DTOs protect the API contract from internal model changes.
- This is also aligned with the requirement to use records when possible.

#### Repository pattern is implemented

Status: Complete

Where it is in the project:

- Contract: [src/Application/Students/Interfaces/IStudentRepository.cs](src/Application/Students/Interfaces/IStudentRepository.cs)
- Implementation: [src/Infrastructure/Repositories/StudentRepository.cs](src/Infrastructure/Repositories/StudentRepository.cs)

How it works:

- The Application layer depends on an abstraction, not on EF Core directly.
- The Infrastructure layer implements that abstraction with `ApplicationDbContext`.
- This keeps the Application layer testable and decoupled from persistence technology.

Repository methods you can mention:

- `GetByIdAsync`
- `GetAllAsync`
- `AddAsync`
- `UpdateAsync`
- `DeleteAsync`

Important detail:

- Repository queries are tenant-aware, so every student operation is scoped by `TenantId`.

#### Transactions are handled properly

Status: Partial

Where it is in the project:

- [src/Infrastructure/Repositories/StudentRepository.cs](src/Infrastructure/Repositories/StudentRepository.cs)

How it currently works:

- Each repository write calls `SaveChangesAsync`.
- That means each create, update, or delete is persisted atomically at the repository-call level.

Presentation note:

- This is enough for simple CRUD operations.
- However, there is no explicit Unit of Work or custom transaction boundary that coordinates multiple writes as one larger business transaction.

How to explain it honestly:

- Transaction support exists through EF Core save operations.
- A dedicated transaction orchestration pattern is not implemented yet.

#### At least one Azure integration is demonstrated

Status: Complete

Where it is in the project:

- Redis cache: [src/Infrastructure/Caching/RedisStudentCacheService.cs](src/Infrastructure/Caching/RedisStudentCacheService.cs)
- Service Bus publisher: [src/Infrastructure/Messaging/AzureServiceBusStudentEventPublisher.cs](src/Infrastructure/Messaging/AzureServiceBusStudentEventPublisher.cs)
- DI wiring: [src/Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs](src/Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs)
- Supporting scope note: [docu/tasks/11-AzureServicesScope.md](docu/tasks/11-AzureServicesScope.md)

How Redis works:

- `GetByIdAsync` and `GetAllAsync` in the service first try cache.
- On cache miss, the service reads from the repository.
- The result is serialized and stored in Redis.
- On create, update, or delete, the relevant cache entries are invalidated or refreshed.

How Azure Service Bus works:

- After create, update, or delete, the Application service publishes an integration event.
- Infrastructure serializes that event and sends a message to a Service Bus queue.
- The message subject distinguishes event types such as `student.created`.

What to say in the presentation:

- The project demonstrates two Azure-oriented integrations: caching and asynchronous messaging.
- Redis improves read performance.
- Service Bus prepares the API for integration with other services.

#### SignalR hub is implemented

Status: Missing

Current state:

- There is no SignalR hub class.
- There is no `MapHub` configuration in startup.
- There is no `IHubContext` usage in the student workflow.

How to present it:

- SignalR was part of the original scope but is not implemented in the current codebase.

#### Webhook is triggered on at least one action

Status: Missing

Current state:

- There is no webhook publisher service.
- There is no outgoing HTTP callback after student actions.

How to present it:

- External asynchronous notification is partially addressed through Azure Service Bus events.
- Webhooks specifically are not implemented in the current version.

## Task 2

### Title

Implement Global Error Handling

#### Global exception middleware is implemented

Status: Complete

Where it is in the project:

- [src/Presentation/Middleware/GlobalExceptionMiddleware.cs](src/Presentation/Middleware/GlobalExceptionMiddleware.cs)
- registration in [src/Presentation/Program.cs](src/Presentation/Program.cs)

How it works:

- The middleware wraps the rest of the pipeline with a `try/catch`.
- If an exception escapes from a controller or service, middleware converts it into a controlled HTTP response.

What to say in the presentation:

- Error handling is centralized, so controllers stay focused on business actions.

#### Standardized error responses

Status: Complete

Where it is in the project:

- [src/Presentation/Common/ApiResponse.cs](src/Presentation/Common/ApiResponse.cs)

How it works:

- Every failure response follows the same structure: `success`, `data`, and `errors`.
- Middleware uses `ApiResponse<object?>.FailureResponse(errors)`.

#### Correct HTTP status codes

Status: Complete

Examples:

- `NotFoundException` becomes `404 Not Found`
- `FluentValidation.ValidationException` becomes `400 Bad Request`
- unknown exceptions become `500 Internal Server Error`

Where it is implemented:

- [src/Application/Common/Exceptions/NotFoundException.cs](src/Application/Common/Exceptions/NotFoundException.cs)
- [src/Presentation/Middleware/GlobalExceptionMiddleware.cs](src/Presentation/Middleware/GlobalExceptionMiddleware.cs)

#### Errors are logged

Status: Complete

How it works:

- The middleware logs failures with the correct log level before writing the response.
- Serilog also captures request-level information across the pipeline.

## Task 3

### Title

Introduce a Generic API Response Model

#### Unified response structure

Status: Complete

Where it is in the project:

- [src/Presentation/Common/ApiResponse.cs](src/Presentation/Common/ApiResponse.cs)

How it works:

- `ApiResponse<T>` is a record with `Success`, `Data`, and `Errors`.
- It exposes `SuccessResponse` and `FailureResponse` helper methods.

#### Includes success, data, errors

Status: Complete

What to say in the presentation:

- Every endpoint returns the same outer contract.
- That makes the API easier to consume and document.

#### No raw objects returned

Status: Complete

Examples:

- [src/Presentation/Controllers/StudentsController.cs](src/Presentation/Controllers/StudentsController.cs) wraps all controller results.
- [src/Presentation/Controllers/AuthController.cs](src/Presentation/Controllers/AuthController.cs) also wraps authentication responses.

## Task 4

### Title

Implement JWT Authentication

#### JWT configured

Status: Complete

Where it is in the project:

- [src/Presentation/Program.cs](src/Presentation/Program.cs)
- [src/Presentation/Authentication/JwtOptions.cs](src/Presentation/Authentication/JwtOptions.cs)
- [src/Presentation/Authentication/JwtTokenService.cs](src/Presentation/Authentication/JwtTokenService.cs)

How it works:

- `Program.cs` configures JWT Bearer authentication.
- Token validation checks issuer, audience, lifetime, and signing key.
- The signing key must be at least 32 characters.

#### Protected endpoints require token

Status: Complete

Where it is in the project:

- [src/Presentation/Controllers/StudentsController.cs](src/Presentation/Controllers/StudentsController.cs)

How it works:

- The controller is decorated with `[Authorize(Policy = "AdminOnly")]`.
- Only authenticated users with the `Admin` role can access student management.

#### Unauthorized returns 401

Status: Complete

How it works:

- Invalid or missing JWTs are rejected by the authentication middleware.
- Invalid login and invalid refresh token requests also return `401` from [src/Presentation/Controllers/AuthController.cs](src/Presentation/Controllers/AuthController.cs).

#### Token includes claims

Status: Complete

Where it is in the project:

- [src/Presentation/Authentication/JwtTokenService.cs](src/Presentation/Authentication/JwtTokenService.cs)

Claims to mention:

- `sub`
- `unique_name`
- `ClaimTypes.Name`
- `ClaimTypes.Role`

Extra point for the presentation:

- The project also supports refresh-token rotation through [src/Presentation/Controllers/AuthController.cs](src/Presentation/Controllers/AuthController.cs), which is beyond the minimum acceptance criteria.

## Task 5

### Title

Add Request Validation with FluentValidation

#### Validators exist

Status: Complete

Where it is in the project:

- [src/Application/Students/Validators/CreateStudentRequestValidator.cs](src/Application/Students/Validators/CreateStudentRequestValidator.cs)
- [src/Application/Students/Validators/UpdateStudentRequestValidator.cs](src/Application/Students/Validators/UpdateStudentRequestValidator.cs)

Example rules:

- `TenantId` must not be empty on create.
- `Name` must not be empty and must be at most 200 characters.
- `DateOfBirth` must be provided and must be in the past.

#### Invalid requests return 400

Status: Complete

Where it is in the project:

- [src/Presentation/Filters/ValidationActionFilter.cs](src/Presentation/Filters/ValidationActionFilter.cs)
- configuration in [src/Presentation/Program.cs](src/Presentation/Program.cs)

How it works:

- The action filter inspects action arguments.
- It resolves validators from DI.
- If validation fails, it stops the request and returns `400` with the standard response wrapper.

#### No validation in controllers

Status: Complete

What to say in the presentation:

- Controllers stay thin.
- Validation is moved into the pipeline, which is cleaner and easier to maintain.

## Task 6

### Title

Implement Multi-Tenancy with Tenant Isolation

#### TenantId in all entities

Status: Partial

Where it is in the project:

- [src/Domain/Entities/Student.cs](src/Domain/Entities/Student.cs)

How it works:

- The `Student` entity includes `TenantId` and every student query is scoped by that value.

Important limitation:

- The tenant concept is clear for student data.
- This is not implemented as a shared multi-tenant abstraction across every entity in the whole system.

#### Middleware extracts TenantId

Status: Missing

Current state:

- `tenantId` is passed manually through query string or request body.
- There is no middleware that extracts tenant context from headers, token claims, or host name.

Presentation note:

- Tenant isolation exists in the student workflow, but not through automatic middleware extraction.

#### Queries filtered automatically

Status: Partial

Where it is in the project:

- [src/Infrastructure/Repositories/StudentRepository.cs](src/Infrastructure/Repositories/StudentRepository.cs)

How it works:

- Repository methods explicitly filter by `TenantId`.
- Example: `Where(s => s.TenantId == tenantId)`.

Important limitation:

- There is no EF Core global query filter.
- Isolation depends on passing the tenant correctly into the repository method.

#### Isolation verified

Status: Partial

How to present it:

- The repository design enforces tenant-aware reads and writes for students.
- Full automatic tenant isolation is not finished because middleware and global filters are missing.

## Task 7

### Title

Configure Entity Constraints using IEntityTypeConfiguration

#### Config classes exist

Status: Complete

Where it is in the project:

- [src/Infrastructure/Configurations/StudentConfiguration.cs](src/Infrastructure/Configurations/StudentConfiguration.cs)
- [src/Infrastructure/Configurations/UserAccountConfiguration.cs](src/Infrastructure/Configurations/UserAccountConfiguration.cs)
- [src/Infrastructure/Configurations/RefreshTokenConfiguration.cs](src/Infrastructure/Configurations/RefreshTokenConfiguration.cs)

#### Constraints applied

Status: Complete

Concrete examples:

- `Student.Name` is required and limited to 200 characters.
- `Student.DateOfBirth` is stored as SQL `date`.
- `Student` has an index on `(TenantId, Name)`.
- `UserAccount.Username` is unique.

#### No config in DbContext

Status: Complete

Where it is in the project:

- [src/Infrastructure/Persistence/ApplicationDbContext.cs](src/Infrastructure/Persistence/ApplicationDbContext.cs)

How it works:

- The DbContext simply calls `ApplyConfigurationsFromAssembly(...)`.
- That keeps entity-specific configuration outside the DbContext class.

## Task 8

### Title

Implement Database Seeding

#### Seed data exists

Status: Complete

Where it is in the project:

- [src/Infrastructure/Configurations/UserAccountConfiguration.cs](src/Infrastructure/Configurations/UserAccountConfiguration.cs)

What is seeded:

- An initial `admin` user account is seeded with `HasData(...)`.

#### Runs automatically

Status: Partial

How it currently works:

- Seed data is applied through EF Core migrations and model configuration.
- It appears automatically when the database is created or updated with migrations.

Important limitation:

- There is no dedicated startup seeding service that runs on application boot.
- The task note explicitly wanted startup-time seeding, and that part is not implemented as a separate runtime process.

#### No duplicates

Status: Partial

How it works:

- Migration-based seeding is deterministic because it uses fixed seed data.
- There is no custom runtime idempotent seeding workflow for broader initial data.

## Task 9

### Title

Add Structured Logging with Serilog

#### Logging configured

Status: Complete

Where it is in the project:

- [src/Presentation/Program.cs](src/Presentation/Program.cs)

How it works:

- The application creates a bootstrap logger before building the host.
- The host is then configured with `UseSerilog(...)`.
- Logs are enriched with application and environment metadata.

#### Includes request and error logs

Status: Complete

How it works:

- `UseSerilogRequestLogging(...)` logs HTTP requests.
- `GlobalExceptionMiddleware` logs errors.
- The request logging enrichment adds host, scheme, trace id, and tenant id when available.

#### Structured logs

Status: Complete

Examples you can mention:

- Request log message template: `HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms`
- Redis cache operations log keys such as `REDIS HIT`, `REDIS MISS`, and `REDIS DEL` in [src/Infrastructure/Caching/RedisStudentCacheService.cs](src/Infrastructure/Caching/RedisStudentCacheService.cs)
- Azure Service Bus publishes `SERVICE BUS SEND {Subject} student:{StudentId} tenant:{TenantId}` in [src/Infrastructure/Messaging/AzureServiceBusStudentEventPublisher.cs](src/Infrastructure/Messaging/AzureServiceBusStudentEventPublisher.cs)

## Task 10

### Title

Apply OWASP Security Best Practices

#### Security headers configured

Status: Missing

Current state:

- There is no custom middleware for headers such as `X-Frame-Options`, `X-Content-Type-Options`, or `Content-Security-Policy`.

#### CORS restricted

Status: Missing

Current state:

- There is no `AddCors(...)` or `UseCors(...)` configuration in startup.

#### Verified via inspection

Status: Missing

How to present it:

- Security basics such as JWT authentication and HTTPS redirection are present.
- The specific OWASP response-header and CORS hardening tasks are still pending.

## Closing Summary For The Presentation

### Strong points already implemented

- Clean Architecture separation is clear and easy to explain.
- Student CRUD is complete.
- EF Core persistence and migrations are in place.
- DTOs, validation, authentication, generic responses, and global exception handling are implemented.
- Redis caching and Azure Service Bus messaging are integrated.
- Serilog structured logging is configured.

### Gaps you should mention honestly

- SignalR is not implemented.
- Webhooks are not implemented.
- Multi-tenancy is enforced manually, not through middleware and global filters.
- Transaction orchestration is basic rather than explicit.
- Startup-based seeding is only partial.
- OWASP security headers and CORS hardening are still pending.

### Simple presentation script

You can present the project in this order:

1. Explain the four-layer Clean Architecture.
2. Show the `StudentsController` as the entry point.
3. Show `StudentService` as the business orchestrator.
4. Show `StudentRepository` and `ApplicationDbContext` for persistence.
5. Show `ApiResponse<T>`, validation filter, and exception middleware as cross-cutting quality features.
6. Show JWT authentication and the `AdminOnly` policy.
7. Show Redis cache and Azure Service Bus as real-world integrations.
8. Close with the pending items: SignalR, webhooks, stronger tenant automation, and OWASP hardening.