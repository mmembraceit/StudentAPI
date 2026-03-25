API RESTFUL CRUD

Arquitectura: Clean Architecture con cuatro capas principales
Clean architecture no es solo una estructura de carpetas, es control de dependencias.
EF Core, JWT, middleware y validación son herramientas, no el diseño en sí.

1. DOMAIN: entidades, reglas de negocio puras, value objects y contratos mínimos del core.
2. APPLICATION: casos de uso, DTOs, validaciones, mapping y orquestación de reglas.
3. INFRAESTRUCTURA: EF Core, persistencia SQL, repositorios, configuraciones, migraciones, transacciones, caché e integraciones externas.
4. PRESENTACIÓN: controladores, middleware, autenticación, SignalR, configuración HTTP y composición de la app.

* CQRS puede instroducirse en la app.

BUENAS PRÁCTICAS:
- Separar primero arquitectura y responsabilidades antes de tocar detalles del framework.
- Introducir complejidad de forma incremental.
- Hacer visibles los requisitos transversales desde el roadmap, no descuibrirlos más tarde.
- Mantener DTOs, validaciones, errores y seguridad como preocupaciones explícitas.
- Usar records donde aporten valor: request, responses, DTOs y modelos inmutables.
- Evitar que infraestructura dicte reglas a Domain o Application.
- Diseñar pensando en mantenibilidad, no solo en que compile.

¿Cómo se organiza una solución .NET profesional y qué responsabilidad tiene cada capa?

La meta de este paso es poder respondes a las siguientes preguntas con claridad:
1. Qué es una solución en .NET.
2. Qué es un Project.
3. Por qué separamos Domain, Application, Insfrastructure y Presentation.
4. Qué dependencias están permitidas y cuáles no
5. Cómo pensar esta API como si estuviera en un equipo real.

DOMAIN: contiene el corazón del negocio. Aquí conviven:
	- Entidades.
	- Reglas del dominio.
	- Value objects si aplican.
	- Contratos o abstracciones del core, solo si tienen sentido.
	
APPLICATION: Contiene casos de uso. Aquí nos encontramos:
	- DTOs
	- Interfaces que infraestructure implementará.
	- Commands y Queries si usamos una aproximación tipo CQRS ligera.
	- Validaciones.
	- Mapping.
	- Orquestación del flujo de negocio.

INFRASTRUCTURE: Contiene detalles técnicos. Aquí tenemos:
	- EF Core.
	- DbContext.
	- Repositorios.
	- Configuración de entidades.
	- Migraciones.
	- Redis.
	- Azure Service Bus.
	- Implementaciones concretas de servicios externos.

PRESENTATION: Es la puerta de entrada. Aquí viven:
	- Controllers.
	- Middleware.
	- Configuración de autenticación JWT.
	- SignalR Hubs.
	- Wiring del contenedor DI.
	- Configuración HTTP.

---------------------------------------------------------------- 

ESTRUCTURA OBJETIVO

StudentApi/
├─ StudentApi.sln
├─ src/
│  ├─ StudentApi.Domain/
│  ├─ StudentApi.Application/
│  ├─ StudentApi.Infrastructure/
│  └─ StudentApi.Presentation/
└─ tests/
   ├─ StudentApi.UnitTests/
   └─ StudentApi.IntegrationTests/
   
	ESTRUCTURA GENERAL
	* StudentApi.sln : es la solución principal que agrupa a todos los proyectos.
	* src/ : contiene código productivo.
	* test/ : contiene pruebas. Es bueno diseñar la estructura pensando en ellas desde el principio.
	
	CAPAS
	* StudentApi.Domain/ : es el proyecto más estable y más protegido del framework.
	* StudentApi.Aplication/ : define lo que la aplicación quiere hacer.
	* StudentApi.Infrastructure/ : resuelve cómo se implementan los detalles técnicos
	* StudentApi.Presentation/ : expone endpoints HTTP y compone la aplicación.

----------------------------------------------------------------   

DIRECCIÓN DE DEPENDENCIAS (RELACIONES)

Presentation -> Application
Infrastructure -> Application
Application -> Domain

	* Presentation necesita invocar casos de uso definidos en Application.
	* Infrastructure necesita implementar interfaces definidar en Application.
	* Application necesita conocer el modelo central del dominio.
	* Domain no debe depender de nadie porque es el núcleo.

Presentation ───────┐
                    │
Infrastructure ─────┼──► Application ───► Domain
                    │
