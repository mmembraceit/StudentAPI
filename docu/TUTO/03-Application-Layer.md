# 03 — Application Layer

The **Application Layer** contains the business logic of the API. It sits between the Presentation Layer (HTTP) and the Infrastructure Layer (database, cache). It defines **what** the system does, without knowing **how** data is stored or **how** HTTP works.

**Project**: `src/Application/StudentApi.Application.csproj`

---

## What Lives Here?

```
Application/
├── Common/
│   └── Exceptions/
│       └── NotFoundException.cs        # Custom business exception
├── Interfaces/
│   ├── IStudentCacheService.cs         # Cache operations contract
│   ├── IStudentRepository.cs           # Persistence contract (in Students/Interfaces/)
│   ├── IUserAuthRepository.cs          # Auth user lookup contract
│   └── IRefreshTokenRepository.cs      # Refresh token lifecycle contract
├── Students/
│   ├── DTOs/
│   │   └── StudentDto.cs              # Output DTO (what clients see)
│   ├── Request/
│   │   ├── CreateStudentRequest.cs    # Input DTO for creation
│   │   ├── UpdateStudentRequest.cs    # Input DTO for updates
│   │   └── DeleteStudentRequest.cs    # Input DTO for deletion
│   ├── Validators/
│   │   ├── CreateStudentRequestValidator.cs
│   │   └── UpdateStudentRequestValidator.cs
│   ├── Mappings/
│   │   └── StudentMappings.cs         # Entity ↔ DTO conversion
│   ├── Interfaces/
│   │   ├── IStudentService.cs         # Use case contract
│   │   └── IStudentRepository.cs      # Persistence contract
│   └── Services/
│       └── StudentService.cs          # Use case implementation
```

---

## The Core Idea: Ports & Adapters

The Application Layer defines **interfaces** (ports) that describe what it needs from the outside world. It never creates database connections or calls Redis directly. Instead:

```
Application Layer defines:         Infrastructure Layer implements:
──────────────────────             ──────────────────────────────
IStudentRepository          →      StudentRepository (EF Core)
IStudentCacheService        →      RedisStudentCacheService / NoOpStudentCacheService
IUserAuthRepository         →      UserAuthRepository (EF Core)
IRefreshTokenRepository     →      RefreshTokenRepository (EF Core)
```

> **Why?** If you ever need to swap SQL Server for PostgreSQL, or Redis for Memcached, you only change the Infrastructure Layer. The Application Layer remains untouched.

---

## DTOs — Data Transfer Objects

### StudentDto (output)

```csharp
public record StudentDto(
    Guid Id,
    Guid TenantId,
    string Name,
    DateOnly DateOfBirth);
```

This is what clients receive. It's a **record** (immutable value type) that mirrors the domain entity's public shape.

### Request records (input)

```csharp
// What the client sends to create a student
public record CreateStudentRequest(
    Guid TenantId,
    string Name,
    DateOnly DateOfBirth);

// What the client sends to update a student (no TenantId — it comes from the route)
public record UpdateStudentRequest(
    string Name,
    DateOnly DateOfBirth);

// Internal DTO for delete operations
public record DeleteStudentRequest(
    Guid Id,
    Guid TenantId);
```

### Why different DTOs for create vs update?

- **Create** needs `TenantId` in the body because the tenant context comes from the payload.
- **Update** doesn't include `TenantId` or `Id` — those come from the URL route and query string.
- **Delete** combines both, built inside the controller from route/query parameters.

This follows the **principle of least privilege** — each request type only carries the data it needs.

---

## Mappings

**File**: `Students/Mappings/StudentMappings.cs`

```csharp
public static class StudentMappings
{
    public static StudentDto ToDto(this Student student)
    {
        return new StudentDto(
            student.Id,
            student.TenantId,
            student.Name,
            student.DateOfBirth);
    }
}
```

This is a manual extension method that converts a `Student` entity into a `StudentDto`. Used as:

```csharp
var studentDto = student.ToDto();
```

> **Why manual mapping instead of AutoMapper/Mapster?** For a simple 1:1 mapping, manual code is more explicit and easier to debug. Auto-mapping libraries add value when you have many complex transformations.

---

## Validators (Task 5 — FluentValidation)

### CreateStudentRequestValidator

```csharp
public sealed class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(request => request.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");

        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.DateOfBirth)
            .NotEqual(default(DateOnly))
            .WithMessage("DateOfBirth is required.")
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("DateOfBirth must be in the past.");
    }
}
```

### UpdateStudentRequestValidator

```csharp
public sealed class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
    public UpdateStudentRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.DateOfBirth)
            .NotEqual(default(DateOnly))
            .WithMessage("DateOfBirth is required.")
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("DateOfBirth must be in the past.");
    }
}
```

**Design notes**:
- Validators live in the **Application Layer**, not the Presentation Layer. Validation is a business rule ("a student must have a name") — not an HTTP concern.
- `MaximumLength(200)` matches the database constraint in `StudentConfiguration`.
- `DateOfBirth` can't be the default value (`0001-01-01`) and must be in the past.
- The `ValidationActionFilter` in the Presentation Layer resolves and executes these validators.

---

## StudentService — The Heart of Business Logic

**File**: `Students/Services/StudentService.cs`

This class implements `IStudentService` and orchestrates all student use cases. It depends on two interfaces: `IStudentRepository` (database) and `IStudentCacheService` (cache).

### GetByIdAsync — Cache-First Read

