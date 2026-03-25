# Plan de implementación .NET

## Estado actual (actualizado)

### Completado

1. Phase 1 - Estructura base
- Solution creada: StudentApi.slnx
- Proyectos creados: Domain, Application, Infrastructure, Presentation, UnitTests, IntegrationTests
- Referencias entre capas configuradas
- Compilación base validada

2. Phase 2 - Domain (versión minimalista)
- Entidad Student creada como record en src/StudentApi.Domain/Entities/Student.cs
- Modelo actual: Id, TenantId, Name, DateOfBirth
- Enfoque aplicado: entidad mínima, sin sobrecargar reglas en Domain por ahora

3. Phase 3 - Application (parcial)
- Contratos y modelos base creados:
	- Interfaces/IStudentRepository.cs
	- Students/StudentDto.cs
	- Students/CreateStudentRequest.cs
	- Students/UpdateStudentRequest.cs
	- Mappings/StudentMappings.cs

## Pendiente para completar el proyecto

1. Finalizar Phase 3 - Application
- Implementar casos de uso/servicio CRUD de Student
- Definir resultados y manejo de errores de aplicación

2. Phase 4 - Infrastructure
- Crear ApplicationDbContext con EF Core
- Implementar IStudentRepository con EF Core y filtro por TenantId
- Configurar IEntityTypeConfiguration para Student
- Añadir migraciones y transacciones
- Añadir seeding inicial sin duplicados

3. Phase 5 - Presentation
- Crear endpoints CRUD de Student
- Conectar DI de servicios y repositorios

4. Fases transversales
- Validaciones con FluentValidation
- Middleware global de excepciones
- ApiResponse wrapper genérico
- JWT y protección de endpoints
- Multi-tenancy end-to-end (middleware + aislamiento en consultas)
- Logging estructurado con Serilog
- OWASP basics: CORS y security headers

5. Integraciones reales
- Redis cache
- Azure Service Bus
- SignalR
- Webhook en al menos una acción

## Siguiente paso inmediato

Implementar Phase 4 (Infrastructure) método por método para IStudentRepository usando EF Core y multi-tenancy.