using BaroBackend.Domain.Common;
using BaroBackend.Domain.Metagame;
using BaroBackend.Domain.Metagame.Data.Catalog;

using Moq;

namespace BaroBackend.Domain.Tests;
public class PlayerTests
{
    private const string CurrencyA = "currency_a";
    private const string CurrencyB = "currency_b";

    private static readonly DateTimeOffset Now = new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ApplyResourceDelta_PositiveDelta_UpdatesBalanceAndReturnsLedgerEntry()
    {
        var player = Player.Create("player", Now);

        Assert.Equal(0, player.GetResourceAmount(CurrencyA));

        var entry = player.ApplyResourceDelta(CurrencyA, 100, Now, MockResourceCatalog(), "grant");
        Assert.Equal(100, player.GetResourceAmount(CurrencyA));
        Assert.Equal(100, entry.Delta);
        Assert.Equal(100, entry.BalanceAfter);
    }

    [Fact]
    public void ApplyResourceDelta_NegativeDelta_IfWouldGoNegativeThrowsAndKeepsBalance()
    {
        var resourceCatalog = MockResourceCatalog();

        var player = Player.Create("player", Now);
        player.ApplyResourceDelta(CurrencyA, 50, Now, resourceCatalog, "grant");

        Assert.Throws<DomainException>(() => 
            player.ApplyResourceDelta(CurrencyA, -80, Now, resourceCatalog, "spend"));

        Assert.Equal(50, player.GetResourceAmount(CurrencyA));
    }

    private IResourceCatalog MockResourceCatalog()
    {
        var catalogMock = new Mock<IResourceCatalog>();

        catalogMock
            .Setup(c => c.ResourceIdIsValid(CurrencyA))
            .Returns(true);

        catalogMock
            .Setup(c => c.ResourceIdIsValid(CurrencyB))
            .Returns(true);

        return catalogMock.Object;
    }

    [Fact]
    public void ApplyResourceDelta_PositieDelta_DoesNotAffectOtherResource()
    {
        var player = Player.Create("player", Now);

        Assert.Equal(0, player.GetResourceAmount(CurrencyA));
        Assert.Equal(0, player.GetResourceAmount(CurrencyB));
        player.ApplyResourceDelta(CurrencyA, 50, Now, MockResourceCatalog(), "grant");
        Assert.Equal(50, player.GetResourceAmount(CurrencyA));
        Assert.Equal(0, player.GetResourceAmount(CurrencyB));
    }

    [Fact]
    public void ApplyResourceDelta_IncrementalOperations_Accumulate()
    {
        var player = Player.Create("player", Now);
        var resourceCatalog = MockResourceCatalog();

        Assert.Equal(0, player.GetResourceAmount(CurrencyA));
        player.ApplyResourceDelta(CurrencyA, 50, Now, resourceCatalog, "grant");
        Assert.Equal(50, player.GetResourceAmount(CurrencyA));
        player.ApplyResourceDelta(CurrencyA, 50, Now, resourceCatalog, "grant");
        Assert.Equal(100, player.GetResourceAmount(CurrencyA));
        player.ApplyResourceDelta(CurrencyA, -30, Now, resourceCatalog, "grant");
        Assert.Equal(70, player.GetResourceAmount(CurrencyA));
    }
}