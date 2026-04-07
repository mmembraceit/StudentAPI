# Task 7: Configure Entity Constraints using IEntityTypeConfiguration

## Status

Completed

## What Is Implemented

- Entity configurations are defined in dedicated classes.
- Constraints and indexes are applied through configuration classes.
- DbContext applies configurations from assembly and avoids inline entity config.

## Evidence

- src/Infrastructure/Configurations/StudentConfiguration.cs
- src/Infrastructure/Configurations/UserAccountConfiguration.cs
- src/Infrastructure/Configurations/RefreshTokenConfiguration.cs
- src/Infrastructure/Persistence/ApplicationDbContext.cs
