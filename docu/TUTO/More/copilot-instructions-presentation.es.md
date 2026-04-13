# Guía de Presentación del Proyecto

Este documento es una copia orientada a presentación de los requisitos de `copilot-instructions.md`, ampliada con:

- qué significa cada punto
- dónde está implementado en el proyecto
- un ejemplo que puedes decir durante la presentación
- cómo funciona la característica de extremo a extremo
- si el requisito está completo, parcial o faltante

## Cómo Presentar el Proyecto

Usa este flujo de alto nivel primero:

1. Una solicitud entra a la API por la capa de Presentación.
2. El controlador delega el caso de uso a la capa de Aplicación.
3. La capa de Aplicación contiene el flujo de negocio y la orquestación.
4. La capa de Infraestructura maneja EF Core, Redis, SQL Server y Azure Service Bus.
5. La capa de Dominio contiene el modelo central de entidades.

Ejemplo de ruta concreta de solicitud:

- `POST /api/students` llega a [src/Presentation/Controllers/StudentsController.cs](src/Presentation/Controllers/StudentsController.cs)
- el controlador llama a [src/Application/Students/Services/StudentService.cs](src/Application/Students/Services/StudentService.cs)
- el servicio escribe mediante [src/Infrastructure/Repositories/StudentRepository.cs](src/Infrastructure/Repositories/StudentRepository.cs)
- después de guardar, el servicio actualiza caché mediante [src/Infrastructure/Caching/RedisStudentCacheService.cs](src/Infrastructure/Caching/RedisStudentCacheService.cs)
- el servicio también publica un evento de integración mediante [src/Infrastructure/Messaging/AzureServiceBusStudentEventPublisher.cs](src/Infrastructure/Messaging/AzureServiceBusStudentEventPublisher.cs)

## Tarea 1

### Título

Construir una API RESTful CRUD con Clean Architecture

### Descripción

Usando el esquema:

`Student (id, name, dateOfBirth)`

Crear una API RESTful totalmente funcional en .NET que siga los principios de Clean Architecture.

### Criterios de Aceptación

#### La solución está estructurada en Domain, Application, Infrastructure, Presentation

Estado: Completo

Dónde está en el proyecto:

- [src/Domain](src/Domain)
- [src/Application](src/Application)
- [src/Infrastructure](src/Infrastructure)
- [src/Presentation](src/Presentation)

Cómo funciona:

- Domain contiene la entidad de negocio central, por ejemplo [src/Domain/Entities/Student.cs](src/Domain/Entities/Student.cs).
- Application contiene casos de uso, DTOs, validadores, mapeos y contratos de servicios.
- Infrastructure contiene implementaciones técnicas como EF Core, repositorios, caché Redis y Azure Service Bus.
- Presentation contiene controladores, middleware, configuración de autenticación y pipeline de arranque de la API.

Qué decir en la presentación:

- El proyecto está separado por responsabilidad, no por atajos técnicos.
- Los controladores no hablan directamente con EF Core.
- El flujo de negocio vive en la capa Application, mientras que las preocupaciones externas viven en Infrastructure.

#### Existen endpoints CRUD

Estado: Completo

Dónde está en el proyecto:

- [src/Presentation/Controllers/StudentsController.cs](src/Presentation/Controllers/StudentsController.cs)

Endpoints para mencionar:

- `GET /api/students?tenantId={tenantId}`
- `GET /api/students/{id}?tenantId={tenantId}`
- `POST /api/students`
- `PUT /api/students/{id}?tenantId={tenantId}`
- `DELETE /api/students/{id}?tenantId={tenantId}`

Cómo funciona:

- El controlador recibe la solicitud HTTP.
- Delega el trabajo a `IStudentService`.
- El servicio aplica el flujo del caso de uso.
- El repositorio ejecuta la operación SQL mediante EF Core.
- Las respuestas se devuelven usando el wrapper genérico `ApiResponse<T>`.

Ejemplos que puedes explicar:

- `GetAll` lee todos los estudiantes de un tenant y devuelve `ApiResponse<IReadOnlyList<StudentDto>>`.
- `Create` recibe un `CreateStudentRequest`, guarda un nuevo estudiante, invalida la caché de lista por tenant, almacena la entrada individual en caché y publica un evento `student.created`.
- `Delete` primero verifica que el estudiante exista en el alcance del tenant y luego lo elimina.

