using FluentValidation;

namespace StudentApi.Application.Students;


/// Validator for the student creation payload.
/// Executed automatically from <c>ValidationActionFilter</c> in Presentation.

public sealed class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(request => request.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");

        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.DateOfBirth)
            .NotEqual(default(DateOnly))
            .WithMessage("DateOfBirth is required.")
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("DateOfBirth must be in the past.");
    }
}