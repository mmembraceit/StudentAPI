using FluentValidation;

namespace StudentApi.Application.Students;


/// Validator for the student update payload.
/// same as the create validator: keep validation outside the controller.

public sealed class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
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