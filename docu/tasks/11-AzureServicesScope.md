# Azure Services Scope

## Final Decision

Implementation scope for Azure-related integrations is:

- Redis Cache
- Service Bus

The following Azure items are out of scope for this project iteration:

- Azure Functions
- Blob Storage

## Current State

- Redis Cache: Implemented
- Service Bus: Pending

## Redis Implementation Evidence

- src/Infrastructure/Caching/RedisStudentCacheService.cs
- src/Infrastructure/Caching/NoOpStudentCacheService.cs
- src/Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs
- src/Application/Interfaces/IStudentCacheService.cs
- src/Application/Students/Services/StudentService.cs
- src/Presentation/appsettings.Development.json
- docker-compose.yml

## Service Bus Planned Scope

- Publish an integration message after at least one student write operation.
- Recommended first event:
  - StudentCreated
- Recommended implementation points:
  - Application event contract in Application layer
  - Service Bus publisher in Infrastructure layer
  - Invocation from StudentService write use case

## Acceptance Alignment

This scope still satisfies the original requirement to demonstrate Azure integration, with Redis already implemented and Service Bus planned as the second Azure service.
