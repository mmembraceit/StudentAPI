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
├─ StudentApi.slnx
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

----------------------------------------------------------------------------------------   

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

------------------------------------------------------------------------------------- 

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


-------------------------------------------------------------------------------------

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


 ----------------------------------------------------------------
 
Ejercicio opcional: Antes de seguir, intenta responder mentalmente estas tres preguntas:

Por qué DbContext no debería vivir en Domain
Por qué un CreateStudentRequest no debería ser la misma clase que Student
Qué problema aparece si Presentation empieza a depender directamente de EF Core

 ----------------------------------------------------------------
 
 CREACIÓN FÍSICA DE LA SOLUTION
 
 1º - Comprobar el SDK disponible --> [dotnet --version]
 2º - De no estar instalado, instalar el SDK y runtime. SDK para desarrollar, Runtime para ejecutar.
 3º - Materializar la arquitectura que antes era solo diseño:
 
	* CREAR SOLUTION PRINCIPAL.NET:
	
			StudentAPI.slnx
			
	
	* CREAR PROYECTOS BASE y PROYECTOS DE PRUEBA
	
			StudentApi.Domain
			StudentApi.Application
			StudentApi.Infrastructure
			StudentApi.Presentation
			
			StudentApi.UnitTest
			StudentApi.IntegrationTest
		
	
	* AGREGAR REFERENCIAS ENTRE CAPAS
	
	
	* VERIFICAR COMPILACIÓN INICIAL
----------------------------------------------------------------------------------------------------------------------------------------	
CODE:
Estos fueron los comandos clave usados:	

// Comprobación de que el SDK esté disponible

dotnet --version

// Crea el archivo de solución. Se generó [StudentApi.slnx]

dotnet new sln -n StudentApi

// Crea biblioteca de clases. Ideal para aplicaciones que no son ejecutables.

dotnet new classlib -n StudentApi.Domain -o src/StudentApi.Domain
dotnet new classlib -n StudentApi.Application -o src/StudentApi.Application
dotnet new classlib -n StudentApi.Infrastructure -o src/StudentApi.Infrastructure

// Crea el proyecto HHTP ejecutable. Será la capa de presentación

dotnet new webapi -n StudentApi.Presentation -o src/StudentApi.Presentation

// Crea proytectos de pruebas. Uno lo usaremos para unit test y otro para integration tests.

dotnet new xunit -n StudentApi.UnitTests -o tests/StudentApi.UnitTests
dotnet new xunit -n StudentApi.IntegrationTests -o tests/StudentApi.IntegrationTests

// Se definen las dependencias reales entre capas

			// Application referencia Domain

dotnet add src/StudentApi.Application/StudentApi.Application.csproj reference src/StudentApi.Domain/StudentApi.Domain.csproj

			// Infrastructure referencia Application y Domain

dotnet add src/StudentApi.Infrastructure/StudentApi.Infrastructure.csproj reference src/StudentApi.Application/StudentApi.Application.csproj
dotnet add src/StudentApi.Infrastructure/StudentApi.Infrastructure.csproj reference src/StudentApi.Domain/StudentApi.Domain.csproj

			// Presentation referencia Application e Infrastructure

dotnet add src/StudentApi.Presentation/StudentApi.Presentation.csproj reference src/StudentApi.Application/StudentApi.Application.csproj
dotnet add src/StudentApi.Presentation/StudentApi.Presentation.csproj reference src/StudentApi.Infrastructure/StudentApi.Infrastructure.csproj

// Refenrecias de UnitTest a Application y Domain

dotnet add tests/StudentApi.UnitTests/StudentApi.UnitTests.csproj reference src/StudentApi.Application/StudentApi.Application.csproj
dotnet add tests/StudentApi.UnitTests/StudentApi.UnitTests.csproj reference src/StudentApi.Domain/StudentApi.Domain.csproj

// Referencia de IntegratonTest a Presentation			

dotnet add tests/StudentApi.IntegrationTests/StudentApi.IntegrationTests.csproj reference src/StudentApi.Presentation/StudentApi.Presentation.csproj

// Registro de cada proyecto dentro de la solución principal

dotnet sln StudentApi.slnx add src/StudentApi.Domain/StudentApi.Domain.csproj
dotnet sln StudentApi.slnx add src/StudentApi.Application/StudentApi.Application.csproj
dotnet sln StudentApi.slnx add src/StudentApi.Infrastructure/StudentApi.Infrastructure.csproj
dotnet sln StudentApi.slnx add src/StudentApi.Presentation/StudentApi.Presentation.csproj
dotnet sln StudentApi.slnx add tests/StudentApi.UnitTests/StudentApi.UnitTests.csproj
dotnet sln StudentApi.slnx add tests/StudentApi.IntegrationTests/StudentApi.IntegrationTests.csproj

// Verifica que todo restaure y compile

dotnet build StudentApi.slnx
	
-------------------------------------------------------------------------------------------------------------------------------------------------	

BUENAS PRÁCTICAS

