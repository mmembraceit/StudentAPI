# Plan de implementación .NET

## Estado recuperado

- El roadmap acordado existe en `DeployPlan.md`.
- El workspace actual no contiene todavía una solución `.NET` ni proyectos de código.
- Por tanto, aunque el seguimiento previo menciona `Phase 2`, el estado real del repositorio sigue antes de la implementación de `Phase 1`.

## Roadmap

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

## Punto de reanudación recomendado

### Si el código anterior se perdió o estaba en otra carpeta

Retomar desde `Phase 1` en este repositorio:

- crear la solution
- crear proyectos `Domain`, `Application`, `Infrastructure`, `Presentation`
- agregar referencias entre capas
- dejar compilación base funcionando

### Si `step 2` se refería al roadmap pedagógico

El `step 2` corresponde a ordenar el aprendizaje desde fundamentos hasta aspectos transversales de una API real, y ya quedó reflejado en la secuencia de fases anterior.

### Si `Phase 2` se refería a implementación

La siguiente entrega funcional sería la capa `Domain`, pero sólo después de crear la solution y la estructura base de proyectos en este workspace.

## Entrega siguiente sugerida

Implementar `Phase 1` en este repositorio y, a continuación, continuar con `Phase 2` construyendo:

- entidad `Student`
- value objects o reglas mínimas del dominio si aplican
- contratos base o primitivas compartidas del dominio
- uso de `record` donde tenga sentido