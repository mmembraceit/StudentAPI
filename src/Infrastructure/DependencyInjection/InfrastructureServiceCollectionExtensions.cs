using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudentApi.Application.Interfaces;
using StudentApi.Infrastructure.Caching;
using StudentApi.Infrastructure.Persistence;
using StudentApi.Infrastructure.Repositories;

namespace StudentApi.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering infrastructure services.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registers persistence, repositories, and cache-related infrastructure services.
    /// </summary>
    /// <param name="services">Service collection to configure.</param>
    /// <param name="configuration">Application configuration source.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        var redisConnectionString = configuration["Redis:ConnectionString"];
        var redisInstanceName = configuration["Redis:InstanceName"] ?? "StudentApi:";

        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = redisInstanceName;
            });

            services.AddScoped<IStudentCacheService, RedisStudentCacheService>();
        }
        else
        {
            services.AddDistributedMemoryCache();
            services.AddScoped<IStudentCacheService, NoOpStudentCacheService>();
        }

        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IUserAuthRepository, UserAuthRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }
}
