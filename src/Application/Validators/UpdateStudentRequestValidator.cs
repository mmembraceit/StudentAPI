using FluentValidation;

namespace StudentApi.Application.Students;

/// <summary>
/// FluentValidation rules for <see cref="UpdateStudentRequest"/>.
/// </summary>
public sealed class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
    /// <summary>
    /// Initializes validation rules for student update.
    /// </summary>
    public UpdateStudentRequestValidator()
    {
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