Archivos de soporte:

- [src/Application/Students/Interfaces/IStudentService.cs](src/Application/Students/Interfaces/IStudentService.cs)
- [src/Application/Students/Services/StudentService.cs](src/Application/Students/Services/StudentService.cs)
- [src/Infrastructure/Repositories/StudentRepository.cs](src/Infrastructure/Repositories/StudentRepository.cs)

#### La base de datos es SQL y está gestionada con migraciones EF Core

Estado: Completo

Dónde está en el proyecto:

- [src/Infrastructure/Persistence/ApplicationDbContext.cs](src/Infrastructure/Persistence/ApplicationDbContext.cs)
- [src/Infrastructure/Persistence/Migrations](src/Infrastructure/Persistence/Migrations)
- [src/Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs](src/Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs)

Cómo funciona:

- El DbContext define `DbSet<Student>`, `DbSet<UserAccount>` y `DbSet<RefreshToken>`.
- Infrastructure registra SQL Server con `UseSqlServer(connectionString)`.
- Los cambios de esquema se versionan con migraciones.

Ejemplos concretos:

- [src/Infrastructure/Persistence/Migrations/20260326111013_InitialCreate.cs](src/Infrastructure/Persistence/Migrations/20260326111013_InitialCreate.cs) crea la tabla `Students`.
- Migraciones posteriores agregan tablas relacionadas con autenticación.

Qué decir en la presentación:

- La persistencia no es SQL escrito a mano.
- EF Core da un proceso controlado de evolución del esquema mediante migraciones.

#### Se usan DTOs

Estado: Completo

Dónde está en el proyecto:

- [src/Application/Students/DTOs](src/Application/Students/DTOs)
- [src/Application/Students/Mappings/StudentMappings.cs](src/Application/Students/Mappings/StudentMappings.cs)
- [src/Application/Students/CreateStudentRequest.cs](src/Application/Students/CreateStudentRequest.cs)
- [src/Application/Students/UpdateStudentRequest.cs](src/Application/Students/UpdateStudentRequest.cs)

Cómo funciona:

- La API no expone directamente la entidad de dominio.
- El controlador acepta records de request y devuelve records DTO.
- El mapeo se hace en la capa Application con la extensión `ToDto()`.

Ejemplo concreto:

- [src/Domain/Entities/Student.cs](src/Domain/Entities/Student.cs) es el modelo de dominio.
- Se convierte a `StudentDto` mediante [src/Application/Students/Mappings/StudentMappings.cs](src/Application/Students/Mappings/StudentMappings.cs).

Qué decir en la presentación:

- Los DTOs protegen el contrato de la API frente a cambios del modelo interno.
- Esto también está alineado con el requisito de usar records cuando sea posible.

#### Está implementado el patrón Repository

Estado: Completo

Dónde está en el proyecto:

- Contrato: [src/Application/Students/Interfaces/IStudentRepository.cs](src/Application/Students/Interfaces/IStudentRepository.cs)
- Implementación: [src/Infrastructure/Repositories/StudentRepository.cs](src/Infrastructure/Repositories/StudentRepository.cs)

Cómo funciona:

- La capa Application depende de una abstracción, no de EF Core directamente.
- La capa Infrastructure implementa esa abstracción con `ApplicationDbContext`.
- Esto mantiene la capa Application testeable y desacoplada de la tecnología de persistencia.

Métodos del repositorio que puedes mencionar:

- `GetByIdAsync`
- `GetAllAsync`
- `AddAsync`
- `UpdateAsync`
- `DeleteAsync`

Detalle importante:

- Las consultas del repositorio son tenant-aware, por lo que cada operación de estudiante está acotada por `TenantId`.

#### Las transacciones se manejan correctamente

Estado: Parcial

Dónde está en el proyecto:

- [src/Infrastructure/Repositories/StudentRepository.cs](src/Infrastructure/Repositories/StudentRepository.cs)

Cómo funciona actualmente:

