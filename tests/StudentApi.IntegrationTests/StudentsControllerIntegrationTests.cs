using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using StudentApi.IntegrationTests.Infrastructure;

namespace StudentApi.IntegrationTests;

[Collection("ApiIntegration")]
public class StudentsControllerIntegrationTests
{
    private static readonly Guid TenantA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantB = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly HttpClient _client;

    public StudentsControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task StudentsEndpoints_WithoutToken_ReturnUnauthorized()
    {
        var response = await _client.GetAsync($"/api/students?tenantId={TenantA}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task StudentsEndpoints_WithToken_ReturnExpectedStatusAndEnvelope()
    {
        var token = await LoginAndGetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/students", new
        {
            tenantId = TenantA,
            name = "Integration Student",
            dateOfBirth = "2001-01-01"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        using var createBody = await JsonTestHelpers.ReadJsonAsync(createResponse);
        JsonTestHelpers.AssertEnvelope(createBody, expectedSuccess: true);

        var id = createBody.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var getByIdResponse = await _client.GetAsync($"/api/students/{id}?tenantId={TenantA}");
        await JsonTestHelpers.AssertStatusAndEnvelopeAsync(getByIdResponse, HttpStatusCode.OK, expectedSuccess: true);

        var getAllResponse = await _client.GetAsync($"/api/students?tenantId={TenantA}");
        await JsonTestHelpers.AssertStatusAndEnvelopeAsync(getAllResponse, HttpStatusCode.OK, expectedSuccess: true);
    }

    [Fact]
    public async Task Students_AreTenantIsolated_WhenReadingById()
    {
        var token = await LoginAndGetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/students", new
        {
            tenantId = TenantA,
            name = "Tenant A Student",
            dateOfBirth = "2002-02-02"
        });
        createResponse.EnsureSuccessStatusCode();

        using var createBody = await JsonTestHelpers.ReadJsonAsync(createResponse);
        var id = createBody.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var wrongTenantResponse = await _client.GetAsync($"/api/students/{id}?tenantId={TenantB}");

        Assert.Equal(HttpStatusCode.NotFound, wrongTenantResponse.StatusCode);
        using var wrongTenantBody = await JsonTestHelpers.ReadJsonAsync(wrongTenantResponse);
        JsonTestHelpers.AssertEnvelope(wrongTenantBody, expectedSuccess: false);
    }

    private async Task<string> LoginAndGetAccessTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "admin123" });
        response.EnsureSuccessStatusCode();

        using var body = await JsonTestHelpers.ReadJsonAsync(response);
        var token = body.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));

        return token!;
    }
}
