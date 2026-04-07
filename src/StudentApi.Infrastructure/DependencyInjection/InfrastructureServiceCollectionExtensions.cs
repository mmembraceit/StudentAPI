using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudentApi.Application.Interfaces;
using StudentApi.Infrastructure.Persistence;
using StudentApi.Infrastructure.Repositories;

namespace StudentApi.Infrastructure.DependencyInjection;

/// Entry point used to register Infrastructure dependencies.
/// Invoked by <c>Program.cs</c> in Presentation to wire SQL Server, EF Core, and repositories.
public static class InfrastructureServiceCollectionExtensions
{
    /// Registers the DbContext and the student repository.
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IUserAuthRepository, UserAuthRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }
}
