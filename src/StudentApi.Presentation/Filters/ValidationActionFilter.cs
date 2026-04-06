using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StudentApi.Presentation.Common;

namespace StudentApi.Presentation.Filters;

/// Global filter that executes FluentValidation before entering the controller.
/// It looks for registered validators for action arguments and, if there are errors, returns a 400 with <c>ApiResponse</c>.
/// It is related to <c>CreateStudentRequestValidator</c>, <c>UpdateStudentRequestValidator</c>, and the pipeline registration in <c>Program.cs</c>.
public sealed class ValidationActionFilter : IAsyncActionFilter
{
    /// Executes validation before the controller processes the action.
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new List<string>();

        foreach (var argument in context.ActionArguments.Values.Where(value => value is not null))
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(argument!.GetType());
            var validators = context.HttpContext.RequestServices.GetServices(validatorType);

            foreach (var validator in validators.Cast<IValidator>())
            {
                var validationContext = CreateValidationContext(argument);
                var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                errors.AddRange(result.Errors.Select(error => error.ErrorMessage));
            }
        }

        if (errors.Count > 0)
        {
            context.Result = new BadRequestObjectResult(ApiResponse<object?>.FailureResponse(errors));
            return;
        }

        await next();
    }

    
    /// Builds the FluentValidation context for the current argument.
        private static IValidationContext CreateValidationContext(object argument)
    {
        var validationContextType = typeof(ValidationContext<>).MakeGenericType(argument.GetType());
        return (IValidationContext)Activator.CreateInstance(validationContextType, argument)!;
    }
}