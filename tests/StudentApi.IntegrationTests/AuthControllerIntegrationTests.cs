using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using StudentApi.IntegrationTests.Infrastructure;

namespace StudentApi.IntegrationTests;

[Collection("ApiIntegration")]
public class AuthControllerIntegrationTests
{
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAccessAndRefreshTokens()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "admin123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var body = await JsonTestHelpers.ReadJsonAsync(response);
        JsonTestHelpers.AssertEnvelope(body, expectedSuccess: true);

        var data = body.RootElement.GetProperty("data");
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("accessToken").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("refreshToken").GetString()));
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorizedWithFailureEnvelope()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "wrong-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        using var body = await JsonTestHelpers.ReadJsonAsync(response);
        JsonTestHelpers.AssertEnvelope(body, expectedSuccess: false);

        var errors = body.RootElement.GetProperty("errors").EnumerateArray().Select(e => e.GetString()).ToArray();
        Assert.Contains("Invalid username or password.", errors);
    }

    [Fact]
    public async Task Refresh_WithValidToken_RotatesRefreshToken_AndOldTokenIsRejected()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "admin123" });
        loginResponse.EnsureSuccessStatusCode();

        using var loginBody = await JsonTestHelpers.ReadJsonAsync(loginResponse);
        var oldRefreshToken = loginBody.RootElement.GetProperty("data").GetProperty("refreshToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(oldRefreshToken));

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = oldRefreshToken });
        refreshResponse.EnsureSuccessStatusCode();

        using var refreshBody = await JsonTestHelpers.ReadJsonAsync(refreshResponse);
        JsonTestHelpers.AssertEnvelope(refreshBody, expectedSuccess: true);
        var newRefreshToken = refreshBody.RootElement.GetProperty("data").GetProperty("refreshToken").GetString();

        Assert.False(string.IsNullOrWhiteSpace(newRefreshToken));
        Assert.NotEqual(oldRefreshToken, newRefreshToken);

        var reuseOldTokenResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = oldRefreshToken });

        Assert.Equal(HttpStatusCode.Unauthorized, reuseOldTokenResponse.StatusCode);
        using var oldTokenBody = await JsonTestHelpers.ReadJsonAsync(reuseOldTokenResponse);
        JsonTestHelpers.AssertEnvelope(oldTokenBody, expectedSuccess: false);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorizedWithFailureEnvelope()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = "invalid-token" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        using var body = await JsonTestHelpers.ReadJsonAsync(response);
        JsonTestHelpers.AssertEnvelope(body, expectedSuccess: false);

        var errors = body.RootElement.GetProperty("errors").EnumerateArray().Select(e => e.GetString()).ToArray();
        Assert.Contains("Invalid refresh token.", errors);
    }
}
