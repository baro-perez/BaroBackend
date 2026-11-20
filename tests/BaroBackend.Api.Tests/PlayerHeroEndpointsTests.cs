using System.Net;
using System.Numerics;
using System.Security.Cryptography;

using BaroBackend.Api.Metagame.EndpointDtos;
using BaroBackend.Domain.Metagame;
using BaroBackend.Domain.Metagame.Data;
using BaroBackend.Domain.Metagame.Data.Catalog;
using BaroBackend.Infrastructure.Catalog;
using BaroBackend.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BaroBackend.Api.Tests;

[Collection("api")]
public sealed class PlayerHeroEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient client = factory.CreateClient();
    private readonly IHeroCatalog? heroCatalog = (factory.Services.GetService(typeof(IHeroCatalogProvider)) as IHeroCatalogProvider)?.GetObject();
    private readonly IResourceCatalog? resourceCatalog = (factory.Services.GetService(typeof(IResourceCatalogProvider)) as IResourceCatalogProvider)?.GetObject();

    private const string Hero1Id = "hero1";
    private const string CurrencyA = "currency_a";

    /// <summary>
    /// Helper
    /// </summary>
    public async Task<PlayerDto> CreatePlayer(string name)
    {
        using var playerResponse = await client.PostAsJsonAsync(PlayerEndpoints.BasePath, new CreatePlayerDto(name));
        var player = await playerResponse.Content.ReadFromJsonAsync<PlayerDto>();
        Assert.NotNull(player);
        return player;
    }

    /// <summary>
    /// Helper
    /// </summary>
    private async Task GrantCostToPlayer(PlayerDto player, Domain.Metagame.Data.Cost? cost)
    {
        Assert.NotNull(cost);
        foreach (var resource in cost.Resources)
        {
            using var response = await client.PostAsJsonAsync(
                PlayerEndpoints.ResolvePlayerBalanceDevModifyRoute(player.Id),
                new ResourceAndAmountDto(resource.ResourceId, resource.Amount));
        }
    }

    private async Task GrantHero(PlayerDto player, string heroId, int level = 1)
    {
        using var response = await client.PostAsJsonAsync(
            PlayerEndpoints.ResolvePlayerHeroesDevUnlockOneRoute(player.Id),
            new PlayerHeroDto(heroId, level));
    }

    [Fact]
    public async Task PostDevUnlockHero_ExistingHero_HeroIsCreatedAtLocation()
    {
        var player = await CreatePlayer(TestUtils.UniquePlayerName());

        using var heroResponse = await client.PostAsJsonAsync(
            PlayerEndpoints.ResolvePlayerHeroesDevUnlockOneRoute(player.Id),
            new PlayerHeroDto(Hero1Id, 2));
        await TestUtils.AssertStatusAsync(heroResponse, HttpStatusCode.Created);
        var location = heroResponse.Headers.Location;
        Assert.NotNull(location);
        using var result = await client.GetAsync(location);
        var playerHero = await result.Content.ReadFromJsonAsync<PlayerHeroDto>();
        Assert.NotNull(playerHero);
        Assert.Equal(Hero1Id, playerHero.HeroId);
        Assert.Equal(2, playerHero.Level);
    }

    [Fact]
    public async Task PostDevUnlockHero_NonExistingHero_HeroIsCreatedAtLocation()
    {
        var player = await CreatePlayer(TestUtils.UniquePlayerName());

        const string NotHero1Id = "not_" + Hero1Id;
        var heroResponse = await client.PostAsJsonAsync(
            PlayerEndpoints.ResolvePlayerHeroesDevUnlockOneRoute(player.Id),
            new PlayerHeroDto(NotHero1Id, 2));
        await TestUtils.AssertStatusAsync(heroResponse, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostBuyUnlockHero_WithResources_HeroIsCreatedAtLocation()
    {
        var player = await CreatePlayer(TestUtils.UniquePlayerName());

        // Grant unlock cost
        Assert.NotNull(heroCatalog);
        var cost = heroCatalog.GetHeroById(Hero1Id)?.GetUnlockCost();
        Assert.NotNull(cost);
        await GrantCostToPlayer(player, cost);

        using var heroResponse = await client.PostAsync(
            PlayerEndpoints.ResolvePlayerBuyUnlockHeroRoute(player.Id, Hero1Id), null);
        await TestUtils.AssertStatusAsync(heroResponse, HttpStatusCode.Created);

        var location = heroResponse.Headers.Location;
        Assert.NotNull(location);
        using var result = await client.GetAsync(location);
        var playerHero = await result.Content.ReadFromJsonAsync<PlayerHeroDto>();
        Assert.NotNull(playerHero);
        Assert.Equal(Hero1Id, playerHero.HeroId);
        Assert.Equal(1, playerHero.Level);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BaroDbContext>();

        var ledgerChanges = db.LedgerEntries
            .Where(e => e.Reason == Player.GetLedgerReasonBuyUnlockHero(Hero1Id))
            .Where(e => e.PlayerId == player.Id)
            .ToArray();
        BaroBackend.Domain.Tests.TestUtils.VerifyBuyHeroLedger(cost, Hero1Id, ledgerChanges);
    }

    [Fact]
    public async Task PostBuyUnlockHero_WithoutResources_Fails()
    {
        var player = await CreatePlayer(TestUtils.UniquePlayerName());
        using var heroResponse = await client.PostAsync(
            PlayerEndpoints.ResolvePlayerBuyUnlockHeroRoute(player.Id, Hero1Id), null);
        await TestUtils.AssertStatusAsync(heroResponse, HttpStatusCode.UnprocessableContent);
    }

    [Fact]
    public async Task PostLevelUpHero_WithResources_Succeeds()
    {
        var player = await CreatePlayer(TestUtils.UniquePlayerName());
        const int currentLevel = 3;
        await GrantHero(player, Hero1Id, currentLevel);

        // Grant level up cost
        Assert.NotNull(heroCatalog);
        var cost = heroCatalog?.GetHeroById(Hero1Id)?.GetLevelUpCost(currentLevel);
        Assert.NotNull(cost);
        await GrantCostToPlayer(player, cost);

        using var heroResponse = await client.PostAsync(
            PlayerEndpoints.ResolvePlayerBuyLevelUpHeroRoute(player.Id, Hero1Id), null);
        await TestUtils.AssertStatusAsync(heroResponse, HttpStatusCode.OK);

        using var result = await client.GetAsync(PlayerEndpoints.ResolvePlayerHeroRoute(player.Id, Hero1Id));
        var playerHero = await result.Content.ReadFromJsonAsync<PlayerHeroDto>();
        Assert.NotNull(playerHero);
        Assert.Equal(Hero1Id, playerHero.HeroId);
        Assert.Equal(currentLevel + 1, playerHero.Level);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BaroDbContext>();

        var ledgerChanges = db.LedgerEntries
            .Where(e => e.Reason == Player.GetLedgerReasonBuyLevelUpHero(Hero1Id))
            .Where(e => e.PlayerId == player.Id)
            .ToArray();
        BaroBackend.Domain.Tests.TestUtils.VerifyBuyHeroLedger(cost, Hero1Id, ledgerChanges, currentLevel + 1);
    }

    [Fact]
    public async Task PostBuyLevelUpHero_WithoutResources_Fails()
    {
        var player = await CreatePlayer(TestUtils.UniquePlayerName());
        const int currentLevel = 3;
        await GrantHero(player, Hero1Id, currentLevel);

        // Grant unlock cost instead of level up cost, which is lower
        Assert.NotNull(heroCatalog);
        var cost = heroCatalog?.GetHeroById(Hero1Id)?.GetUnlockCost();
        await GrantCostToPlayer(player, cost);

        using var heroResponse = await client.PostAsync(
            PlayerEndpoints.ResolvePlayerBuyLevelUpHeroRoute(player.Id, Hero1Id), null);
        await TestUtils.AssertStatusAsync(heroResponse, HttpStatusCode.UnprocessableContent);
    }

    [Fact]
    public async Task PostLevelUpHero_ConcurrentLevelUpDeterministic_Throws()
    {
        var player = await CreatePlayer(TestUtils.UniquePlayerName());
        const int currentLevel = 3;
        await GrantHero(player, Hero1Id, currentLevel);

        // Grant level up cost
        Assert.NotNull(heroCatalog);
        Assert.NotNull(resourceCatalog);
        var cost = heroCatalog.GetHeroById(Hero1Id)?.GetLevelUpCost(currentLevel);
        await GrantCostToPlayer(player, cost);

        using var scopeA = factory.Services.CreateScope();
        using var scopeB = factory.Services.CreateScope();
        var dbA = scopeA.ServiceProvider.GetRequiredService<BaroDbContext>();
        var dbB = scopeB.ServiceProvider.GetRequiredService<BaroDbContext>();

        var playerA = dbA.Players.FirstOrDefault(p => p.Id == player.Id);
        var playerB = dbB.Players.FirstOrDefault(p => p.Id == player.Id);
        Assert.NotNull(playerA);
        Assert.NotNull(playerB);

        var now = new DateTimeOffset(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

        // First operation succeeds
        playerA.BuyLevelUpHero(Hero1Id, heroCatalog, resourceCatalog, now);
        await dbA.SaveChangesAsync();
        playerB.BuyLevelUpHero(Hero1Id, heroCatalog, resourceCatalog, now);

        // Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => dbB.SaveChangesAsync());
    }

    [Theory]
    [InlineData(5, 30)]
    public async Task PostLevelUpHero_ConcurrentLevelUp_Throws(int loops, int concurrentTasks)
    {
        for (int i = 0; i < loops; ++i)
        {
            await PostLevelUpHero_ConcurrentLevelUp_Throws_Implementation(concurrentTasks);
        }
    }

    private async Task PostLevelUpHero_ConcurrentLevelUp_Throws_Implementation(int concurrentTasks)
    {
        var player = await CreatePlayer(TestUtils.UniquePlayerName());
        const int currentLevel = 3;
        await GrantHero(player, Hero1Id, currentLevel);

        // Grant level up cost
        Assert.NotNull(heroCatalog);
        var cost = heroCatalog?.GetHeroById(Hero1Id)?.GetLevelUpCost(currentLevel);
        await GrantCostToPlayer(player, cost);

        var levelUpUri = PlayerEndpoints.ResolvePlayerBuyLevelUpHeroRoute(player.Id, Hero1Id);
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var tasks = Enumerable.Range(0, concurrentTasks).Select(
            async _ => 
            {
                await gate.Task;
                return await client.PostAsync(levelUpUri, null);
            })
            .ToList();
        gate.SetResult();
        var responses = await Task.WhenAll(tasks);
        Assert.Equal(1, responses.Count(a => a.StatusCode == HttpStatusCode.OK));

        foreach(var r in responses)
        {
            r.Dispose();
        }
    }
}
