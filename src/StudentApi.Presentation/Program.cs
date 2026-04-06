using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using StudentApi.Application.Interfaces;
using StudentApi.Application.Students;
using StudentApi.Infrastructure.DependencyInjection;
using StudentApi.Presentation.Common;
using StudentApi.Presentation.Filters;
using StudentApi.Presentation.Middleware;


// Composition root for the web application.
// This is where Presentation, Application, and Infrastructure are wired together through DI and middleware.
var builder = WebApplication.CreateBuilder(args);


// Registers the global filter that executes FluentValidation validators before the controller.
builder.Services.AddScoped<ValidationActionFilter>();
builder.Services.AddScoped<IValidator<CreateStudentRequest>, CreateStudentRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateStudentRequest>, UpdateStudentRequestValidator>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationActionFilter>();
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(modelState => modelState.Errors)
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "The request is invalid." : error.ErrorMessage)
            .ToArray();

        return new BadRequestObjectResult(ApiResponse<object?>.FailureResponse(errors));
    };
});
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IStudentService, StudentService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// Global error middleware. Converts application exceptions into standard HTTP responses.
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.MapControllers();

app.Run();
