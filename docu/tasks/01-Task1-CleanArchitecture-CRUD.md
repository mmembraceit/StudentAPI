# Task 1: Build a RESTful CRUD API with Clean Architecture

## Status

Partial

## What Is Implemented

- Clean Architecture structure exists in layer folders under src:
  - src/Domain
  - src/Application
  - src/Infrastructure
  - src/Presentation
- Full Student CRUD endpoints implemented in StudentsController.
- SQL persistence implemented with EF Core and SQL Server.
- Repository pattern implemented.
- DTO mapping implemented via custom mapping extensions.
- FluentValidation integrated in request pipeline.
- Azure-related integration demonstrated with Redis cache.

## Azure Services Scope (Final)

- In scope:
  - Redis Cache
  - Service Bus
- Out of scope for this iteration:
  - Azure Functions
  - Blob Storage

Reference: [11-AzureServicesScope.md](11-AzureServicesScope.md)

## Evidence

- src/Presentation/Controllers/StudentsController.cs
- src/Application/Students/Interfaces/IStudentService.cs
- src/Application/Students/Interfaces/IStudentRepository.cs
- src/Application/Students/Mappings/StudentMappings.cs
- src/Infrastructure/Repositories/StudentRepository.cs
- src/Infrastructure/Persistence/ApplicationDbContext.cs
- src/Infrastructure/Persistence/Migrations
- src/Infrastructure/Caching/RedisStudentCacheService.cs
- docker-compose.yml

## Remaining Gaps Against Acceptance Criteria

- Transaction management is not explicitly implemented/documented as a dedicated unit-of-work transaction strategy.
- SignalR hub not implemented.
- Webhook trigger not implemented.
- Azure Service Bus integration not implemented.

## Suggested Next Steps

1. Add transactional boundary strategy for write use cases.
2. Add SignalR hub and publish events on create/update/delete.
3. Add webhook publisher for at least one student action.
4. Add Service Bus producer for async integration.