External Services ──┘

---------------------------------------------------------------- 

REFERENCIAS ESPERADAS ENTRE PROYECTOS

StudentApi.Domain
- No referencia a ningún proyecto de la solución

StudentApi.Application
- Referencia a StudentApi.Domain

StudentApi.Infrastructure
- Referencia a StudentApi.Application
- Referencia a StudentApi.Domain

StudentApi.Presentation
- Referencia a StudentApi.Application
- Referencia a StudentApi.Infrastructure


----------------------------------------------------------------

 ┌─────────────────────────────────────┐
 │                                     │
 │        Presentation Layer           │
 │                                     │
 │  ┌─────────────────────────────┐    │
 │  │     Application Layer       │    │
 │  │                             │    │
 │  │   ┌─────────────────────┐   │    │
 │  │   │    Domain Layer     │   │    │
 │  │   └─────────────────────┘   │    │
 │  │                             │    │
 │  └─────────────────────────────┘    │
 │                                     │
 │        Infrastructure Layer         │
 │                                     │
 │                                     │
 └─────────────────────────────────────┘
 
 ----------------------------------------------------------------
 
EJEMPLO DE RESPONSABILIDADES

 
// Ejemplo de Student en DOMAIN.
// La entidad pertenece al negocio, no al controlador ni al DbContext.
// [private set] protege consistencia y evita mutaciones aritrarias desde cualquier parte.
// [Guid] es una opción razonable para identificadores de APIs distribuidas.
// [DateOnly] expresa mejor intención que [DateTime] cuando solo importa la fecha.

┌────────────────────────────────────────────────────────┐
│              Student                                   │
├────────────────────────────────────────────────────────┤
│ public class Student                                   │
│ {                                                      │
│     public Guid Id { get; private set; }               │
│     public string Name { get; private set; }           │
│     public DateOnly DateOfBirth { get; private set; }  │
│ }                                                      │
└────────────────────────────────────────────────────────┘

// Ejemplo de request DTO en Application
// Un DTO no es una entidad.
// [redord] encaja bien para request y responses porque suelen ser datos inmutables.

┌───────────────────────────────────────────────────────────────────────┐
│            CreateStudentRequest                                       │
├───────────────────────────────────────────────────────────────────────┤
│                                                                       │
│ public record CreateStudentRequest(string Name, DateOnly DateOfBirth);│
│                                                                       │
└───────────────────────────────────────────────────────────────────────┘

// Ejemplo de ApplicationDbContext en Infrastructure
// EF Core vive en Infraestructure, no en Domain.
// La persistencia es un detalle técnico, no el negocio

┌────────────────────────────────────────────────────────┐
│            ApplicationDbContext                        │
├────────────────────────────────────────────────────────┤
│ public class ApplicationDbContext : DbContext          │
│ {                                                      │
│ }                                                      │
└────────────────────────────────────────────────────────┘

// Ejemplo de controller en Presentation
// El controlador recibe HTTP.
// No debería contener reglas de negocio complejas
// Su trabajo ideal es delegar a Application
┌────────────────────────────────────────────────────────┐
│            ApiController                               │
├────────────────────────────────────────────────────────┤
│ [ApiController]                                        │
│ [Route("api/[controller]")]                            │
│ public class StudentsController : ControllerBase       │
│ {                                                      │
│ }                                                      │
│                                                        │
└────────────────────────────────────────────────────────┘                                                      


 ----------------------------------------------------------------

BUENAS PRÁCTICAS
- Mantener Domain libre de dependencias de framework.
- Separar entidades del dominio y DTOs.
- Usar nombres explícitos y consistentes desde el inicio.
- Diseñar dependencias en una sola dirección 
- Reservar infraestructure para detalles técnicos reales
- Mantener controladores delegados.
- Crear la estructura pensando en evolución , tresting y mantenimiento.
- Usar records para modelos de transporte cuando no necesiten comportamiento complejo.

¿QUÉ DEBO APRENDER?
- Una solution no es lo mismo que un Project.
- Clean Architecture trata de dependencias y responsabilidades, no solo de carpetas.
- Domain debe ser la parte más estable del sistema.
- Application modela casos de uso, no detalles técnicos.
- Infrastructure implementa tecnología concreta sin contaminar el core.
- Presentation expone la API, pero no debería contener lóigica del negocio.
- DTOs y entidades cumplen roles distintos.
- La dirección de dependencias es una decisión arquitectónica crítica.