- Cada escritura del repositorio llama a `SaveChangesAsync`.
- Eso significa que cada create, update o delete se persiste de forma atómica al nivel de la llamada del repositorio.

Nota para presentación:

- Esto es suficiente para operaciones CRUD simples.
- Sin embargo, no hay un Unit of Work explícito ni un límite de transacción personalizado que coordine múltiples escrituras como una transacción de negocio mayor.

Cómo explicarlo con honestidad:

- El soporte transaccional existe mediante las operaciones de guardado de EF Core.
- Un patrón de orquestación transaccional dedicado aún no está implementado.

#### Se demuestra al menos una integración Azure

Estado: Completo

Dónde está en el proyecto:

- Caché Redis: [src/Infrastructure/Caching/RedisStudentCacheService.cs](src/Infrastructure/Caching/RedisStudentCacheService.cs)
- Publicador Service Bus: [src/Infrastructure/Messaging/AzureServiceBusStudentEventPublisher.cs](src/Infrastructure/Messaging/AzureServiceBusStudentEventPublisher.cs)
- Registro DI: [src/Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs](src/Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs)
- Nota de alcance: [docu/tasks/11-AzureServicesScope.md](docu/tasks/11-AzureServicesScope.md)

Cómo funciona Redis:

- `GetByIdAsync` y `GetAllAsync` del servicio intentan primero caché.
- En cache miss, el servicio lee del repositorio.
- El resultado se serializa y se guarda en Redis.
- En create, update o delete, las entradas relevantes de caché se invalidan o refrescan.

Cómo funciona Azure Service Bus:

- Después de create, update o delete, el servicio de Application publica un evento de integración.
- Infrastructure serializa ese evento y envía un mensaje a una cola de Service Bus.
- El subject del mensaje distingue tipos de evento como `student.created`.

Qué decir en la presentación:

- El proyecto demuestra dos integraciones orientadas a Azure: caché y mensajería asíncrona.
- Redis mejora rendimiento de lectura.
- Service Bus prepara la API para integrarse con otros servicios.

#### Está implementado un hub SignalR

Estado: Faltante

Estado actual:

- No existe una clase de hub SignalR.
- No existe configuración `MapHub` en startup.
- No existe uso de `IHubContext` en el flujo de estudiantes.

Cómo presentarlo:

- SignalR era parte del alcance original pero no está implementado en la base de código actual.

#### Se dispara un webhook en al menos una acción

Estado: Faltante

Estado actual:

- No existe un servicio publicador de webhooks.
- No existe callback HTTP saliente después de acciones sobre estudiantes.

Cómo presentarlo:

- La notificación asíncrona externa está parcialmente cubierta con eventos de Azure Service Bus.
- Los webhooks específicamente no están implementados en la versión actual.

## Tarea 2

### Título

Implementar manejo global de errores

#### Existe middleware global de excepciones

Estado: Completo

Dónde está en el proyecto:

- [src/Presentation/Middleware/GlobalExceptionMiddleware.cs](src/Presentation/Middleware/GlobalExceptionMiddleware.cs)
- registro en [src/Presentation/Program.cs](src/Presentation/Program.cs)

Cómo funciona:

- El middleware envuelve el resto del pipeline con un `try/catch`.
- Si una excepción escapa de un controlador o servicio, el middleware la convierte en una respuesta HTTP controlada.

Qué decir en la presentación:

- El manejo de errores está centralizado, así los controladores se mantienen enfocados en acciones de negocio.

#### Respuestas de error estandarizadas

Estado: Completo

Dónde está en el proyecto:

- [src/Presentation/Common/ApiResponse.cs](src/Presentation/Common/ApiResponse.cs)

Cómo funciona:

- Cada respuesta de error sigue la misma estructura: `success`, `data` y `errors`.
- El middleware usa `ApiResponse<object?>.FailureResponse(errors)`.

#### Códigos HTTP correctos

Estado: Completo

Ejemplos:

- `NotFoundException` se convierte en `404 Not Found`
- `FluentValidation.ValidationException` se convierte en `400 Bad Request`
- excepciones desconocidas se convierten en `500 Internal Server Error`

Dónde está implementado:

- [src/Application/Common/Exceptions/NotFoundException.cs](src/Application/Common/Exceptions/NotFoundException.cs)
- [src/Presentation/Middleware/GlobalExceptionMiddleware.cs](src/Presentation/Middleware/GlobalExceptionMiddleware.cs)

#### Los errores se registran en logs

Estado: Completo

Cómo funciona:

- El middleware registra fallos con el nivel de log correcto antes de escribir la respuesta.
- Serilog también captura información a nivel request a lo largo del pipeline.

## Tarea 3

### Título

Introducir un modelo genérico de respuesta API

#### Estructura de respuesta unificada

Estado: Completo

Dónde está en el proyecto:

- [src/Presentation/Common/ApiResponse.cs](src/Presentation/Common/ApiResponse.cs)

Cómo funciona:

- `ApiResponse<T>` es un record con `Success`, `Data` y `Errors`.
- Expone métodos helper `SuccessResponse` y `FailureResponse`.

#### Incluye success, data, errors

Estado: Completo

Qué decir en la presentación:

- Todos los endpoints devuelven el mismo contrato externo.
- Eso hace la API más fácil de consumir y documentar.

#### No se devuelven objetos crudos

Estado: Completo

Ejemplos:

- [src/Presentation/Controllers/StudentsController.cs](src/Presentation/Controllers/StudentsController.cs) envuelve todos los resultados del controlador.
- [src/Presentation/Controllers/AuthController.cs](src/Presentation/Controllers/AuthController.cs) también envuelve respuestas de autenticación.

## Tarea 4

### Título

Implementar autenticación JWT

#### JWT configurado

Estado: Completo

Dónde está en el proyecto:

- [src/Presentation/Program.cs](src/Presentation/Program.cs)
- [src/Presentation/Authentication/JwtOptions.cs](src/Presentation/Authentication/JwtOptions.cs)
- [src/Presentation/Authentication/JwtTokenService.cs](src/Presentation/Authentication/JwtTokenService.cs)

Cómo funciona:

- `Program.cs` configura autenticación JWT Bearer.
- La validación del token revisa issuer, audience, lifetime y signing key.
- La signing key debe tener al menos 32 caracteres.

#### Endpoints protegidos requieren token

Estado: Completo

Dónde está en el proyecto:

- [src/Presentation/Controllers/StudentsController.cs](src/Presentation/Controllers/StudentsController.cs)

Cómo funciona:

- El controlador está decorado con `[Authorize(Policy = "AdminOnly")]`.
- Solo usuarios autenticados con rol `Admin` pueden acceder a gestión de estudiantes.

#### Unauthorized devuelve 401

Estado: Completo

Cómo funciona:

- JWT inválidos o ausentes son rechazados por el middleware de autenticación.
- Login inválido y refresh token inválido también devuelven `401` desde [src/Presentation/Controllers/AuthController.cs](src/Presentation/Controllers/AuthController.cs).

#### El token incluye claims

Estado: Completo

Dónde está en el proyecto:

- [src/Presentation/Authentication/JwtTokenService.cs](src/Presentation/Authentication/JwtTokenService.cs)

Claims para mencionar:

- `sub`
- `unique_name`
- `ClaimTypes.Name`
- `ClaimTypes.Role`

Punto extra para la presentación:

- El proyecto también soporta rotación de refresh token mediante [src/Presentation/Controllers/AuthController.cs](src/Presentation/Controllers/AuthController.cs), lo cual va más allá del criterio mínimo.

## Tarea 5

### Título

Agregar validación de requests con FluentValidation

#### Existen validadores

Estado: Completo

Dónde está en el proyecto:

- [src/Application/Students/Validators/CreateStudentRequestValidator.cs](src/Application/Students/Validators/CreateStudentRequestValidator.cs)
- [src/Application/Students/Validators/UpdateStudentRequestValidator.cs](src/Application/Students/Validators/UpdateStudentRequestValidator.cs)

Reglas de ejemplo:

- `TenantId` no debe estar vacío en create.
- `Name` no debe estar vacío y debe tener máximo 200 caracteres.
- `DateOfBirth` debe estar presente y ser en el pasado.

#### Requests inválidos devuelven 400

Estado: Completo

Dónde está en el proyecto:

- [src/Presentation/Filters/ValidationActionFilter.cs](src/Presentation/Filters/ValidationActionFilter.cs)
- configuración en [src/Presentation/Program.cs](src/Presentation/Program.cs)

Cómo funciona:

- El action filter inspecciona los argumentos de la acción.
- Resuelve validadores desde DI.
- Si la validación falla, detiene la request y devuelve `400` con el wrapper estándar.

#### No hay validación en controladores

Estado: Completo

Qué decir en la presentación:

- Los controladores se mantienen delgados.
- La validación se mueve al pipeline, lo cual es más limpio y mantenible.

## Tarea 6

### Título

Implementar multi-tenancy con aislamiento por tenant

#### TenantId en todas las entidades

Estado: Parcial

Dónde está en el proyecto:

- [src/Domain/Entities/Student.cs](src/Domain/Entities/Student.cs)

Cómo funciona:

- La entidad `Student` incluye `TenantId` y cada consulta de estudiantes se acota por ese valor.

Limitación importante:

- El concepto tenant está claro para datos de estudiantes.
- Esto no está implementado como abstracción compartida de multi-tenancy en todas las entidades del sistema.

#### Middleware extrae TenantId

Estado: Faltante

Estado actual:

- `tenantId` se pasa manualmente por query string o request body.
- No hay middleware que extraiga contexto tenant desde headers, claims del token o hostname.

Nota para presentación:

- El aislamiento por tenant existe en el flujo de estudiantes, pero no mediante extracción automática por middleware.

#### Consultas filtradas automáticamente

Estado: Parcial

Dónde está en el proyecto:

- [src/Infrastructure/Repositories/StudentRepository.cs](src/Infrastructure/Repositories/StudentRepository.cs)

Cómo funciona:

- Métodos del repositorio filtran explícitamente por `TenantId`.
- Ejemplo: `Where(s => s.TenantId == tenantId)`.

Limitación importante:

- No existe filtro global de consulta en EF Core.
- El aislamiento depende de pasar el tenant correctamente al método del repositorio.

#### Aislamiento verificado

Estado: Parcial

Cómo presentarlo:

- El diseño del repositorio fuerza lecturas y escrituras tenant-aware para estudiantes.
- El aislamiento automático completo no está terminado porque faltan middleware y filtros globales.

## Tarea 7

### Título

Configurar restricciones de entidad con IEntityTypeConfiguration

#### Existen clases de configuración

Estado: Completo

Dónde está en el proyecto:

- [src/Infrastructure/Configurations/StudentConfiguration.cs](src/Infrastructure/Configurations/StudentConfiguration.cs)
- [src/Infrastructure/Configurations/UserAccountConfiguration.cs](src/Infrastructure/Configurations/UserAccountConfiguration.cs)
- [src/Infrastructure/Configurations/RefreshTokenConfiguration.cs](src/Infrastructure/Configurations/RefreshTokenConfiguration.cs)

#### Restricciones aplicadas

Estado: Completo

Ejemplos concretos:

- `Student.Name` es requerido y limitado a 200 caracteres.
- `Student.DateOfBirth` se almacena como `date` en SQL.
- `Student` tiene índice en `(TenantId, Name)`.
- `UserAccount.Username` es único.

#### No hay configuración dentro del DbContext

Estado: Completo

Dónde está en el proyecto:

- [src/Infrastructure/Persistence/ApplicationDbContext.cs](src/Infrastructure/Persistence/ApplicationDbContext.cs)

Cómo funciona:

- El DbContext simplemente llama a `ApplyConfigurationsFromAssembly(...)`.
- Eso mantiene la configuración específica de entidad fuera de la clase DbContext.

## Tarea 8

### Título

Implementar seeding de base de datos

#### Existe data semilla

Estado: Completo

Dónde está en el proyecto:

- [src/Infrastructure/Configurations/UserAccountConfiguration.cs](src/Infrastructure/Configurations/UserAccountConfiguration.cs)

Qué se siembra:

- Un usuario inicial `admin` se siembra con `HasData(...)`.

#### Se ejecuta automáticamente

Estado: Parcial

Cómo funciona actualmente:

- La data semilla se aplica mediante migraciones EF Core y configuración del modelo.
- Aparece automáticamente cuando la base se crea o actualiza con migraciones.

Limitación importante:

- No hay un servicio de seeding dedicado en startup que corra al iniciar la app.
- La nota de la tarea pedía seeding en tiempo de arranque, y esa parte no está implementada como proceso runtime separado.

#### Sin duplicados

Estado: Parcial

Cómo funciona:

- El seeding basado en migraciones es determinista porque usa datos semilla fijos.
- No hay un flujo de seeding runtime idempotente personalizado para datos iniciales más amplios.

## Tarea 9

### Título

Agregar logging estructurado con Serilog

#### Logging configurado

Estado: Completo

Dónde está en el proyecto:

- [src/Presentation/Program.cs](src/Presentation/Program.cs)

Cómo funciona:

- La aplicación crea un bootstrap logger antes de construir el host.
- El host luego se configura con `UseSerilog(...)`.
- Los logs se enriquecen con metadata de aplicación y entorno.

#### Incluye logs de request y errores

Estado: Completo

Cómo funciona:

- `UseSerilogRequestLogging(...)` registra requests HTTP.
- `GlobalExceptionMiddleware` registra errores.
- El enriquecimiento de request logging agrega host, scheme, trace id y tenant id cuando está disponible.

#### Logs estructurados

Estado: Completo

Ejemplos que puedes mencionar:

- Plantilla de mensaje de request: `HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms`
- Operaciones de caché Redis registran claves como `REDIS HIT`, `REDIS MISS` y `REDIS DEL` en [src/Infrastructure/Caching/RedisStudentCacheService.cs](src/Infrastructure/Caching/RedisStudentCacheService.cs)
- Azure Service Bus registra `SERVICE BUS SEND {Subject} student:{StudentId} tenant:{TenantId}` en [src/Infrastructure/Messaging/AzureServiceBusStudentEventPublisher.cs](src/Infrastructure/Messaging/AzureServiceBusStudentEventPublisher.cs)

## Tarea 10

### Título

Aplicar mejores prácticas OWASP de seguridad

#### Headers de seguridad configurados

Estado: Faltante

Estado actual:

- No hay middleware personalizado para headers como `X-Frame-Options`, `X-Content-Type-Options` o `Content-Security-Policy`.

#### CORS restringido

Estado: Faltante

Estado actual:

- No hay configuración `AddCors(...)` ni `UseCors(...)` en startup.

#### Verificado por inspección

Estado: Faltante

Cómo presentarlo:

- Hay básicos de seguridad como autenticación JWT y redirección HTTPS.
- Las tareas específicas de endurecimiento OWASP en headers de respuesta y CORS siguen pendientes.

## Resumen de Cierre para la Presentación

### Puntos fuertes ya implementados

- La separación Clean Architecture es clara y fácil de explicar.
- El CRUD de estudiantes está completo.
- La persistencia EF Core y migraciones están en su lugar.
- DTOs, validación, autenticación, respuestas genéricas y manejo global de excepciones están implementados.
- Caché Redis y mensajería Azure Service Bus están integradas.
- Logging estructurado con Serilog está configurado.

### Brechas que debes mencionar con honestidad

- SignalR no está implementado.
- Webhooks no están implementados.
- Multi-tenancy se aplica manualmente, no por middleware y filtros globales.
- La orquestación transaccional es básica, no explícita.
- Seeding en startup es parcial.
- Headers OWASP de seguridad y endurecimiento CORS siguen pendientes.

### Guion simple de presentación

Puedes presentar el proyecto en este orden:

1. Explica las cuatro capas de Clean Architecture.
2. Muestra `StudentsController` como punto de entrada.
3. Muestra `StudentService` como orquestador de negocio.
4. Muestra `StudentRepository` y `ApplicationDbContext` para persistencia.
5. Muestra `ApiResponse<T>`, validation filter y exception middleware como features transversales de calidad.
6. Muestra autenticación JWT y la policy `AdminOnly`.
7. Muestra caché Redis y Azure Service Bus como integraciones de mundo real.
8. Cierra con los pendientes: SignalR, webhooks, automatización tenant más fuerte y endurecimiento OWASP.
