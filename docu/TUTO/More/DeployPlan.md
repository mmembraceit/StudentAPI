## Plan: Roadmap de enseñanza API .NET

Crear una guía pedagógica por micro-fases para una API REST en .NET con Clean Architecture, orientada a alguien que viene de Java, Python o JavaScript. En esta etapa el entregable es el roadmap completo de fases, que luego se desarrollará una por una con explicación previa al código.

**Steps**
1. Confirmar el punto de partida: proyecto greenfield sin solución .NET existente; los requisitos viven en `copilot-instructions.md`.
2. Organizar el aprendizaje desde fundamentos hasta aspectos transversales de una API real en empresa.
3. Distribuir los requisitos técnicos dentro de una secuencia didáctica que reduzca carga cognitiva para alguien no experto en C#.
4. Mantener el enfoque incremental: no avanzar a la siguiente fase hasta que la anterior esté clara.
5. Persistir en `plan.md` la guía acordada para poder seguirla fase por fase.

**Roadmap de fases**
1. Phase 0 → Setup de entorno y conceptos base de .NET
2. Phase 1 → Crear la Solution y los proyectos de Clean Architecture
3. Phase 2 → Construir la capa Domain
4. Phase 3 → Construir la capa Application
5. Phase 4 → Construir la capa Infrastructure con EF Core, Repository Pattern, configuraciones de entidades y migraciones
6. Phase 5 → Construir la capa Presentation y el CRUD básico de Student
7. Phase 6 → Añadir validaciones con FluentValidation
8. Phase 7 → Implementar manejo global de errores con middleware
9. Phase 8 → Estandarizar respuestas con un Generic API Response Wrapper
10. Phase 9 → Implementar autenticación JWT y proteger endpoints
11. Phase 10 → Implementar multi-tenancy con TenantId, aislamiento, transacciones y seeding de datos
12. Phase 11 → Añadir integraciones reales: Redis cache, Azure Service Bus, SignalR y Webhooks
13. Phase 12 → Añadir logging estructurado con Serilog y aplicar seguridad OWASP, CORS y headers

**Relevant files**
- `c:\Users\mmb84\LocalRepository\Practice\StudentsAPI.mmb\copilot-instructions.md` — fuente de requisitos funcionales y no funcionales
- `C:\Users\mmb84\LocalRepository\Practice\StudentsAPI.mmb\plan.md` — guía persistida para continuar el trabajo fase por fase

**Verification**
1. Verificar que el roadmap cubra las fases solicitadas por el usuario: 0 a 12.
2. Verificar que todos los requisitos del documento queden ubicados en alguna fase del roadmap.
3. Verificar que el orden pedagógico vaya de conceptos base a integración y endurecimiento de la API.

**Decisions**
- Incluido: roadmap pedagógico completo en español.
- Excluido por ahora: explicación detallada de cada fase, comandos, código e implementación.
- Azure Functions y Blob Storage quedan fuera de la implementación porque el requisito indica que no son necesarios.
- Redis, Service Bus, SignalR y Webhooks se agrupan en una fase avanzada de integraciones reales.
- Constraints con `IEntityTypeConfiguration`, transacciones y seeding se introducen cuando ya exista contexto suficiente de infraestructura y persistencia.