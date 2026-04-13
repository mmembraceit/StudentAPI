using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace StudentApi.IntegrationTests.Infrastructure;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=localhost,14333;Database=StudentApiDb;User Id=sa;Password=StudentApi#2026!Db;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true");
        builder.UseSetting("Jwt:Issuer", "StudentApi");
        builder.UseSetting("Jwt:Audience", "StudentApiClients");
        builder.UseSetting("Jwt:ExpirationMinutes", "60");
        builder.UseSetting("Jwt:Key", "SuperLongLocalDevJwtSecretKey_ChangeMe_123456");
        builder.UseSetting("Redis:ConnectionString", string.Empty);
        builder.UseSetting("AzureServiceBus:ConnectionString", string.Empty);
        builder.UseSetting("AzureServiceBus:QueueName", string.Empty);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var values = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost,14333;Database=StudentApiDb;User Id=sa;Password=StudentApi#2026!Db;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true",
                ["Jwt:Issuer"] = "StudentApi",
                ["Jwt:Audience"] = "StudentApiClients",
                ["Jwt:ExpirationMinutes"] = "60",
                ["Jwt:Key"] = "SuperLongLocalDevJwtSecretKey_ChangeMe_123456",
                ["Redis:ConnectionString"] = string.Empty,
                ["AzureServiceBus:ConnectionString"] = string.Empty,
                ["AzureServiceBus:QueueName"] = string.Empty
            };

            config.AddInMemoryCollection(values);
        });
    }
}
