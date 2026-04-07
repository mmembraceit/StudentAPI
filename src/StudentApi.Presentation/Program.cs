using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StudentApi.Application.Interfaces;
using StudentApi.Application.Students;
using StudentApi.Infrastructure.DependencyInjection;
using StudentApi.Presentation.Authentication;
using StudentApi.Presentation.Common;
using StudentApi.Presentation.Filters;
using StudentApi.Presentation.Middleware;
using System.Text;


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

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
{
    throw new InvalidOperationException("JWT key must be configured and be at least 32 characters long.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Authorization policy that requires the user to have the "Admin" role. Applied to student management endpoints.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
