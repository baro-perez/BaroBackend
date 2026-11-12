using BaroBackend.Domain.Common;
using BaroBackend.Domain.Metagame;

namespace BaroBackend.Domain.Tests;

public class PlayerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ApplyResourceDelta_PositiveDelta_UpdatesBalanceAndReturnsLedgerEntry()
    {
        var player = Player.Create("player", Now);

        Assert.Equal(0, player.GetResourceAmount(Resources.CurrencyA));

        var entry = player.ApplyResourceDelta(Resources.CurrencyA, 100, Now, "grant");
        Assert.Equal(100, player.GetResourceAmount(Resources.CurrencyA));
        Assert.Equal(100, entry.Delta);
        Assert.Equal(100, entry.BalanceAfter);
    }

    [Fact]
    public void ApplyResourceDelta_NegativeDelta_IfWouldGoNegativeThrowsAndKeepsBalance()
    {
        var player = Player.Create("player", Now);
        player.ApplyResourceDelta(Resources.CurrencyA, 50, Now, "grant");

        Assert.Throws<DomainException>(() => player.ApplyResourceDelta(Resources.CurrencyA, -80, Now, "spend"));

        Assert.Equal(50, player.GetResourceAmount(Resources.CurrencyA));
    }

    [Fact]
    public void ApplyResourceDelta_PositieDelta_DoesNotAffectOtherResource()
    {
        var player = Player.Create("player", Now);

        Assert.Equal(0, player.GetResourceAmount(Resources.CurrencyA));
        Assert.Equal(0, player.GetResourceAmount(Resources.CurrencyB));
        player.ApplyResourceDelta(Resources.CurrencyA, 50, Now, "grant");
        Assert.Equal(50, player.GetResourceAmount(Resources.CurrencyA));
        Assert.Equal(0, player.GetResourceAmount(Resources.CurrencyB));
    }

    [Fact]
    public void ApplyResourceDelta_IncrementalOperations_Accumulate()
    {
        var player = Player.Create("player", Now);

        Assert.Equal(0, player.GetResourceAmount(Resources.CurrencyA));
        player.ApplyResourceDelta(Resources.CurrencyA, 50, Now, "grant");
        Assert.Equal(50, player.GetResourceAmount(Resources.CurrencyA));
        player.ApplyResourceDelta(Resources.CurrencyA, 50, Now, "grant");
        Assert.Equal(100, player.GetResourceAmount(Resources.CurrencyA));
        player.ApplyResourceDelta(Resources.CurrencyA, -30, Now, "grant");
        Assert.Equal(70, player.GetResourceAmount(Resources.CurrencyA));
    }
}