# 02 — Presentation Layer

The **Presentation Layer** is the outermost layer of the application. It is the ASP.NET Core web host — it receives HTTP requests, delegates work to the Application Layer, and returns HTTP responses.

**Project**: `src/Presentation/StudentApi.Presentation.csproj`

---

## What Lives Here?

```
Presentation/
├── Program.cs                          # Composition root (DI + middleware)
├── Controllers/
│   ├── StudentsController.cs           # CRUD endpoints for students
│   └── AuthController.cs              # Login & refresh-token endpoints
├── Middleware/
│   └── GlobalExceptionMiddleware.cs   # Task 2: central error handling
├── Filters/
│   └── ValidationActionFilter.cs      # Task 5: FluentValidation integration
├── Authentication/
│   ├── JwtOptions.cs                  # Strongly typed JWT settings
│   ├── IJwtTokenService.cs            # Token generation contract
│   ├── JwtTokenService.cs             # HMAC-SHA256 JWT implementation
│   ├── IPasswordHasher.cs             # Password verification contract
│   ├── Pbkdf2PasswordHasher.cs        # PBKDF2-SHA256 implementation
│   ├── IRefreshTokenService.cs        # Refresh token ops contract
│   └── RefreshTokenService.cs         # Token generation + SHA-256 hashing
├── Common/
│   └── ApiResponse.cs                 # Task 3: generic response wrapper
├── appsettings.json                    # Default configuration
├── appsettings.Development.json        # Dev overrides (Docker endpoints)
├── Properties/launchSettings.json      # IDE run profiles
└── Dockerfile                          # Container build instructions
```

---

## Program.cs — The Composition Root

`Program.cs` is where everything is wired together. It follows the **.NET Minimal Hosting Model** (no `Startup.cs` class). Let's walk through it section by section.

### 1. Validation Registration

```csharp
builder.Services.AddScoped<ValidationActionFilter>();
builder.Services.AddScoped<IValidator<CreateStudentRequest>, CreateStudentRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateStudentRequest>, UpdateStudentRequestValidator>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationActionFilter>();
});
```

**What's happening**:
- `ValidationActionFilter` is registered as a **global action filter** — it runs before every controller action.
- FluentValidation validators are registered in DI so the filter can resolve them at runtime.

**Why global filters?** Controllers never touch validation logic — the filter intercepts the request, finds matching validators, and returns `400 Bad Request` if validation fails. This is the **Separation of Concerns** principle in action.

### 2. Model State Error Formatting

```csharp
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(modelState => modelState.Errors)
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                ? "The request is invalid." : error.ErrorMessage)
            .ToArray();

        return new BadRequestObjectResult(ApiResponse<object?>.FailureResponse(errors));
    };
});
```

**What's happening**: ASP.NET Core has its own built-in model validation (e.g., `[Required]` attributes). This override ensures that even those built-in errors use the same `ApiResponse<T>` format. Without this, the API would return two different error formats.

### 3. JWT Authentication

```csharp
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
{
    throw new InvalidOperationException("JWT key must be configured and be at least 32 characters long.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.Zero  // No grace period for expired tokens
        };
    });
```

**Key design decisions**:
- **Fail-fast at startup**: If the JWT key is missing or too short, the app crashes immediately with a clear message. This prevents running an insecure API accidentally.
- **`ClockSkew = TimeSpan.Zero`**: By default, .NET adds a 5-minute grace period for token expiration. We disable it for strict security.
- **Options Pattern**: `JwtOptions` is bound from configuration using `IOptions<JwtOptions>` — this is the standard .NET way to work with config sections.

### 4. Authorization Policies

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});
```

**What this does**: Defines a named policy `"AdminOnly"` that checks the `role` claim in the JWT. Controllers use `[Authorize(Policy = "AdminOnly")]` to enforce it.

### 5. Middleware Pipeline

```csharp
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

**Order matters!** The middleware pipeline processes requests top-to-bottom:

```
Request → GlobalExceptionMiddleware → HTTPS Redirect → Authentication → Authorization → Controller
Response ←─────────────────────────────────────────────────────────────────────────────── Controller
```

`GlobalExceptionMiddleware` is first so it can catch exceptions from **any** downstream middleware.

---

## StudentsController — CRUD Endpoints

**File**: `Controllers/StudentsController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class StudentsController : ControllerBase
```

### Endpoints

| Method | Route | Action | Request | Response |
|--------|-------|--------|---------|----------|
| `GET` | `/api/students?tenantId={id}` | `GetAll` | Query param | `ApiResponse<IReadOnlyList<StudentDto>>` |
| `GET` | `/api/students/{id}?tenantId={id}` | `GetById` | Route + query | `ApiResponse<StudentDto>` |
| `POST` | `/api/students` | `Create` | JSON body | `ApiResponse<StudentDto>` (201) |
| `PUT` | `/api/students/{id}?tenantId={id}` | `Update` | Route + query + JSON body | `ApiResponse<StudentDto>` |
| `DELETE` | `/api/students/{id}?tenantId={id}` | `Delete` | Route + query | `ApiResponse<object>` |

### How a Controller Action Looks

```csharp
[HttpPost]
public async Task<ActionResult<ApiResponse<StudentDto>>> Create(
    [FromBody] CreateStudentRequest request,
    CancellationToken cancellationToken)
{
    var createdStudent = await _studentService.CreateAsync(request, cancellationToken);

    return CreatedAtAction(
        nameof(GetById),
        new { id = createdStudent.Id, tenantId = createdStudent.TenantId },
        ApiResponse<StudentDto>.SuccessResponse(createdStudent));
}
```

