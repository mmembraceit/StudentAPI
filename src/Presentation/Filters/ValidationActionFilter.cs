using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StudentApi.Presentation.Common;

namespace StudentApi.Presentation.Filters;

/// <summary>
/// Executes FluentValidation validators for action arguments before controller logic runs.
/// </summary>
public sealed class ValidationActionFilter : IAsyncActionFilter
{
    /// <summary>
    /// Validates action arguments and short-circuits with a 400 response when errors are found.
    /// </summary>
    /// <param name="context">Current action execution context.</param>
    /// <param name="next">Delegate that executes the next action pipeline step.</param>
    /// <returns>A task that completes when validation and next step execution finish.</returns>
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

    /// <summary>
    /// Builds a typed FluentValidation context for a runtime argument instance.
    /// </summary>
    /// <param name="argument">Action argument to validate.</param>
    /// <returns>Validation context consumed by FluentValidation validators.</returns>
    private static IValidationContext CreateValidationContext(object argument)
    {
        var validationContextType = typeof(ValidationContext<>).MakeGenericType(argument.GetType());
        return (IValidationContext)Activator.CreateInstance(validationContextType, argument)!;
    }
}