- Separar /src y /test desde el principio
- Mantener Domain sin referencias a otros proyectos internos.
- Hacer que Application dependa de Domain, no de Infrastructure.
- Usar Presentation como composition root.
- Verificar compilación inmediatamente despues de crear la estructura.
- Materializar la arquitectura con referencias explícitas
- Diseñar pruebas desde el comienzo, aunque aún estén vacías.

¿QUÉ DEBO APRENDER?

- Una solución .NET agrupa proyectos, pero no reemplaza la arquitectura.
- Las referencias entre csproj son una herramienta de diseño
- Domain debe seguir siendo el núcleo más protegido
- Presentación es la puerta de entrada, no el lugar de la lógica del negocio.
- Infrastructure implementa los detalles técnicos, peno no debe dictar las reglas del core.
- Que una solucion compile vacía ya es una validación importante del diseño base.

EJERCICIO OPCIONAL:
Sin tocar todavía lógica de negocio, intenta responder por tu cuenta estas tres preguntas:

¿Por qué StudentApi.Domain no debe referenciar StudentApi.Infrastructure?
¿Por qué StudentApi.Presentation sí puede referenciar StudentApi.Infrastructure?
¿Qué tipo de código pondrías en Application que nunca pondrías en Domain?

-------------------------------------------------------------------------------------------------------------------------------------------------

LIMPIEZA DE PLANTILLA INICIAL Y DEFINICION DE LA ORGANIZACION INTERNA

- Se quitan archivos de ejemplo que no aportan nada al dominio real. (Class1.cs, UnitTest1.cs o endpoints demo)
- Dejar la API con un arranque neutral y profesional.
- Crear una organización interna coherente por capas.
- Evitar que artefactos de compilación ensucien el repositorio.

Pasamos de una solución "generada por plantilla" o una solución "lista para desarrollo serio".

Cambios realizados:

Eliminados:

	src/StudentApi.Domain/Class1.cs
	src/StudentApi.Application/Class1.cs
	src/StudentApi.Infrastructure/Class1.cs
	tests/StudentApi.UnitTests/UnitTest1.cs
	tests/StudentApi.IntegrationTests/UnitTest1.cs
	src/StudentApi.Presentation/StudentApi.Presentation.http
	
Actualizado:

	Program.cs
	
Añadido:

	.gitignore
	
Carpetas creadas:

	Domain:
		Entities
		Common
		ValueObjects
		
	Application:
		Abstractions
		Students
		DTOs
		Mappings
		Validators
		
	Infrastructure:
		Persistence
		Repositories
		Configurations
		DependencyInjection
		
	Presentation:
		Controllers
		Middleware
		Extensions
		Authentication
		Hubs

Código principal que dejamos en Program.cs

┌────────────────────────────────────────────────────────┐
│            Program.cs                                  │ 
├────────────────────────────────────────────────────────┤
│  var builder = WebApplication.CreateBuilder(args);     │
│                                                        │
│  builder.Services.AddControllers();                    │
│  builder.Services.AddOpenApi();                        │
│                                                        │
│  var app = builder.Build();                            │
│                                                        │
│  if (app.Environment.IsDevelopment())                  │
│  {                                                     │
│    app.MapOpenApi();                                   │
│  }                                                     │
│                                                        │
│  app.UseHttpsRedirection();                            │
│                                                        │
│  app.MapControllers();                                 │
│                                                        │
│  app.Run();                                            │
└────────────────────────────────────────────────────────┘    

// Crea el host y la configuración de ASP.NET Core
// Es el punto de entrada real a la aplicación
var builder = WebApplication.CreateBuilder(args);

// Registra soporte para controladores
// Esto es importante porque la api va a correr con controllers
builder.Services.AddControllers(); 

// Mantiene soporte para documentación OpenAPI
// Viene bien para sigueinte fases, cuando empecemos a exponer endpoints reales
builder.Services.AddOpenApi();

// Construye la aplicación con todos los servicios registrados
var app = builder.Build();

// Expone OpenAPI solo en desarrollo
// Decisión razonble para no abrir superficie innecesaria en otros entornos
if (app.Environment.IsDevelopment())

// Fuerza redirección a HHTPS
// No es toda la seguridad de la app, pero sí una base correcta
app.UseHttpsRedirection();

// Conecta la pipeline HTTP con los controladores
// Esto deja Presentation preparada para recibir un StudentController real en fases posteriores
app.MapControllers();

// Inicia la aplicación
app.Run();

------------------------------------------------------------------------------------------------

BUENAS PRÁCTICAS

- Eliminar placeholders antes de construir negocio encima
- Mantener el arranque de la API lo más simple y explícito posible.
- Preparar Presentation para controllers reales
- Crear carpetas por intención arquitectónica, no por costumbre.
- Conservar solo los archivos generados que sí tien valor estructural.
- Validar con compilación después de cada limpieza importante

OPTIONAL EXERCISE:
Haz este ejercicio mental antes de seguir:

Piensa qué pondrías primero en Entities y por qué no lo pondrías en Application.
Intenta justificar por qué Abstractions existe antes de tener implementaciones concretas.

------------------------------------------------------------------------------------------------

