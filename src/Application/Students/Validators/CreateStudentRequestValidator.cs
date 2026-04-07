using FluentValidation;

namespace StudentApi.Application.Students;

/// <summary>
/// FluentValidation rules for <see cref="CreateStudentRequest"/>.
/// </summary>
public sealed class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    /// <summary>
    /// Initializes validation rules for student creation.
    /// </summary>
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