```csharp
public async Task<StudentDto> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
{
    // 1. Try cache first
    var cachedStudent = await _studentCacheService.GetByIdAsync(id, tenantId, cancellationToken);
    if (cachedStudent is not null)
        return cachedStudent;

    // 2. Fall back to database
    var student = await _studentRepository.GetByIdAsync(id, tenantId, cancellationToken);
    if (student is null)
        throw new NotFoundException($"Student with id '{id}' was not found for tenant '{tenantId}'.");

    // 3. Store in cache for next time
    var studentDto = student.ToDto();
    await _studentCacheService.SetByIdAsync(studentDto, cancellationToken);

    return studentDto;
}
```

**Flow diagram**:

```
GetByIdAsync(id, tenantId)
        │
        ▼
  ┌─── Cache lookup ───┐
  │                     │
  ▼ HIT                 ▼ MISS
Return DTO         DB lookup
                       │
                   ┌───┴───┐
                   │       │
                   ▼ null  ▼ found
              throw 404   Map to DTO
                          → Cache it
                          → Return DTO
```

### CreateAsync — Write-Through Cache

```csharp
public async Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
{
    var student = new Student
    {
        Id = Guid.NewGuid(),
        TenantId = request.TenantId,
        Name = request.Name,
        DateOfBirth = request.DateOfBirth
    };

    await _studentRepository.AddAsync(student, cancellationToken);        // DB insert
    var studentDto = student.ToDto();
    await _studentCacheService.SetByIdAsync(studentDto, cancellationToken);          // Cache the new item
    await _studentCacheService.InvalidateAllAsync(student.TenantId, cancellationToken); // Invalidate list cache

    return studentDto;
}
```

**Why invalidate the list cache?** After adding a new student, the cached list of "all students for tenant X" is stale. Instead of updating it (complex), we just delete it. The next `GetAllAsync` call will rebuild it from the database.

### UpdateAsync — Verify-Then-Update

```csharp
public async Task<StudentDto> UpdateAsync(Guid id, Guid tenantId, UpdateStudentRequest request, ...)
{
    var currentStudent = await _studentRepository.GetByIdAsync(id, tenantId, cancellationToken);
    if (currentStudent is null)
        throw new NotFoundException(...);

    var updatedStudent = currentStudent with   // record 'with' expression
    {
        Name = request.Name,
        DateOfBirth = request.DateOfBirth
    };

    await _studentRepository.UpdateAsync(updatedStudent, cancellationToken);
    var studentDto = updatedStudent.ToDto();
    await _studentCacheService.SetByIdAsync(studentDto, cancellationToken);
    await _studentCacheService.InvalidateAllAsync(tenantId, cancellationToken);

    return studentDto;
}
```

**Note the `with` expression**: Since `Student` is a `record`, we use `with { ... }` to create a new instance with modified properties. The original object is unchanged — records are designed for immutable workflows.

### DeleteAsync — Verify-Then-Delete

```csharp
public async Task DeleteAsync(DeleteStudentRequest request, CancellationToken cancellationToken = default)
{
    var currentStudent = await _studentRepository.GetByIdAsync(request.Id, request.TenantId, cancellationToken);
    if (currentStudent is null)
        throw new NotFoundException(...);

    await _studentRepository.DeleteAsync(request.Id, request.TenantId, cancellationToken);
    await _studentCacheService.InvalidateByIdAsync(request.Id, request.TenantId, cancellationToken);
    await _studentCacheService.InvalidateAllAsync(request.TenantId, cancellationToken);
}
```

Both cache entries are invalidated: the by-id entry and the list.

---

## Interfaces — Contracts for the Outside World

### IStudentRepository

```csharp
public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Student>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Student student, CancellationToken cancellationToken = default);
    Task UpdateAsync(Student student, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}
```

Every method takes `tenantId` — there is no way to accidentally read another tenant's data.

### IStudentCacheService

```csharp
public interface IStudentCacheService
{
    Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, ...);
    Task SetByIdAsync(StudentDto student, ...);
    Task<IReadOnlyList<StudentDto>?> GetAllAsync(Guid tenantId, ...);
    Task SetAllAsync(Guid tenantId, IReadOnlyList<StudentDto> students, ...);
    Task InvalidateByIdAsync(Guid id, Guid tenantId, ...);
    Task InvalidateAllAsync(Guid tenantId, ...);
}
```

Notice: the cache service works with **DTOs**, not domain entities. This is intentional — we cache the serialized output, not internal domain objects.

### IUserAuthRepository & IRefreshTokenRepository

```csharp
public interface IUserAuthRepository
{
    Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
}

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetActiveByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task RevokeAsync(Guid id, string replacedByTokenHash, CancellationToken cancellationToken = default);
}
```

These support the authentication flow (see [06-Auth-And-Postman.md](06-Auth-And-Postman.md)).

---

## NotFoundException — Custom Exception

```csharp
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
```

This is thrown by `StudentService` when a student doesn't exist. The `GlobalExceptionMiddleware` in the Presentation Layer catches it and returns a `404 Not Found` response.

**Why a custom exception?** It creates a clear mapping between business rules ("resource not found") and HTTP semantics (404). The service layer doesn't know about HTTP — it just throws a business-level exception.

---

## Key Takeaways

| Concept | Implementation |
|---------|---------------|
| **Interface segregation** | Small, focused interfaces for each concern (repository, cache, service) |
| **Cache-aside pattern** | Service checks cache → falls back to DB → populates cache |
| **Immutable DTOs** | All DTOs and requests are `record` types |
| **No framework dependencies** | Application Layer only depends on Domain; no EF Core, no ASP.NET |
| **Business exceptions** | `NotFoundException` conveys business meaning, middleware handles HTTP mapping |
| **Validation rules** | FluentValidation in Application, filter execution in Presentation |
| **CancellationToken** | Every async method supports cancellation |
