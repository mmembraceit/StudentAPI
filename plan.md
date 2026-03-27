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
	- Interfaces/IStudentService.cs
	- Students/StudentService.cs

4. Phase 4 - Infrastructure (base completada)
- ApplicationDbContext implementado
- Configuración de Student con IEntityTypeConfiguration
- Repositorio EF Core con filtro por TenantId implementado
- Registro de DI en InfrastructureServiceCollectionExtensions
- Migración inicial creada en Persistence/Migrations
- Nota de entorno: apply de migración bloqueado por ausencia de LocalDB en la máquina actual

## Pendiente para completar el proyecto

1. Cerrar Phase 4 - Infrastructure
- Aplicar migración inicial en una instancia SQL disponible (LocalDB/SQL Express)
- Añadir estrategia de transacciones para operaciones compuestas
- Añadir seeding inicial sin duplicados

2. Phase 5 - Presentation
- Crear endpoints CRUD de Student
- Conectar DI de servicios y repositorios

3. Fases transversales
- Validaciones con FluentValidation
- Middleware global de excepciones
- ApiResponse wrapper genérico
- JWT y protección de endpoints
- Multi-tenancy end-to-end (middleware + aislamiento en consultas)
- Logging estructurado con Serilog
- OWASP basics: CORS y security headers

4. Integraciones reales
- Redis cache
- Azure Service Bus
- SignalR
- Webhook en al menos una acción

## Siguiente paso inmediato

Completar base SQL local (instalar/configurar LocalDB o cambiar a instancia SQL Express) y luego pasar a Phase 5 con StudentsController CRUD.