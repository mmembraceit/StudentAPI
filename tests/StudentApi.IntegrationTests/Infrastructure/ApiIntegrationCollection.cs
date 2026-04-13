using Xunit;

namespace StudentApi.IntegrationTests.Infrastructure;

[CollectionDefinition("ApiIntegration")]
public sealed class ApiIntegrationCollection : ICollectionFixture<TestWebApplicationFactory>
{
}