CONSTRUCCIÓN DE LA CAPA DOMAIN

Definir la primera entidad del dominio --> Student.cs


// record: inmutabilidad y muy bueno para modelos simples
// id: identidad única
// Tentid: para multi-tenancy
// Nama: valor por defecto apra evitar null
// DateOnly: representa fecha sin hora, ideal para nacimiento
┌────────────────────────────────────────────────────────┐
│            Student.cs                                  │
├────────────────────────────────────────────────────────┤
│ public record Student                                  │
│ {                                                      │
│    public Guid Id { get; init; }                       │ 
│    public Guid TenantId { get; init; }                 │
│    public string Name { get; init; } = string.Empty;   │
│    public DateOnly DateOfBirth { get; init; }          │  
│ }                                                      │
│                                                        │
└────────────────────────────────────────────────────────┘    

------------------------------------------------------------------------------------------------

CONSTRUCCIÓN DE LA CAPA DE Aplication/

En Clean Architecture, Application define qué necesita el sistema para ejecutar casos de uso:

	- Modelos de entrada/salida
	- Contratos (interfaces) que luego Infrastructure implementa
	- Mapeos entre Domain y DTOs
	
¿Qué problema resuelve?:

	- Evita que la lógica de casos de uso dependa de EF Core o HTTP
	- Permite testear Application con dobles de repositorio
	- Mantiene fronteras limpias entre capas
	
IMPLEMENTATION STEPS:

	- Crear DTO de salida para Student.
	- Crear request de creación y actualización.
	- Definir interfaz de repositorio asíncrona, tenant-aware.
	- Añadir mapeo de Domain Student hacia DTO.
	- Validar compilación completa de la solución.
	
	

// Desacopla lo que expones de cómo es la entidad interna
// Si mañana Domain cambia, no obligas a cambiar clientes externo inmediatamente
// Se incluye el contexto de tenant en la respuesta	
┌────────────────────────────────────────────────────────┐
│            StudentDTO.cs                               │
├────────────────────────────────────────────────────────┤
│	namespace StudentApi.Application.Students;           │
│                                                        │
│	public record StudentDto(                            │
│		Guid Id,                                         │
│		Guid TenantId,                                   │
│		string Name,                                     │
│		DateOnly DateOfBirth);                           │                      
│                                                        │
└────────────────────────────────────────────────────────┘    



// Permite crear explícitamente un tenant
// Nota de arquitectura: cuando implemente middleware de tenant, quizás prefiera no recibir Tenantid por body y tomarlo por contexto.
// Name, DateOfBirth --> datos mínimos de negocio, correcto en fase inicial
┌────────────────────────────────────────────────────────┐
│            CreateStudentRequest.cs                     │
├────────────────────────────────────────────────────────┤
│	namespace StudentApi.Application.Students;           │
│                                                        │
│	public record CreateStudentRequest(                  │
│		Guid TenantId,                                   │                      
│		string Name,                                     │
│		DateOnly DateOfBirth);                           │                      
│                                                        │
└────────────────────────────────────────────────────────┘    


// No incluye ni Id ni TenantId, ya que llegan por ruta o por contexto.
// Buena separación de responsabilidades
┌────────────────────────────────────────────────────────┐
│            UpdateStudentRequest.cs                     │
├────────────────────────────────────────────────────────┤
│	namespace StudentApi.Application.Students;           │
│                                                        │
│	public record UpdateStudentRequest(                  │
│		string Name,                                     │
│		DateOnly DateOfBirth);                           │                      
│                                                        │
└────────────────────────────────────────────────────────┘    



// Application depende de Domain --> Dirección correcta
// Tanto Task como Task<T> representan operaciones asíncronas
// Cada método de la interfaz cubre una tarea concreta del CRUD
// La interfaz no implementa nada, solo define el contrato que infrastructure debe cumplir.
┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                                                 │
│            IStudentRepository.cs                                                                                │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│	using StudentApi.Domain.Entities;                                                                             │
│                                                                                                                 │
│	namespace StudentApi.Application.Interfaces;                                                                  │
│                                                                                                                 │
│	public interface IStudentRepository                                                                           │
│	{                                                                                                             │
│		Task<Student?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);       │
│                                                                                                                 │
│		Task<IReadOnlyList<Student>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);   │
│                                                                                                                 │
│		Task AddAsync(Student student, CancellationToken cancellationToken = default);                            │
│                                                                                                                 │
│		Task UpdateAsync(Student student, CancellationToken cancellationToken = default);                         │
│                                                                                                                 │
│		Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);                  │
│	}				                                                                                              │
│                                                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘ 


// Tarea CRUD: Read individual
// Student?: Si no existe, debe poder devolver null sin lanzar excepcion por flujo normal
// tenantId: fuerza el aislamiento de datos por tenant desde contrato
// CancellationToken permite cancelar la consulta si el requst HTTP se corta o expira
Task<Student?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default); 

