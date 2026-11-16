using BaroBackend.Domain.Common;
using BaroBackend.Domain.Metagame;
using BaroBackend.Domain.Metagame.Data;

using Moq;

namespace BaroBackend.Domain.Tests;

public class PlayerTests
{
    private const string CurrencyA = "currency_a";
    private const string CurrencyB = "currency_b";

    private static readonly DateTimeOffset Now = new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ApplyResourceDelta_PositiveDelta_UpdatesBalanceAndReturnsLedgerEntry()
    {
        var player = Player.Create("player", Now);

        Assert.Equal(0, player.GetResourceAmount(CurrencyA));

        var entry = await player.ApplyResourceDelta(CurrencyA, 100, Now, MockResourceCatalog(), "grant");
        Assert.Equal(100, player.GetResourceAmount(CurrencyA));
        Assert.Equal(100, entry.Delta);
        Assert.Equal(100, entry.BalanceAfter);
    }

    [Fact]
    public async Task ApplyResourceDelta_NegativeDelta_IfWouldGoNegativeThrowsAndKeepsBalance()
    {
        var resourceCatalog = MockResourceCatalog();

        var player = Player.Create("player", Now);
        await player.ApplyResourceDelta(CurrencyA, 50, Now, resourceCatalog, "grant");

        await Assert.ThrowsAsync<DomainException>(() => 
            player.ApplyResourceDelta(CurrencyA, -80, Now, resourceCatalog, "spend"));

        Assert.Equal(50, player.GetResourceAmount(CurrencyA));
    }

    private IResourceCatalog MockResourceCatalog()
    {
        var catalogMock = new Mock<IResourceCatalog>();

        catalogMock
            .Setup(c => c.ResourceIdIsValid(CurrencyA))
            .Returns(Task.FromResult(true));

        catalogMock
            .Setup(c => c.ResourceIdIsValid(CurrencyB))
            .Returns(Task.FromResult(true));

        return catalogMock.Object;
    }

    [Fact]
    public async Task ApplyResourceDelta_PositieDelta_DoesNotAffectOtherResource()
    {
        var player = Player.Create("player", Now);

        Assert.Equal(0, player.GetResourceAmount(CurrencyA));
        Assert.Equal(0, player.GetResourceAmount(CurrencyB));
        await player.ApplyResourceDelta(CurrencyA, 50, Now, MockResourceCatalog(), "grant");
        Assert.Equal(50, player.GetResourceAmount(CurrencyA));
        Assert.Equal(0, player.GetResourceAmount(CurrencyB));
    }

    [Fact]
    public async Task ApplyResourceDelta_IncrementalOperations_Accumulate()
    {
        var player = Player.Create("player", Now);
        var resourceCatalog = MockResourceCatalog();

        Assert.Equal(0, player.GetResourceAmount(CurrencyA));
        await player.ApplyResourceDelta(CurrencyA, 50, Now, resourceCatalog, "grant");
        Assert.Equal(50, player.GetResourceAmount(CurrencyA));
        await player.ApplyResourceDelta(CurrencyA, 50, Now, resourceCatalog, "grant");
        Assert.Equal(100, player.GetResourceAmount(CurrencyA));
        await player.ApplyResourceDelta(CurrencyA, -30, Now, resourceCatalog, "grant");
        Assert.Equal(70, player.GetResourceAmount(CurrencyA));
    }
}