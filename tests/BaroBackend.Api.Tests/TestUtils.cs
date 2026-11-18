using System.Net;

namespace BaroBackend.Api.Tests;

public static class TestUtils
{
    public static string UniquePlayerName()
    {
        return "p" + Guid.NewGuid().ToString("N")[..10];
    }

    public static async Task AssertStatusAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        if (response.StatusCode != expected)
        {
            var body = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Expected {expected} but got {(int)response.StatusCode} {response.StatusCode}.\n{body}");
        }
    }
}
