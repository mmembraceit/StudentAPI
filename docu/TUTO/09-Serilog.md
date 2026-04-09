# 09 — Structured Logging with Serilog

This document explains the current Serilog implementation in StudentAPI. It is written for junior developers, so it focuses on the basic ideas first and then connects them to the codebase.

---

## Current Status

**Serilog is implemented.**

The project now uses Serilog as the main logging provider while the code continues to log through `ILogger<T>`.

That means:
- Logging already exists in the same places as before
- Serilog now formats and writes those logs
- HTTP request logging is enabled
- Logs are written to the console and to rolling log files

So this document explains two things:

1. What logging is doing today
2. How Serilog is now connected to that logging pipeline

---

## What Is Logging?

Logging means writing information about what the application is doing.

Examples:
- The API started successfully
- A request failed with a 404 or 500
- Redis cache had a hit or miss
- A Service Bus event was skipped or sent

Logs are useful for:
- debugging problems
- understanding application behavior
- monitoring production systems
- auditing failures and unusual situations

---

## What Is Structured Logging?

Structured logging means logs are written as data, not just as plain sentences.

Instead of logging only this:

```text
Student 123 failed for tenant 456
```

you log something like this:

```text
Message: Request failed
studentId: 123
tenantId: 456
statusCode: 500
```

This is much better because log tools can search, filter, and group by individual fields.

That is one of the main reasons teams use Serilog.

---

## What The Project Uses Today

The current project uses `ILogger<T>` in the code, with Serilog registered underneath as the actual logging provider.

You can see that in several places.

### 1. Global exception logging

File:

```text
src/Presentation/Middleware/GlobalExceptionMiddleware.cs
```

What it does:
- catches unhandled exceptions
- maps them to HTTP responses
- logs the failure with a log level

Examples:
- `Warning` for business problems like not found or validation errors
- `Error` for unexpected exceptions

### 2. Redis cache logging

File:

```text
src/Infrastructure/Caching/RedisStudentCacheService.cs
```

What it logs:
- `REDIS HIT`
- `REDIS MISS`
- `REDIS SET`
- `REDIS DEL`

This helps developers understand whether reads are coming from Redis or SQL Server.

### 3. Azure Service Bus logging

Files:

```text
src/Infrastructure/Messaging/AzureServiceBusStudentEventPublisher.cs
src/Infrastructure/Messaging/NoOpStudentEventPublisher.cs
```

What they log:
- `SERVICE BUS SEND ...`
- `SERVICE BUS SKIP ...`

This helps verify whether the real publisher or the fallback publisher is active.

### 4. Host and framework logging

The app also logs host lifecycle events like:
- app started
- listening on a port
- environment name

These come from ASP.NET Core itself.

---

## Where Logging Is Configured Today

Current log levels are configured in:

```text
src/Presentation/appsettings.json
src/Presentation/appsettings.Development.json
```

The important section is:

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning"
  }
}
```

This means:
- application logs at `Information` or higher are shown
- ASP.NET Core framework logs are reduced to `Warning` or higher to avoid noise

---

## Why Serilog Helps

Even though logging already worked before, Serilog improves the project in several ways.

### 1. Better structured output

Serilog is designed for structured logging from the start.

### 2. Better sink support

A **sink** is where logs go.

Common sinks:
- console
- file
- Seq
- Elasticsearch
- Azure Monitor

### 3. Easier request logging

Serilog can automatically log one structured record per HTTP request.

### 4. Better enrichment

Serilog can attach extra fields to every log, such as:
- request id
- correlation id
- tenant id
- environment
- machine name

This is very useful in real production systems.

---

## What Task 9 Really Means

Task 9 is not about “adding more logs.”

It is about replacing or improving the logging pipeline so logs are:
- structured
- consistent
- easier to search
- easier to send to external tools

In practice, implementing Serilog usually means:

1. Add Serilog packages
2. Configure Serilog as the app logger in `Program.cs`
3. Add request logging middleware
4. Configure sinks such as console and file
5. Keep using `ILogger<T>` in application code, because Serilog integrates under the hood

That last point is important:

**Most code would still keep `ILogger<T>`.**

You usually do not rewrite all logging calls just because Serilog was added.

---

## What Changed In This Project

These are the files most directly affected by the Serilog implementation.

### Program.cs

File:

```text
src/Presentation/Program.cs
```

This is where Serilog is registered as the main logger.

Responsibilities now include:
- bootstrap logger
- read Serilog config
- call `UseSerilog()`
- add request logging middleware

### appsettings files

Files:

```text
src/Presentation/appsettings.json
src/Presentation/appsettings.Development.json
```

These now contain a `Serilog` section for:
- minimum levels
- sinks
- output templates

### Existing logging classes

Files like these may stay mostly the same:

```text
src/Presentation/Middleware/GlobalExceptionMiddleware.cs
src/Infrastructure/Caching/RedisStudentCacheService.cs
src/Infrastructure/Messaging/AzureServiceBusStudentEventPublisher.cs
```

Why?

Because they already log through `ILogger<T>`, which Serilog can capture automatically.

---

## Example Of Current Logging Style

Today, the code already uses structured placeholders like this:

```csharp
_logger.LogInformation("REDIS HIT {CacheKey}", key);
```

That is good practice.

Even before Serilog is added, this style is already better than string concatenation like:

```csharp
_logger.LogInformation("REDIS HIT " + key);
```

Why?

Because structured placeholders preserve the key as a log field.

So the project is already moving in the right direction.

---

## What A Junior Developer Should Understand

The most important concept is this:

```text
ILogger<T> is the logging API used by the code
Serilog is the logging engine behind that API
```

That means:
- Controllers, middleware, and services can keep using `ILogger<T>`
- Serilog changes how logs are formatted, enriched, and stored
- The application code does not need to know the low-level logging provider details

This is similar to how the app uses interfaces for Redis and Service Bus.

---

## Good Logging Practices For This Project

Whether or not Serilog is implemented yet, these are good practices:

1. Log meaningful events, not noise
2. Use structured placeholders like `{StudentId}` and `{TenantId}`
3. Use the correct log level:
   - `Information` for normal important flow
   - `Warning` for recoverable or client-side issues
   - `Error` for unexpected failures
4. Never log secrets
5. Keep messages short and useful

---

## Current Serilog Setup In StudentAPI

The current Task 9 implementation includes:

1. Console sink for local development
2. Rolling file sink for easier inspection
3. Request logging middleware
4. Error logging from middleware
5. Structured fields for:
   - request path
   - status code
   - tenant id when available
  - trace id

That is enough to satisfy Task 9 well without overengineering it.

---

## Current Reality

- Logging exists
- `ILogger<T>` is already used correctly in several places
- The project logs cache activity, Service Bus activity, requests, and errors
- Serilog is the active logging provider
- Console and rolling file output are configured

---

## Quick Mental Model

Use this mental model:

```text
Code --> ILogger<T> --> Serilog --> Console / File sinks
```

If you understand that, you understand the role Serilog will play in this project.