// Tarea CRUD: Read list
// IReadOnlyList: usándolo el contrato es má seguro, no se pueden agregar ni quitar elementos.
// Ejemplo sobre IreadOnlyList: 
//			Repositorio trae 100 estudiantes de tenant A.
//			Caso de uso los mapea a DTO
//			Si alguien medio pudiera hace stuedents.clear(), rompería el flujo sin querer.
//			Con IReadOnlyList, eso ya no compila. No hace inmutable cada objketo Student; hace no mutable la colección.			
// Si se devolviera List<Student> el consumidor podría hacer .add(), .Remove(), .Clear(). Podrían ocurrir mutaciones accidentales.
// tenatId: evita lecturas cruzadas entre tentants.
Task<IReadOnlyList<Student>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);

// Tarea CRUD: Create
// Recibe entidad Domain completa
Task AddAsync(Student student, CancellationToken cancellationToken = default);

// Tarea CRUD: Update
// El caso de uso construye el nuevo estado de Student y el repositorio persiste
Task UpdateAsync(Student student, CancellationToken cancellationToken = default);

// Tarea CRUD: Delete
// Id + TenantId: para borrar una entidad específica dentro del tenant correcto !! Evita el antipatrón de borrar solo por id global.
Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default); 

BUENAS PRÁCTICAS

	- Mantener esta interfaz en Application y no en Infrastructure.
	- Mantener firmas tenant-aware desde el principio.
	- Usar async en todos los accesos a persistencia.
	- No filtrar tenancy en controller; hacerlo en casos de uso y repositorio.
	- Dejar que Infrastructure implemente detalles técnicos, no Application.

¿QUÉ DEBO APRENDER?:

	- Esta interfaz define política de acceso, no tecnología.
	- Task aquí no es decoración, es una decisión de rendimiento y escalabilidad.
	- tenantId en firmas es una barrera de seguridad de diseño.
	- CRUD no es solo operaciones, también reglas de aislamiento y contrato.

---------------------------------------------------------------------------------------------------------------------------------------	
			
CREAR IStudentService y StudentService con CRUD

Se definen los casos de uso CRUD de Student en Application para tener la lógica de aplicación completa antes de entrar en la imlementación técnica con EF Core en Infrastructure.
Se va a crear una capa de orquestación en Application.
Aquí se usan interfaces, DTOs y apeos para desacoplar la lógica del detalle de la persistencia.

IStudentService define qué operaciones ofrece la aplicación.
StudentService implementa esas operaciones usando IStudentRepository.
Infrastructure luego se encargará del cómo real de guardar/consultar los datos.

IMPLEMENTATION STEPS:

Crear contrato de servicio:
IStudentService.cs

┌──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                                                                              │
│           IStudentService.cs                                                                                                                 │
├──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│	using StudentApi.Application.Students;                                                                                                     │
│                                                                                                                                              │
│	namespace StudentApi.Application.Interfaces;                                                                                               │
│                                                                                                                                              │
│	public interface IStudentService                                                                                                           │
│	{                                                                                                                                          │
│		Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);                                 │
│                                                                                                                                              │
│		Task<IReadOnlyList<StudentDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);	                           │
│                                                                                                                                              │
│		Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);                             │
│                                                                                                                                              │
│		Task<StudentDto> UpdateAsync(Guid id, Guid tenantId, UpdateStudentRequest request, CancellationToken cancellationToken = default);     │
│                                                                                                                                              │
│		Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);                                               │
│	}                                                                                                                                          │
│		                                                                                                                                       │
│                                                                                                                                              │
└──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘ 

// Tarea de negocio: obtener un Student por id dentro de un tenant.
// Devuelve nullable porque un recurso puede no existir. 
Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

// Tarea de negocio: listar Students de un tenant
// Devuelve IReeadOnlyList de DTO para solo lectura
Task<IReadOnlyList<StudentDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);

// Tarea de negocio: crear un Student desde un request de entrada.
// >Devuelve DTO del recurso creado
Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);

// Tarea de negocio: actualizar datos editables de un Student existente.
// Usa id y tenantid para mantener aislamiento multi-tenant.
Task<StudentDto> UpdateAsync(Guid id, Guid tenantId, UpdateStudentRequest request, CancellationToken cancellationToken = default);

// Tarea de negocio: elmininar un Student por id y tenantId
// No delvuelve objeto, solo ejecuta acción.
Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);


┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                                                                                             │
│           StudentService.cs                                                                                                                                 │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│	using StudentApi.Application.Interfaces;                                                                                                                  │
│	using StudentApi.Application.Mappings;                                                                                                                    │
│	using StudentApi.Domain.Entities;																														  │
│                                                                                                                                                             │  
│	namespace StudentApi.Application.Students;                                                                                                                │
│                                                                                                                                                             │
│	public class StudentService : IStudentService                                                                                                             │
│	{                                                                                                                                                         │  
│		private readonly IStudentRepository _studentRepository;                                                                                               │
│                                                                                                                                                             │
│		public StudentService(IStudentRepository studentRepository)                                                                                           │
│		{                                                                                                                                                     │
│			_studentRepository = studentRepository;                                                                                                           │
│		}                                                                                                                                                     │
│                                                                                                                                                             │
│		public async Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)                                    │
│		{                                                                                                                                                     │
│			var student = await _studentRepository.GetByIdAsync(id, tenantId, cancellationToken);                                                             │
│                                                                                                                                                             │
│			return student?.ToDto();                                                                                                                          │
│		}                                                                                                                                                     │  
│                                                                                                                                                             │
│		public async Task<IReadOnlyList<StudentDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)                                │
│		{                                                                                                                                                     │
│			var students = await _studentRepository.GetAllAsync(tenantId, cancellationToken);                                                                 │
│                                                                                                                                                             │ 
│			return students.Select(s => s.ToDto()).ToList();                                                                                                  │
│		}                                                                                                                                                     │      
│                                                                                                                                                             │
│		public async Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)                                │
│		{                                                                                                                                                     │
│			var student = new Student                                                                                                                         │
│			{                                                                                                                                                 │
│				Id = Guid.NewGuid(),                                                                                                                          │
│				TenantId = request.TenantId,                                                                                                                  │
│				Name = request.Name,                                                                                                                          │
│				DateOfBirth = request.DateOfBirth                                                                                                             │
│			};                                                                                                                                                │
│                                                                                                                                                             │ 
│			await _studentRepository.AddAsync(student, cancellationToken);                                                                                    │
│                                                                                                                                                             │
│			return student.ToDto();                                                                                                                           │
│		}                                                                                                                                                     │
│                                                                                                                                                             │
│		public async Task<StudentDto> UpdateAsync(Guid id, Guid tenantId, UpdateStudentRequest request, CancellationToken cancellationToken = default)        │
│		{                                                                                                                                                     │  
│			var currentStudent = await _studentRepository.GetByIdAsync(id, tenantId, cancellationToken);                                                      │
│                                                                                                                                                             │
│			if (currentStudent is null)                                                                                                                       │
│			{                                                                                                                                                 │
│				throw new KeyNotFoundException($"Student with id '{id}' was not found for tenant '{tenantId}'.");                                             │
│			}                                                                                                                                                 │
│                                                                                                                                                             │
│			var updatedStudent = currentStudent with                                                                                                          │
│			{                                                                                                                                                 │
│				Name = request.Name,                                                                                                                          │
│				DateOfBirth = request.DateOfBirth                                                                                                             │   
│			};                                                                                                                                                │
│                                                                                                                                                             │
│			await _studentRepository.UpdateAsync(updatedStudent, cancellationToken);                                                                          │
│                                                                                                                                                             │
│			return updatedStudent.ToDto();                                                                                                                    │  
│		}                                                                                                                                                     │ 
│                                                                                                                                                             │   
│		public async Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)                                                  │
│		{                                                                                                                                                     │
│			var currentStudent = await _studentRepository.GetByIdAsync(id, tenantId, cancellationToken);                                                      │ 
│                                                                                                                                                             │
│			if (currentStudent is null)                                                                                                                       │
│			{                                                                                                                                                 │
│				throw new KeyNotFoundException($"Student with id '{id}' was not found for tenant '{tenantId}'.");                                             │
│			}                                                                                                                                                 │
│                                                                                                                                                             │
│			await _studentRepository.DeleteAsync(id, tenantId, cancellationToken);                                                                            │
│		}                                                                                                                                                     │   
│	}                                                                                                                                                         │
│                                                                                                                                                             │
│                                                                                                                                                             │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘ 

Campo _studentRepository:
- Dependencia principal del servicio.
- Es interfaz, no implementación concreta, para mantener desacoplamiento.

Constructor:
- Inyección de dependencias.
- Permite sustituir repositorio real por fake o mock en pruebas.

// Método GetByIdAsync
// Llama al repositorio
// Si encuentra entidad, la mapea a DTO con ToDto
// Si no existe, devuelve null
public async Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)                                    
{                                                                                                                                                     
	var student = await _studentRepository.GetByIdAsync(id, tenantId, cancellationToken);                                                             
                                                                                                                                                             
	return student?.ToDto();                                                                                                                          
}  


// Método GetAllAsync
// Obtiene la lista de entidades desde repositorio
// Mapea cada Student a StudentDto
// Devuelve lista de solo lectira por contrato
public async Task<IReadOnlyList<StudentDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
{
    var students = await _studentRepository.GetAllAsync(tenantId, cancellationToken);

    return students.Select(s => s.ToDto()).ToList();
}


// Método CreateAsync
// Crea un entidad Student en memoria
// Asigna id nuevo
// Propaga TenantId desde requet
// Persiste con AddAsync
// Devuelve DTO resultante
public async Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
{
    var student = new Student
    {
        Id = Guid.NewGuid(),
        TenantId = request.TenantId,
        Name = request.Name,
        DateOfBirth = request.DateOfBirth
    };

    await _studentRepository.AddAsync(student, cancellationToken);

    return student.ToDto();
}


