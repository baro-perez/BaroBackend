using System.Net;

using BaroBackend.Api.Metagame.EndpointDtos;
using BaroBackend.Infrastructure.Persistence;

using Microsoft.Extensions.DependencyInjection;

namespace BaroBackend.Api.Tests;

[Collection("api")]
public sealed class PlayerEndpointsTests(ApiFactory factory)
{
    private const string CurrencyA = "currency_a";

    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task PostPlayer_NewPlayer_ReturnsPlayerAndCanBeFetched()
    {
        var name = TestUtils.UniquePlayerName();

        using var response = await client.PostAsJsonAsync(PlayerEndpoints.BasePath, new CreatePlayerDto(name));
        await TestUtils.AssertStatusAsync(response, HttpStatusCode.Created);
        var location = response.Headers.Location;
        Assert.NotNull(location);
        using var result = await client.GetAsync(location);
        var response2 = await result.Content.ReadFromJsonAsync<PlayerDto>();
        Assert.NotNull(response2);
        Assert.Equal(name, response2.Name);
    }

    [Fact]
    public async Task PostPlayer_NewPlayer_ShortNameRejected()
    {
        var name = "bo";
        using var response = await client.PostAsJsonAsync(PlayerEndpoints.BasePath, new CreatePlayerDto(name));
        await TestUtils.AssertStatusAsync(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostPlayer_NewPlayer_LongNameRejected()
    {
        var name = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";
        using var response = await client.PostAsJsonAsync(PlayerEndpoints.BasePath, new CreatePlayerDto(name));
        await TestUtils.AssertStatusAsync(response, HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("ana", "ana")]
    [InlineData("bob", "Bob")]
    [InlineData("car", "CAR")]
    public async Task PostPlayer_DuplicatePlayer_Fails(string name1, string name2)
    {
        using var response1 = await client.PostAsJsonAsync(PlayerEndpoints.BasePath, new CreatePlayerDto(name1));
        await TestUtils.AssertStatusAsync(response1, HttpStatusCode.Created);
        using var response2 = await client.PostAsJsonAsync(PlayerEndpoints.BasePath, new CreatePlayerDto(name2));
        await TestUtils.AssertStatusAsync(response2, HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostModifications_SingleCurrencyToManyPlayers_AreDifferent()
    {
        using var responsePlayer1 = await client.PostAsJsonAsync(
            PlayerEndpoints.BasePath, new CreatePlayerDto(TestUtils.UniquePlayerName()));
        var player1 = await responsePlayer1.Content.ReadFromJsonAsync<PlayerDto>();
        using var responsePlayer2 = await client.PostAsJsonAsync(
            PlayerEndpoints.BasePath, new CreatePlayerDto(TestUtils.UniquePlayerName()));
        var player2 = await responsePlayer2.Content.ReadFromJsonAsync<PlayerDto>();

        Assert.NotNull(player1);
        Assert.NotNull(player2);

        using var responseModifyPlayer1 = await client.PostAsJsonAsync(
            PlayerEndpoints.ResolvePlayerBalanceDevModifyRoute(player1.Id),
            new ResourceAndAmountDto(CurrencyA, 100));
        await TestUtils.AssertStatusAsync(responseModifyPlayer1, HttpStatusCode.OK);

        using var responseModifyPlayer2 = await client.PostAsJsonAsync(
            PlayerEndpoints.ResolvePlayerBalanceDevModifyRoute(player2.Id),
            new ResourceAndAmountDto(CurrencyA, 50));
        await TestUtils.AssertStatusAsync(responseModifyPlayer2, HttpStatusCode.OK);

        var resourceAmounts1 = await client.GetFromJsonAsync<ResourceAndAmountDto[]>(
            PlayerEndpoints.ResolvePlayerBalancesRoute(player1.Id));
        Assert.NotNull(resourceAmounts1);

        var resourceAmounts2 = await client.GetFromJsonAsync<ResourceAndAmountDto[]>(
            PlayerEndpoints.ResolvePlayerBalancesRoute(player2.Id));
        Assert.NotNull(resourceAmounts2);

        var currency1 = resourceAmounts1.FirstOrDefault(r => r.ResourceId == CurrencyA);
        var currency2 = resourceAmounts1.FirstOrDefault(r => r.ResourceId == CurrencyA);
        Assert.NotNull(currency1);
        Assert.NotNull(currency2);
        Assert.Equal(100, currency1.Amount);
        Assert.Equal(100, currency2.Amount);
    }

    [Fact]
    public async Task PostModifications_SingleCurrency_CanBeRetrievedBack()
    {
        var amount = 100;
        using var responsePlayer = await client.PostAsJsonAsync(
            PlayerEndpoints.BasePath, new CreatePlayerDto(TestUtils.UniquePlayerName()));
        var player = await responsePlayer.Content.ReadFromJsonAsync<PlayerDto>();
        Assert.NotNull(player);

        using var responseModify = await client.PostAsJsonAsync(
            PlayerEndpoints.ResolvePlayerBalanceDevModifyRoute(player.Id),
            new ResourceAndAmountDto(CurrencyA, amount));
        await TestUtils.AssertStatusAsync(responseModify, HttpStatusCode.OK);

        var resourceAmounts = await client.GetFromJsonAsync<ResourceAndAmountDto[]>(
            PlayerEndpoints.ResolvePlayerBalancesRoute(player.Id));
        Assert.NotNull(resourceAmounts);

        var currency = resourceAmounts.FirstOrDefault(r => r.ResourceId == CurrencyA);
        Assert.NotNull(currency);
        Assert.Equal(amount, currency.Amount);
    }

    [Theory]
    [InlineData(50, 25)]
    [InlineData(100, 200)]
    [InlineData(100, -50)]
    public async Task PostModifications_SingleCurrency_Accumulates(long a, long b)
    {
        var expected = a + b;
        using var responsePlayer = await client.PostAsJsonAsync(
            PlayerEndpoints.BasePath, new CreatePlayerDto(TestUtils.UniquePlayerName()));
        var player = await responsePlayer.Content.ReadFromJsonAsync<PlayerDto>();
        Assert.NotNull(player);

        using var responseModifyA = await client.PostAsJsonAsync(
            PlayerEndpoints.ResolvePlayerBalanceDevModifyRoute(player.Id),
            new ResourceAndAmountDto(CurrencyA, a));
        await TestUtils.AssertStatusAsync(responseModifyA, HttpStatusCode.OK);

        using var responseModifyB = await client.PostAsJsonAsync(
            PlayerEndpoints.ResolvePlayerBalanceDevModifyRoute(player.Id),
            new ResourceAndAmountDto(CurrencyA, b));
        await TestUtils.AssertStatusAsync(responseModifyB, HttpStatusCode.OK);

        var resourceAmounts = await client.GetFromJsonAsync<ResourceAndAmountDto[]>(
            PlayerEndpoints.ResolvePlayerBalancesRoute(player.Id));
        Assert.NotNull(resourceAmounts);

        var currency = resourceAmounts.FirstOrDefault(r => r.ResourceId == CurrencyA);
        Assert.NotNull(currency);
        Assert.Equal(expected, currency.Amount);
    }

    [Theory]
    [InlineData(50, 75)]
    [InlineData(100, 200)]
    public async Task PostModifications_SingleCurrency_CantRemoveMoreThanYouHave(long a, long b)
    {
        Assert.True(b > a, "b > a");
        using var responsePlayer = await client.PostAsJsonAsync(
            PlayerEndpoints.BasePath, new CreatePlayerDto(TestUtils.UniquePlayerName()));
        var player = await responsePlayer.Content.ReadFromJsonAsync<PlayerDto>();
        Assert.NotNull(player);

        using var responseModifyA = await client.PostAsJsonAsync(
            PlayerEndpoints.ResolvePlayerBalanceDevModifyRoute(player.Id),
            new ResourceAndAmountDto(CurrencyA, a));
        await TestUtils.AssertStatusAsync(responseModifyA, HttpStatusCode.OK);

        using var responseModifyB = await client.PostAsJsonAsync(
            PlayerEndpoints.ResolvePlayerBalanceDevModifyRoute(player.Id),
            new ResourceAndAmountDto(CurrencyA, -b));
        await TestUtils.AssertStatusAsync(responseModifyB, HttpStatusCode.BadRequest);

        var resourceAmounts = await client.GetFromJsonAsync<ResourceAndAmountDto[]>(
            PlayerEndpoints.ResolvePlayerBalancesRoute(player.Id));
        Assert.NotNull(resourceAmounts);

        var currency = resourceAmounts.FirstOrDefault(r => r.ResourceId == CurrencyA);
        Assert.NotNull(currency);
        Assert.Equal(a, currency.Amount);
    }

    [Theory]
    [InlineData(50, 75, 100, 125, 150)]
    [InlineData(100, 200, 300, 400, 500)]
    [InlineData(100, -20, 30, -40, -50)]
    public async Task PostModifications_SingleCurrency_LedgerIsConsistent(
        long a, long b, long c, long d, long e)
    {
        var expected = a + b + c + d + e;
        using var responsePlayer = await client.PostAsJsonAsync(
            PlayerEndpoints.BasePath, new CreatePlayerDto(TestUtils.UniquePlayerName()));
        var player = await responsePlayer.Content.ReadFromJsonAsync<PlayerDto>();
        Assert.NotNull(player);

        foreach (var v in new[] { a, b, c, d, e })
        {
            using var responseModify = await client.PostAsJsonAsync(
            PlayerEndpoints.ResolvePlayerBalanceDevModifyRoute(player.Id),
                new ResourceAndAmountDto(CurrencyA, v));
            await TestUtils.AssertStatusAsync(responseModify, HttpStatusCode.OK);
        }

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BaroDbContext>();

        var sum = db.LedgerEntries
            .Where(e => e.PlayerId == player.Id)
            .Select(e => e.Delta).Sum();
        Assert.Equal(expected, sum);

        var lastEntry = db.LedgerEntries
            .OrderBy(e => e.CreatedAt)
            .LastOrDefault();
        Assert.Equal(expected, lastEntry?.BalanceAfter);
    }
}