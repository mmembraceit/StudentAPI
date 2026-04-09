# 08 — Azure Service Bus

This document explains the Azure Service Bus integration in StudentAPI from a junior-friendly point of view. It focuses on what problem Service Bus solves, where the code lives, how the message publishing flow works, and what happens when Azure Service Bus is not configured.

---

## What Problem Does Service Bus Solve?

Sometimes an API should tell other systems that something important happened.

Examples:
- A student was created
- A student was updated
- A student was deleted

The API could call other systems directly, but that would make the request slower and tightly coupled.

Instead, StudentAPI publishes an **integration event** to Azure Service Bus:

```text
Student changed in API
        ↓
Create event message
        ↓
Send message to Service Bus queue
        ↓
Another service can process it later
```

This gives us:
- Better separation between systems
- Faster API responses than direct external calls
- A clean way to integrate future workers, Azure Functions, webhooks, or other services

---

## Where The Code Lives

The integration is split by layer, following Clean Architecture.

### Application layer

These files define **what** should be published, without knowing **how** Azure works:

```text
src/Application/
├── Interfaces/
│   └── IStudentEventPublisher.cs
└── Students/
    └── Events/
        ├── StudentCreatedIntegrationEvent.cs
        ├── StudentUpdatedIntegrationEvent.cs
        └── StudentDeletedIntegrationEvent.cs
```

### Infrastructure layer

These files implement the Azure-specific behavior:

```text
src/Infrastructure/
├── Messaging/
│   ├── AzureServiceBusOptions.cs
│   ├── AzureServiceBusStudentEventPublisher.cs
│   └── NoOpStudentEventPublisher.cs
└── DependencyInjection/
    └── InfrastructureServiceCollectionExtensions.cs
```

### Service orchestration

The Student service publishes events after successful write operations:

```text
src/Application/Students/Services/StudentService.cs
```

---

## The Main Contract

The application layer defines this interface:

```csharp
public interface IStudentEventPublisher
{
    Task PublishCreatedAsync(StudentCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    Task PublishUpdatedAsync(StudentUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    Task PublishDeletedAsync(StudentDeletedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
```

Why this is important:
- The application layer does not depend on Azure SDK types
- The service only knows it is publishing events
- Infrastructure decides whether to use real Azure Service Bus or a fallback publisher

---

## The Integration Events

StudentAPI currently publishes three events:

1. `StudentCreatedIntegrationEvent`
2. `StudentUpdatedIntegrationEvent`
3. `StudentDeletedIntegrationEvent`

Each event contains:
- `EventId`
- `OccurredAtUtc`
- `StudentId`
- `TenantId`
- `Name`
- `DateOfBirth`

This payload gives downstream consumers enough information to understand what happened without re-querying the API immediately.

---

## How The Flow Works

### Create flow

```text
HTTP POST /api/students
        ↓
StudentsController
        ↓
StudentService.CreateAsync
        ↓
1. Save student in SQL Server
2. Update Redis cache
3. Build StudentCreatedIntegrationEvent
4. Publish event through IStudentEventPublisher
```

### Update flow

```text
HTTP PUT /api/students/{id}
        ↓
StudentsController
        ↓
StudentService.UpdateAsync
        ↓
1. Load current student
2. Save updated student in SQL Server
3. Refresh by-id cache and invalidate list cache
4. Build StudentUpdatedIntegrationEvent
5. Publish event
```

### Delete flow

```text
HTTP DELETE /api/students/{id}
        ↓
StudentsController
        ↓
StudentService.DeleteAsync
        ↓
1. Confirm student exists
2. Delete from SQL Server
3. Invalidate Redis keys
4. Build StudentDeletedIntegrationEvent
5. Publish event
```

Notice that **read operations** do not publish Service Bus messages. Only state-changing operations do.

---

## Dependency Injection Decision

The infrastructure registration checks configuration values.

If these are present:
- `AzureServiceBus:ConnectionString`
- `AzureServiceBus:QueueName`

then the app registers:

```text
AzureServiceBusStudentEventPublisher
```

Otherwise it registers:

```text
NoOpStudentEventPublisher
```

This decision happens in:

```text
src/Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs
```

That means the rest of the application does not need `if` statements for “Azure enabled” versus “Azure disabled”.

---

## Real Publisher vs No-Op Publisher

### Real Azure publisher

`AzureServiceBusStudentEventPublisher` does the real work:

1. Receives an integration event
2. Serializes it to JSON
3. Creates a `ServiceBusMessage`
4. Adds metadata like:
   - `Subject`
   - `MessageId`
   - `studentId`
   - `tenantId`
5. Sends the message to the configured queue
6. Logs `SERVICE BUS SEND ...`

### No-op publisher

`NoOpStudentEventPublisher` is a safe fallback used when Service Bus is not configured.

It does not send anything. It only logs:

```text
SERVICE BUS SKIP ...
```

This is useful for local development because:
- The app still runs without Azure
- The application flow remains the same
- Developers can verify that publishing would have happened

This is the same design idea used by the Redis no-op cache fallback.

---

## Configuration

The configuration section looks like this:

```json
"AzureServiceBus": {
  "ConnectionString": "",
  "QueueName": ""
}
```

This lives in:

```text
src/Presentation/appsettings.json
src/Presentation/appsettings.Development.json
```

For local testing with a real Service Bus namespace or emulator, you can override these values using environment variables:

```powershell
$env:AzureServiceBus__ConnectionString='...'
$env:AzureServiceBus__QueueName='student-events'
```

---

## What The Publisher Sends

When the real Azure publisher is active, it sends a `ServiceBusMessage` with:

- Message body: serialized JSON event
- `ContentType = application/json`
- `Subject = student.created` or `student.updated` or `student.deleted`
- `MessageId = EventId`
- Application properties:
  - `studentId`
  - `tenantId`

These extra fields help consumers filter or inspect messages without parsing the whole body first.

---

## Logging Behavior

### When Azure Service Bus is not configured

You should see logs like:

```text
SERVICE BUS SKIP student.created student:... tenant:...
```

### When Azure Service Bus is configured correctly

You should see logs like:

```text
SERVICE BUS SEND student.created student:... tenant:...
```

This is the easiest first check to confirm which publisher implementation is active.

---

## Why This Design Is Good

From a junior developer perspective, these are the most important design lessons:

1. The application layer defines the port (`IStudentEventPublisher`)
2. Infrastructure implements the adapter (`AzureServiceBusStudentEventPublisher`)
3. The service publishes only after successful writes
4. The app can run without Azure using the no-op fallback
5. The rest of the code does not care whether messages go to real Azure or not

This is a classic Clean Architecture pattern: **depend on abstractions, not external tools directly**.

---

## Current Limitations

The current implementation is good for the project, but it is still a minimal version.

Important limitations:

1. It publishes directly after the DB write
2. If SQL succeeds but Service Bus send fails, the API request can still fail after the data is already saved
3. There is no outbox pattern yet
4. There is no message consumer in this repository yet

For a production-grade solution, teams often use an **outbox pattern** so database changes and outgoing events are coordinated more safely.

---

## Quick Mental Model

Use this simple mental model:

```text
Student API changes data
        ↓
StudentService creates an integration event
        ↓
IStudentEventPublisher publishes it
        ↓
Infrastructure decides:
    - send to Azure Service Bus
    - or skip safely in local mode
```

If you understand that flow, you understand the whole feature.