// Método UpdateAsync
// Primero busca la entidad actual pr id y tenantId
// Si no existe, lanza KeyNotFoundException con mensaje explícito.
// Si existe, crea nueva versión con with usando Name y DateOfBirth del request.
// Persiste con UpdateAsync.
// Devuelve DTO actualizado.
public async Task<StudentDto> UpdateAsync(Guid id, Guid tenantId, UpdateStudentRequest request, CancellationToken cancellationToken = default)
{
    var currentStudent = await _studentRepository.GetByIdAsync(id, tenantId, cancellationToken);

    if (currentStudent is null)
    {
        throw new KeyNotFoundException($"Student with id '{id}' was not found for tenant '{tenantId}'.");
    }

    var updatedStudent = currentStudent with
    {
        Name = request.Name,
        DateOfBirth = request.DateOfBirth
    };

    await _studentRepository.UpdateAsync(updatedStudent, cancellationToken);

    return updatedStudent.ToDto();
 }


// Método DeleteAsync
// Primero valida existencia por id y TenantId
// Si no existe, lanza KeyNotFoundException; Si existe, llama DeleteAsync del repositorio.
public async Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
{
    var currentStudent = await _studentRepository.GetByIdAsync(id, tenantId, cancellationToken);

    if (currentStudent is null)
    {
        throw new KeyNotFoundException($"Student with id '{id}' was not found for tenant '{tenantId}'.");
    }

    await _studentRepository.DeleteAsync(id, tenantId, cancellationToken);
    }
}


DETALLES MULTI-TENANCY EN EL SERVICIO:
- En GetByIdAsync, GetAllAsync, UpdateAsync y DeleteAsync siempre se incluye tenantId.
- Esto refuerza aislamiento y evita cruces entre tenants desde Application.

DETALLES ASÍNCRONOS
- Todos los métodos son async con Task o Task<T>
- Se pasa CancellationToken para cancelación cooperativa en requests HTTP

BUENAS PRÁCTICAS
- Contrato de servicio separado de implementación.
- Orquestación en Application, no en controller.
- Repositorio como dependencia por interfaz.
- Mapeo explícito Domain a DTO.
- TenantId presente en operaciones sensibles.
- Verificación de existencia antes de update/delete.
- Uso consistente de métodos asíncronos y CancellationToken.

----------------------------------------------------------------------------------------------------------------------------------------------------

INFRASTRUCTURE CON EF Core

STEP 1 - IMPLEMENTAR DBCONTEXT + REPOSITORY + DI

Ya dejamos implementada la base real de la persistencia con EF Core para Student, conectada con multi-tenancy y lista para usar desde Application.

¿QUE SE HACE EN ESTE STEP?

1- Crear el DbContext de EF Core.
2- Definimos la configuración de entidad con IEntityTypeConfiguration
3- Se implementa IStudentRepòsitory método por método.
4- Registramos Infrastructure en DI.
5- Añadimos cadena de conexión en Presentación.


IMPLEMENTATION STEPS:

1 - Añadir paquetes de EF Core en StudentApi.,Infrastructure.csproj

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.5" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.5" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
  
  
2 - Crear ApplicationDbContext.cs

// Aquí estamos creando el contexto EF COre
// ApplicationDbContext hereda de DContext
// Expone DbSet<Student> como tabla lógica Students.
// OnModelCreating usa ApplyConfigurationFromAssenbly: con esto aplica todas las configuraciones IEntityTypeConfiguration del assenbly Infrastructure.
// La ventaja es un DbContext limpio, reglas de schema fuera de él.

using Microsoft.EntityFrameworkCore;
using StudentApi.Domain.Entities;

namespace StudentApi.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

3 - Configuración de entidad StudentConfiguration.cs

// Configuración de la entidad Student para SQL

┌────────────────────────────────────────────────────────────────────────────────┐
│                                                                                │                                                              
│           StudentConfiguration.cs                                              │                                                                   
├────────────────────────────────────────────────────────────────────────────────┤
│	using Microsoft.EntityFrameworkCore;                                         │
│	using Microsoft.EntityFrameworkCore.Metadata.Builders;                       │
│	using StudentApi.Domain.Entities;                                            │
│                                                                                │
│	namespace StudentApi.Infrastructure.Configurations;                          │
│                                                                                │
│	public class StudentConfiguration : IEntityTypeConfiguration<Student>        │
│	{                                                                            │
│		public void Configure(EntityTypeBuilder<Student> builder)                │
│		{                                                                        │
│			builder.ToTable("Students");                                         │
│                                                                                │
│			builder.HasKey(s => s.Id);                                           │
│                                                                                │
│			builder.Property(s => s.TenantId)                                    │ 
│				.IsRequired();                                                   │
│                                                                                │
│			builder.Property(s => s.Name)                                        │
│				.HasMaxLength(200)                                               │
│				.IsRequired();                                                   │ 
│                                                                                │
│			builder.Property(s => s.DateOfBirth)                                 │
│				.HasColumnType("date")                                           │
│				.IsRequired();                                                   │
│                                                                                │
│			builder.HasIndex(s => new { s.TenantId, s.Name });                   │
│		}                                                                        │
│	}                                                                            │                 
│		                                                                         │                                                             
│                                                                                │      
└────────────────────────────────────────────────────────────────────────────────┘ 

// ToTable Students: fija el nombre de la tabla
builder.ToTable("Students")

