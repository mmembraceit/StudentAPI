using System.Net;
using System.Text.Json;

namespace StudentApi.IntegrationTests.Infrastructure;

internal static class JsonTestHelpers
{
    public static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(payload);
    }

    public static void AssertEnvelope(JsonDocument document, bool expectedSuccess)
    {
        var root = document.RootElement;
        Assert.Equal(expectedSuccess, root.GetProperty("success").GetBoolean());
        Assert.True(root.TryGetProperty("data", out _));
        Assert.True(root.TryGetProperty("errors", out _));
    }

    public static async Task AssertStatusAndEnvelopeAsync(HttpResponseMessage response, HttpStatusCode statusCode, bool expectedSuccess)
    {
        Assert.Equal(statusCode, response.StatusCode);

        using var body = await ReadJsonAsync(response);
        AssertEnvelope(body, expectedSuccess);
    }
}
