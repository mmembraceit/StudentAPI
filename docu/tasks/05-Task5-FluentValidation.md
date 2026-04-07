# Task 5: Add Request Validation with FluentValidation

## Status

Completed

## What Is Implemented

- Validators exist for create and update student requests.
- Validation filter executes validators before controller logic.
- Invalid requests return 400 with standardized ApiResponse errors.
- Validation logic is outside controllers.

## Evidence

- src/Application/Students/Validators/CreateStudentRequestValidator.cs
- src/Application/Students/Validators/UpdateStudentRequestValidator.cs
- src/Presentation/Filters/ValidationActionFilter.cs
- src/Presentation/Program.cs