// HashKey id: define la clave primaria.
builder.HasKey(s => s.Id); 

// TenantId rquerido: refuerza multi-tenancy a nivel de esquema
builder.Property(s => s.TenantId)                                    
				.IsRequired(); 

// Name requerido y con longitud máxima 200: constraint de base de datos.
builder.Property(s => s.Name)                                        
				.HasMaxLength(200)                                               
				.IsRequired();   

// DateOfBirth tipo SQL date: mapeo correcto para fechas sin hora.
builder.Property(s => s.DateOfBirth)                                 
				.HasColumnType("date")                                           
				.IsRequired();   
				
// Indice TenantiD + Name:
//		Mejora búsquedas por tenant y nombre.
//		Soporta mejor consultas frecuentes segmentadas por tenant.
builder.HasIndex(s => new { s.TenantId, s.Name })



FASE CON LA QUE ESTOY

CURRENT PHASE:
Phase 4 – Infrastructure con EF Core

CURRENT STEP:
Step 1 – Persistencia real y conexión multi-tenant

GOAL:
Explicar en profundidad qué hace esta fase, por qué se implementa así en empresa, y qué resuelve cada parte del código actual.

THEORY:
En Clean Architecture, Infrastructure implementa detalles técnicos:

Cómo guardar y leer datos.
Cómo mapear el modelo de dominio a tablas SQL.
Cómo conectar dependencias técnicas en el contenedor DI.
En tu caso, esta fase convierte contratos de Application en ejecución real:

Application define la interfaz en IStudentRepository.cs.
Infrastructure la implementa en StudentRepository.cs.
Presentation registra todo desde Program.cs.
IMPLEMENTATION STEPS:

Crear el contexto EF Core:
ApplicationDbContext.cs
Configurar la entidad Student para SQL:
StudentConfiguration.cs
Implementar el repositorio con consultas multi-tenant:
StudentRepository.cs
Registrar servicios técnicos en DI:
InfrastructureServiceCollectionExtensions.cs
Conectar Infrastructure desde Presentation:
Program.cs
appsettings.json
CODE EXPLANATION:

ApplicationDbContext en ApplicationDbContext.cs
Hereda de DbContext: es la puerta de EF Core a la base de datos.
DbSet Students: representa la colección de entidades Student en el contexto.
OnModelCreating con ApplyConfigurationsFromAssembly:
Carga automáticamente todas las clases de configuración tipo IEntityTypeConfiguration del assembly.
Beneficio: DbContext limpio, reglas de mapping separadas.
Por qué es importante:

Evita meter configuración de tablas directamente en DbContext.
Escala mejor cuando tengas muchas entidades.
StudentConfiguration en StudentConfiguration.cs
ToTable Students: fija nombre de tabla.
HasKey Id: define clave primaria.
TenantId requerido: refuerza multi-tenancy a nivel de esquema.
Name requerido y con longitud máxima 200: constraint de base de datos.
DateOfBirth tipo SQL date: mapeo correcto para fechas sin hora.
Índice TenantId + Name:
Mejora búsquedas por tenant y nombre.
Soporta mejor consultas frecuentes segmentadas por tenant.
Por qué en Infrastructure y no en Domain:

Estas reglas son de persistencia física, no reglas del negocio puro.
StudentRepository en StudentRepository.cs
GetByIdAsync:

Consulta por Id y TenantId.
Usa AsNoTracking:
lectura más ligera,
evita overhead de tracking cuando no vas a modificar el resultado.
Resuelve aislamiento multi-tenant en lectura individual.
GetAllAsync:

Filtra por TenantId.
AsNoTracking para rendimiento.
OrderBy Name para orden estable.
Devuelve lista de solo lectura por contrato de Application.
AddAsync:

Inserta entidad en DbSet.
Guarda cambios inmediatamente con SaveChangesAsync.
UpdateAsync:

Marca entidad como modificada.
Guarda cambios con SaveChangesAsync.
DeleteAsync:

Busca por Id y TenantId.
Si no existe, retorna sin error.
Si existe, elimina y guarda cambios.
Razonamiento empresarial:

Cada método aplica tenantId explícitamente.
El repositorio no permite operaciones ciegas sin contexto de tenant.
DI de Infrastructure en InfrastructureServiceCollectionExtensions.cs
Lee DefaultConnection desde configuración.
Si no existe, lanza excepción clara y temprana.
Registra ApplicationDbContext con SQL Server.
Registra IStudentRepository con StudentRepository como Scoped.
Por qué Scoped:

Es el ciclo de vida correcto para DbContext en aplicaciones web por request.
Entrada desde Presentation
Program.cs llama AddInfrastructure.
appsettings.json contiene la connection string LocalDB.
Resultado: la API ya puede resolver repositorio y DbContext en runtime.
BEST PRACTICES:

Separar contrato en Application e implementación en Infrastructure.
Aplicar filtro tenant en todas las consultas de repositorio.
Usar AsNoTracking en lecturas para mejorar rendimiento.
Mantener configuraciones de entidad fuera de DbContext.
Fallar rápido cuando falta configuración crítica como ConnectionString.
WHAT YOU SHOULD LEARN:

