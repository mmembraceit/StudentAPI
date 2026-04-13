using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudentApi.Application.Interfaces;
using StudentApi.Infrastructure.Caching;
using StudentApi.Infrastructure.DependencyInjection;
using StudentApi.Infrastructure.Messaging;

namespace StudentApi.UnitTests.Infrastructure.DependencyInjection;

public class InfrastructureServiceCollectionExtensionsTests
{
    [Fact]
    public void AddInfrastructure_WithEmptyRedisConfig_RegistersNoOpCache()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=StudentApiDb;Trusted_Connection=True;",
            ["Redis:ConnectionString"] = string.Empty,
            ["AzureServiceBus:ConnectionString"] = string.Empty,
            ["AzureServiceBus:QueueName"] = string.Empty
        });

        services.AddInfrastructure(configuration);

        var descriptor = services.Last(d => d.ServiceType == typeof(IStudentCacheService));
        Assert.Equal(typeof(NoOpStudentCacheService), descriptor.ImplementationType);
    }

    [Fact]
    public void AddInfrastructure_WithRedisConfig_RegistersRedisCache()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=StudentApiDb;Trusted_Connection=True;",
            ["Redis:ConnectionString"] = "localhost:6379",
            ["Redis:InstanceName"] = "StudentApi:",
            ["AzureServiceBus:ConnectionString"] = string.Empty,
            ["AzureServiceBus:QueueName"] = string.Empty
        });

        services.AddInfrastructure(configuration);

        var descriptor = services.Last(d => d.ServiceType == typeof(IStudentCacheService));
        Assert.Equal(typeof(RedisStudentCacheService), descriptor.ImplementationType);
    }

    [Fact]
    public void AddInfrastructure_WithEmptyServiceBusConfig_RegistersNoOpPublisher()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=StudentApiDb;Trusted_Connection=True;",
            ["Redis:ConnectionString"] = string.Empty,
            ["AzureServiceBus:ConnectionString"] = string.Empty,
            ["AzureServiceBus:QueueName"] = string.Empty
        });

        services.AddInfrastructure(configuration);

        var descriptor = services.Last(d => d.ServiceType == typeof(IStudentEventPublisher));
        Assert.Equal(typeof(NoOpStudentEventPublisher), descriptor.ImplementationType);
    }

    [Fact]
    public void AddInfrastructure_WithServiceBusConfig_RegistersAzurePublisher()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=StudentApiDb;Trusted_Connection=True;",
            ["Redis:ConnectionString"] = string.Empty,
            ["AzureServiceBus:ConnectionString"] = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;",
            ["AzureServiceBus:QueueName"] = "student-events"
        });

        services.AddInfrastructure(configuration);

        var descriptor = services.Last(d => d.ServiceType == typeof(IStudentEventPublisher));
        Assert.Equal(typeof(AzureServiceBusStudentEventPublisher), descriptor.ImplementationType);
    }

    private static IConfiguration BuildConfiguration(IReadOnlyDictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
