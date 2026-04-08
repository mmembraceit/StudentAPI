# StudentAPI — Tutorial & Documentation Index

Welcome to the **StudentAPI** didactic documentation. This guide walks you through a production-style .NET 10 REST API built with **Clean Architecture**, explaining every layer, pattern, and design decision in the codebase.

---

## What Is This Project?

StudentAPI is a fully functional RESTful CRUD API that manages `Student` entities. It demonstrates real-world patterns and technologies used in professional .NET development:

- **Clean Architecture** with strict layer separation
- **JWT Authentication** with refresh-token rotation
- **Multi-Tenancy** (tenant-scoped data isolation)
- **Redis Distributed Cache** with graceful fallback
- **FluentValidation** for request validation
- **Global Error Handling** middleware
- **Generic API Response** contract
- **EF Core** with SQL Server, migrations, entity configurations, and database seeding

---

## Implementation Status (Tasks 1–10)

| # | Task | Status | Details |
|---|------|--------|---------|
| 1 | **Clean Architecture CRUD** | ✅ Done | 4 layers, full CRUD, EF Core, Repository Pattern, DTO mapping, Redis Cache |
| 2 | **Global Error Handling** | ✅ Done | `GlobalExceptionMiddleware` maps exceptions to HTTP status codes |
| 3 | **Generic API Response** | ✅ Done | `ApiResponse<T>` wraps all success/error responses uniformly |
| 4 | **JWT Authentication** | ✅ Done | HMAC-SHA256 tokens, refresh rotation, PBKDF2 password hashing |
| 5 | **FluentValidation** | ✅ Done | `CreateStudentRequestValidator`, `UpdateStudentRequestValidator`, `ValidationActionFilter` |
| 6 | **Multi-Tenancy** | ✅ Done | `TenantId` on all entities, queries filtered by tenant |
| 7 | **Entity Configurations** | ✅ Done | `IEntityTypeConfiguration<T>` for Student, UserAccount, RefreshToken |
| 8 | **Database Seeding** | ✅ Done | Admin user seeded via `UserAccountConfiguration.HasData(...)` |
| 9 | **Serilog** | ❌ Not done | Uses built-in `ILogger` only — Serilog not integrated |
| 10 | **OWASP Security** | ⚠️ Partial | HTTPS redirect configured; no CORS, security headers, or rate limiting |

### Task 1 — Sub-features status

| Sub-feature | Status | Notes |
|-------------|--------|-------|
| Clean Architecture layers | ✅ | Domain, Application, Infrastructure, Presentation |
| SQL Database (EF Core) | ✅ | SQL Server 2022 via Docker |
| Repository Pattern | ✅ | `IStudentRepository` → `StudentRepository` |
| DTO Mapping | ✅ | Custom extension method (`StudentMappings.ToDto()`) |
| Transaction Management | ⚠️ | Single `SaveChangesAsync` per operation (implicit transaction) |
| FluentValidation | ✅ | See Task 5 |
| Redis Cache | ✅ | Dual-cache pattern (by-id + list per tenant) |
| Azure Functions | ❌ | Not required per spec |
| Blob Storage | ❌ | Not required per spec |
| Azure Service Bus | ❌ | Not implemented |
| SignalR | ❌ | Not implemented |
| Webhooks | ❌ | Not implemented |

---

## Tutorial Documents

| # | Document | What You Will Learn |
|---|----------|---------------------|
| 01 | [Run & Dependencies](01-Run-And-Dependencies.md) | How to clone, configure, and run the project with Docker and locally |
| 02 | [Presentation Layer](02-Presentation-Layer.md) | Controllers, middleware, filters, Program.cs composition root |
| 03 | [Application Layer](03-Application-Layer.md) | Services, DTOs, validators, interfaces, mapping, exceptions |
| 04 | [Infrastructure Layer](04-Infrastructure-Layer.md) | EF Core, repositories, caching, DI registration, migrations |
| 05 | [Domain Layer](05-Domain-Layer.md) | Entities, records, domain design |
| 06 | [Auth & Postman](06-Auth-And-Postman.md) | JWT flow, refresh tokens, password hashing, Postman testing |
| 07 | [Redis Cache vs Database](07-Redis-Cache-vs-Database.md) | Cache-first strategy, key design, invalidation, NoOp fallback |

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│                  Presentation Layer                 
│  Controllers · Middleware · Filters · Auth · Config │
├─────────────────────────────────────────────────────┤
│                  Application Layer                   │
│  Services · DTOs · Validators · Interfaces · Maps    │
├─────────────────────────────────────────────────────┤
│                 Infrastructure Layer                 │
│  EF Core · Repositories · Caching · DI · Migrations │
├─────────────────────────────────────────────────────┤
│                    Domain Layer                      │
│           Entities (Student, UserAccount,             │
│                   RefreshToken)                       │
└─────────────────────────────────────────────────────┘
```

**Dependency Rule**: Each layer only depends on the layer below it. Domain has zero dependencies. Application depends on Domain. Infrastructure depends on Application + Domain. Presentation depends on all layers and wires them together.

---

## Technology Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 | Runtime & SDK |
| Entity Framework Core | 10.0.5 | ORM + Migrations |
| SQL Server | 2022 | Primary database |
| Redis | 7-alpine | Distributed cache |
| FluentValidation | 12.0.0 | Request validation |
| JWT Bearer | 10.0.5 | Token authentication |
| Docker Compose | — | Local dev infrastructure |
| xUnit | 2.9.3 | Test framework (scaffolded) |
