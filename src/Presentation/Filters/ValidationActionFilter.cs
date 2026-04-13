using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StudentApi.Presentation.Common;

namespace StudentApi.Presentation.Filters;


/// Executes FluentValidation validators for action arguments before controller logic runs.
public sealed class ValidationActionFilter : IAsyncActionFilter
{
   
    /// Validates action arguments and short-circuits with a 400 response when errors are found.
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

  
    /// Builds a typed FluentValidation context for a runtime argument instance.
    /// <returns>Validation context consumed by FluentValidation validators.</returns>
    private static IValidationContext CreateValidationContext(object argument)
    {
        var validationContextType = typeof(ValidationContext<>).MakeGenericType(argument.GetType());
        return (IValidationContext)Activator.CreateInstance(validationContextType, argument)!;
    }
}