Infrastructure no es lógica de negocio; es implementación técnica.
Multi-tenancy real empieza en el diseño de consultas, no solo en tener la columna TenantId.
EF Core se usa mejor con DbContext limpio + configuraciones separadas.
Repositorio bien implementado protege consistencia y seguridad de acceso a datos.
NEXT STEP:
Lo que falta para cerrar completamente esta parte de Phase 4:

Crear migración inicial y aplicar base de datos.
Añadir registration de IStudentService en DI para usar servicio en controllers.
Crear endpoints CRUD en Presentation y probar flujo end to end.
Después entrar en FluentValidation y middleware de errores con la base ya funcional.

----------------------------------------------------------------------------------------------------------------------------------------------------

PHASE 4 - STEP 2
MIGRACIONES EF CORE + CREACIÓN FÍSICA DE BASE DE DATOS

GOAL:
Cerrar Infrastructure dejando el modelo materializado en SQL y listo para pruebas end-to-end desde Presentation.

THEORY:
En Step 1 dejamos la persistencia implementada en código.
En Step 2 convertimos ese modelo en objetos reales en base de datos:

- Migration: historial versionado de cambios de esquema.
- Database update: aplicación real de esos cambios sobre una instancia SQL.

Esto separa claramente:
- Diseño del modelo en C#.
- Estado físico del esquema en SQL.

IMPLEMENTATION STEPS:

1. Asegurar paquete de diseño en startup project (Presentation) para tooling de EF.
2. Crear migración inicial desde Infrastructure.
3. Aplicar migración a la base configurada en appsettings.
4. Validar bloqueo de entorno si no existe LocalDB.
5. Dejar alternativa de conexión a SQL Express o SQL Server existente.

CODE:

// 1) Asegura herramientas EF Core en startup project
dotnet add src/StudentApi.Presentation/StudentApi.Presentation.csproj package Microsoft.EntityFrameworkCore.Design

// 2) Crea migración inicial
dotnet ef migrations add InitialCreate --project src/StudentApi.Infrastructure --startup-project src/StudentApi.Presentation --output-dir Persistence/Migrations

// 3) Aplica esquema a la base configurada
dotnet ef database update --project src/StudentApi.Infrastructure --startup-project src/StudentApi.Presentation

// 4) Ver migraciones detectadas por EF
dotnet ef migrations list --project src/StudentApi.Infrastructure --startup-project src/StudentApi.Presentation

// 5) Alinea versión de tool con runtime
dotnet tool update --global dotnet-ef --version 10.0.5


ESTADO REAL EN ESTE PROYECTO:

- La migración inicial ya fue creada correctamente.
- Los archivos de migración existen en:
	- src/StudentApi.Infrastructure/Persistence/Migrations/20260326111013_InitialCreate.cs
	- src/StudentApi.Infrastructure/Persistence/Migrations/20260326111013_InitialCreate.Designer.cs
	- src/StudentApi.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs
- El build de la solución está correcto.
- El apply a base quedó bloqueado por entorno: LocalDB no está instalado/disponible.


EXPLICACIÓN TÉCNICA DEL BLOQUEO:

Si la connection string apunta a (localdb)\\MSSQLLocalDB y la máquina no tiene LocalDB runtime:
- EF puede compilar y generar migraciones.
- Pero no puede abrir conexión para crear/actualizar la base.

Conclusión:
No es fallo de arquitectura ni de código de capas.
Es una dependencia de entorno de ejecución SQL.


RUTAS DE RESOLUCIÓN:

Ruta A - Instalar LocalDB
- Instalar SQL Server Express con LocalDB.
- Mantener la connection string actual.
- Reintentar database update.

Ruta B - Usar SQL Express / SQL Server existente
- Cambiar DefaultConnection en appsettings.
- Ejecutar database update contra esa instancia.

Ejemplo de connection string para SQLEXPRESS:

"ConnectionStrings": {
	"DefaultConnection": "Server=.\\SQLEXPRESS;Database=StudentApiDb;Trusted_Connection=True;TrustServerCertificate=True;"
}


BEST PRACTICES:

- Versionar siempre la carpeta Migrations en git.
- No editar manualmente migraciones salvo casos puntuales y controlados.
- Mantener una sola fuente de verdad del esquema: modelo EF + migraciones.
- Fallar rápido si falta connection string o instancia SQL.
- Validar cada cambio de esquema con build + database update.


WHAT YOU SHOULD LEARN:

- Migration no es solo "crear tabla", es historia de evolución del sistema.
- Infrastructure puede estar bien aunque el entorno local aún no esté listo.
- El startup project importa en EF Tools porque define configuración y DI activos.
- Tener SQL operativo es prerrequisito para cerrar la fase técnica de persistencia.


NEXT STEP:

Cuando confirmemos la ruta A o B y termine database update con éxito:
- Cerrar formalmente Phase 4.
- Entrar a Phase 5 Step 1: StudentsController CRUD en Presentation consumiendo IStudentService.

----------------------------------------------------------------------------------------------------------------------------------------------------