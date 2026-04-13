Task 1

Title: Build a RESTful CRUD API with Clean Architecture
 
Description:

Using the schema:

Student (id, name, dateOfBirth)
 
Create a fully functional RESTful API in .NET that follows Clean Architecture principles.

The solution must be structured into layers and implement best practices used in the company.
 
The API should support full CRUD operations and integrate multiple real-world components such as persistence, messaging, caching, and external integrations.
 
Technologies and patterns to include:

- Clean Architecture

- SQL Database (via Entity Framework Core)

- Repository Pattern

- DTO Mapping (Mapster or custom mapping)

- Transaction Management

- FluentValidation

- Azure Services:

  - Azure Functions (NOT NECESSARY IMPLEMENT)

  - Redis Cache

  - Blob Storage (NOT NECESSARY IMPLEMENT)

  - Azure Service Bus (Queues/Topics)

- SignalR (for real-time updates)

- Webhooks (for external notifications)
 
Acceptance Criteria:

- Solution is structured into Domain, Application, Infrastructure, Presentation

- CRUD endpoints exist

- Database is SQL-based and managed via EF Core migrations

- DTOs are used

- Repository pattern is implemented

- Transactions are handled properly

- At least one Azure integration is demonstrated

- SignalR hub is implemented

- Webhook is triggered on at least one action
 
Take into account : Use records when possible 

Task 2
Title: Implement Global Error Handling

Description:
Introduce a centralized error handling mechanism using middleware.

Acceptance Criteria:
- Global exception middleware is implemented
- Standardized error responses
- Correct HTTP status codes
- Errors are logged

Task 3
Title: Introduce a Generic API Response Model

Description:
Standardize API responses using a generic wrapper.

Acceptance Criteria:
- Unified response structure
- Includes success, data, errors
- No raw objects returned

Task 4
Title: Implement JWT Authentication

Description:
Secure the API using JWT.

Acceptance Criteria:
- JWT configured
- Protected endpoints require token
- Unauthorized returns 401
- Token includes claims

Task 5
Title: Add Request Validation with FluentValidation

Description:
Implement validation for incoming requests.

Acceptance Criteria:
- Validators exist
- Invalid requests return 400
- No validation in controllers

Task 6
Title: Implement Multi-Tenancy with Tenant Isolation

Description:
Introduce TenantId-based isolation.

Acceptance Criteria:
- TenantId in all entities
- Middleware extracts TenantId
- Queries filtered automatically
- Isolation verified

Task 7
Title: Configure Entity Constraints using IEntityTypeConfiguration

Description:
Define DB constraints via configuration classes.

Acceptance Criteria:
- Config classes exist
- Constraints applied
- No config in DbContext

Task 8
Title: Implement Database Seeding

# Here we can seed data o table creation, but we want to seed data on application startup, so we can have some initial data to work with.

Description:
Seed initial data.

Acceptance Criteria:
- Seed data exists
- Runs automatically
- No duplicates

Task 9
Title: Add Structured Logging with Serilog

Description:
Integrate Serilog.

Acceptance Criteria:
- Logging configured
- Includes request and error logs
- Structured logs

Task 10
Title: Apply OWASP Security Best Practices

Description:
Apply security headers and configurations.

Acceptance Criteria:
- Security headers configured
- CORS restricted
- Verified via inspection