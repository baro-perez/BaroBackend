using System.Net;

using BaroBackend.Api.Metagame.EndpointDtos;

namespace BaroBackend.Domain.Tests;

[Collection("api")]
public sealed class PlayerEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task Creating_a_player_returns_created_and_it_can_be_fetched()
    {
        var name = UniqueName();

        var response = await client.PostAsJsonAsync("/players", new CreatePlayerRequest(name));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);
        var result = await client.GetAsync(location);
        var response2 = await result.Content.ReadFromJsonAsync<PlayerResponse>();
        Assert.NotNull(response2);
        Assert.Equal(name, response2.Name);
    }

    private static string UniqueName() => "p" + Guid.NewGuid().ToString("N")[..10];
}