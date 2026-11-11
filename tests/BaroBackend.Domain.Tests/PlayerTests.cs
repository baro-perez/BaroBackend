using BaroBackend.Domain.Common;
using BaroBackend.Domain.MetaGame;

namespace BaroBackend.Domain.Tests;

public class PlayerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ApplyResourceDelta_positive_delta_updates_balance_and_returns_ledger_entry()
    {
        var player = Player.Create("player", Now);

        Assert.Equal(0, player.GetBalance(Resources.CurrencyA));

        var entry = player.ApplyResourceDelta(Resources.CurrencyA, 100, Now, "grant");
        Assert.Equal(100, player.GetBalance(Resources.CurrencyA));
        Assert.Equal(100, entry.Delta);
        Assert.Equal(100, entry.BalanceAfter);
    }

    [Fact]
    public void ApplyResourceDelta_that_would_go_negative_throws_and_keeps_balance()
    {
        var player = Player.Create("player", Now);
        player.ApplyResourceDelta(Resources.CurrencyA, 50, Now, "grant");

        Assert.Throws<DomainException>(() => player.ApplyResourceDelta(Resources.CurrencyA, -80, Now, "spend"));

        Assert.Equal(50, player.GetBalance(Resources.CurrencyA));
    }

    [Fact]
    public void ApplyResourceDelta_resource_added_does_not_affect_another()
    {
        var player = Player.Create("player", Now);

        Assert.Equal(0, player.GetBalance(Resources.CurrencyA));
        Assert.Equal(0, player.GetBalance(Resources.CurrencyB));
        player.ApplyResourceDelta(Resources.CurrencyA, 50, Now, "grant");
        Assert.Equal(50, player.GetBalance(Resources.CurrencyA));
        Assert.Equal(0, player.GetBalance(Resources.CurrencyB));
    }

    [Fact]
    public void ApplyResourceDelta_incremental_operations_accumulate()
    {
        var player = Player.Create("player", Now);

        Assert.Equal(0, player.GetBalance(Resources.CurrencyA));
        player.ApplyResourceDelta(Resources.CurrencyA, 50, Now, "grant");
        Assert.Equal(50, player.GetBalance(Resources.CurrencyA));
        player.ApplyResourceDelta(Resources.CurrencyA, 50, Now, "grant");
        Assert.Equal(100, player.GetBalance(Resources.CurrencyA));
        player.ApplyResourceDelta(Resources.CurrencyA, -30, Now, "grant");
        Assert.Equal(70, player.GetBalance(Resources.CurrencyA));
    }
}