**Notice**:
- The controller does **zero** business logic — it delegates to `IStudentService`.
- It returns `CreatedAtAction` (HTTP 201) with a `Location` header pointing to the new resource.
- `CancellationToken` is passed through — if the client disconnects, the operation is cancelled.

---

## AuthController — Login & Token Refresh

**File**: `Controllers/AuthController.cs`

This controller is **not** behind the `AdminOnly` policy. Its endpoints use `[AllowAnonymous]`.

### Login Flow (`POST /api/auth/login`)

```
Client sends { username, password }
       ↓
1. Look up user by username (must be active)
2. Verify password using PBKDF2-SHA256
3. Generate JWT access token (60 min lifetime)
4. Generate refresh token (7 day lifetime)
5. Hash refresh token with SHA-256
6. Store hash + metadata in DB
7. Return both tokens to client
```

### Refresh Flow (`POST /api/auth/refresh`)

```
Client sends { refreshToken }
       ↓
1. Hash the incoming refresh token
2. Find matching active (non-revoked, non-expired) record in DB
3. Look up the user by stored username
4. Generate new access + refresh tokens
5. Revoke old refresh token (mark with RevokedAtUtc + ReplacedByTokenHash)
6. Store new refresh token hash
7. Return new token pair
```

> **Why token rotation?** If a refresh token is stolen, the attacker can use it once — but the next time the legitimate user refreshes, the old token is already revoked and the theft is detected.

---

## GlobalExceptionMiddleware (Task 2)

**File**: `Middleware/GlobalExceptionMiddleware.cs`

This middleware wraps the entire request pipeline in a `try/catch`. It maps exception types to HTTP status codes:

| Exception Type | HTTP Status | Log Level |
|----------------|-------------|-----------|
| `NotFoundException` | 404 Not Found | Warning |
| `ValidationException` | 400 Bad Request | Warning |
| Everything else | 500 Internal Server Error | Error |

```csharp
var (statusCode, errors, logLevel) = exception switch
{
    NotFoundException notFoundException =>
        (StatusCodes.Status404NotFound, new[] { notFoundException.Message }, LogLevel.Warning),

    ValidationException validationException =>
        (StatusCodes.Status400BadRequest,
         validationException.Errors.Select(error => error.ErrorMessage).ToArray(),
         LogLevel.Warning),

    _ => (StatusCodes.Status500InternalServerError,
         new[] { "An unexpected error occurred." },
         LogLevel.Error)
};
```

**Why is this important?**
- **Consistency**: Every error returns the same `ApiResponse<T>` structure with `success: false`.
- **Security**: 500 errors never leak stack traces or internal messages to the client.
- **Logging**: 4xx errors are warnings (client's fault), 5xx are errors (our fault).

---

## ValidationActionFilter (Task 5)

**File**: `Filters/ValidationActionFilter.cs`

This is the bridge between ASP.NET Core's filter pipeline and FluentValidation. It runs **before** the controller action is invoked.

```csharp
public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
{
    var errors = new List<string>();

    foreach (var argument in context.ActionArguments.Values.Where(value => value is not null))
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(argument!.GetType());
        var validators = context.HttpContext.RequestServices.GetServices(validatorType);

        foreach (var validator in validators.Cast<IValidator>())
        {
            var validationContext = CreateValidationContext(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
            errors.AddRange(result.Errors.Select(error => error.ErrorMessage));
        }
    }

    if (errors.Count > 0)
    {
        context.Result = new BadRequestObjectResult(ApiResponse<object?>.FailureResponse(errors));
        return;  // Short-circuit — controller action never runs
    }

    await next();  // Proceed to controller
}
```

**How it works step by step**:
1. Iterates over all action arguments (e.g., `CreateStudentRequest`).
2. Constructs the generic `IValidator<CreateStudentRequest>` type via reflection.
3. Resolves all matching validators from the DI container.
4. Runs each validator and collects errors.
5. If any errors exist → 400 response, controller never runs.
6. If no errors → calls `next()` to proceed normally.

---

## ApiResponse\<T> (Task 3)

**File**: `Common/ApiResponse.cs`

```csharp
public record ApiResponse<T>(bool Success, T? Data, IReadOnlyList<string> Errors)
{
    public static ApiResponse<T> SuccessResponse(T? data)
        => new(true, data, Array.Empty<string>());

    public static ApiResponse<T> FailureResponse(IEnumerable<string> errors)
        => new(false, default, errors.Distinct().ToArray());
}
```

**Every API response** looks like this:

```json
// Success
{
  "success": true,
  "data": { "id": "...", "name": "John" },
  "errors": []
}

// Failure
{
  "success": false,
  "data": null,
  "errors": ["Student with id '...' was not found for tenant '...'"]
}
```

**Why use this pattern?**
- Frontend code can always check `response.success` instead of parsing HTTP status codes.
- Error messages are always in the same place (`errors` array).
- `.Distinct()` prevents duplicate error messages.

---

## Dockerfile

The API uses a **multi-stage build**:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# Restore + publish in SDK image

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
# Copy published output into lightweight runtime image
EXPOSE 8080
```

**Why multi-stage?** The SDK image is large (~1 GB) because it includes the compiler. The runtime image is small (~200 MB) — it only includes what's needed to run the compiled code.

---

## Key Takeaways

| Concept | Where It's Applied |
|---------|--------------------|
| **Thin controllers** | Controllers only delegate to `IStudentService` — no business logic |
| **Fail-fast configuration** | JWT key validated at startup, not at first request |
| **Consistent error format** | `ApiResponse<T>` used everywhere (middleware, filters, controllers) |
| **Separation of concerns** | Validation in filters, error mapping in middleware, business logic in services |
| **CancellationToken propagation** | Every async method accepts and passes `CancellationToken` |
| **Options pattern** | `JwtOptions` bound from config, injected via `IOptions<JwtOptions>` |
