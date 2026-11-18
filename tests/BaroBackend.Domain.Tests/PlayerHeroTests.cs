using BaroBackend.Domain.Common;
using BaroBackend.Domain.Metagame;
using BaroBackend.Domain.Metagame.Data;
using BaroBackend.Domain.Metagame.Data.Catalog;

namespace BaroBackend.Domain.Tests;
public class PlayerHeroTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);
    private const string SingleHeroId = "hero1";
    private static readonly IHeroCatalog SingleHeroCatalog = 
        TestUtils.CreateHeroCatalogWithSingleHero(SingleHeroId);
    private static readonly IHeroCatalog MockHeroCatalog =
        TestUtils.CreateMockHeroCatalog();
    private static readonly IResourceCatalog MockResourceCatalog =
        TestUtils.CreateMockResourceCatalog();


    [Theory]
    [InlineData(1), InlineData(4)]
    public void DevUnlockHero_ExistingHero_HeroExistsInPlayerAtGivenLevel(int level)
    {
        var player = Player.Create("player", Now);

        Assert.DoesNotContain(player.Heroes, h => h.HeroId == SingleHeroId);
        var result = player.DevUnlockHero(SingleHeroId, level, SingleHeroCatalog, Now);
        Assert.Contains(player.Heroes, h => h.HeroId == SingleHeroId);
        Assert.Equal(level, player.GetHeroLevel(SingleHeroId));

        // verify ledger

        var ledgerEntry = result.LedgerChanges[0];
        Assert.Equal(SingleHeroId, ledgerEntry.ItemId);
        Assert.Equal(PlayerItem.Hero, ledgerEntry.ItemType);
        Assert.Equal(level, ledgerEntry.Delta);
        Assert.Equal(level, ledgerEntry.BalanceAfter);
    }

    [Fact]
    public void DevUnlockHero_ExistingHero_ThrowsWithLevelZero()
    {
        var player = Player.Create("player", Now);

        Assert.DoesNotContain(player.Heroes, h => h.HeroId == SingleHeroId);
        Assert.Throws<DomainException>(() => player.DevUnlockHero(SingleHeroId, 0, SingleHeroCatalog, Now));
    }

    [Fact]
    public void DevUnlockHero_NonExistingHero_Throws()
    {
        var player = Player.Create("player", Now);
        const string HeroId = "not_" + SingleHeroId;
        Assert.DoesNotContain(player.Heroes, h => h.HeroId == HeroId);
        Assert.Throws<DomainException>(() => 
            player.DevUnlockHero(HeroId, 0, SingleHeroCatalog, Now));
    }

    [Fact]
    public void BuyUnlockHero_NonExistingHero_Throws()
    {
        var player = Player.Create("player", Now);
        const string HeroId = "not_" + SingleHeroId;
        Assert.DoesNotContain(player.Heroes, h => h.HeroId == HeroId);
        Assert.Throws<DomainException>(() => 
            player.BuyUnlockHero(HeroId, SingleHeroCatalog, MockResourceCatalog, Now));
    }

    [Fact]
    public void BuyUnlockHero_ExistingHeroAndHasResources_HeroExistsInPlayerAtLevelOne()
    {
        var player = Player.Create("player", Now);

        Assert.DoesNotContain(player.Heroes, h => h.HeroId == SingleHeroId);

        var heroData = MockHeroCatalog.GetHeroById(SingleHeroId);
        Assert.NotNull(heroData);
        var cost = heroData.GetUnlockCost();
        foreach (var resource in cost.Resources)
        {
            player.ApplyResourceDelta(resource.ResourceId, resource.Amount, Now,
                MockResourceCatalog, "test");
        }

        var result = player.BuyUnlockHero(SingleHeroId, MockHeroCatalog, MockResourceCatalog, Now);
        Assert.Contains(player.Heroes, h => h.HeroId == SingleHeroId);
        Assert.Equal(1, player.GetHeroLevel(SingleHeroId));

        TestUtils.VerifyBuyHeroLedger(cost, SingleHeroId, result.LedgerChanges);
    }

    [Fact]
    public void BuyUnlockHero_ExistingHeroAndDoesntHaveResources_Throws()
    {
        var player = Player.Create("player", Now);

        Assert.DoesNotContain(player.Heroes, h => h.HeroId == SingleHeroId);
        var heroData = MockHeroCatalog.GetHeroById(SingleHeroId);
        Assert.NotNull(heroData);
        Assert.False(player.CanSpend(heroData.GetUnlockCost()));

        Assert.Throws<DomainException>(() => player.BuyUnlockHero(
            SingleHeroId, MockHeroCatalog, MockResourceCatalog, Now));
    }


    [Theory]
    [InlineData(1), InlineData(4)]
    public void BuyLevelUpHero_HasResources_HeroWasLeveledUp(int currentLevel)
    {
        var player = Player.Create("player", Now);
        player.DevUnlockHero(SingleHeroId, currentLevel, MockHeroCatalog, Now);
        Assert.Equal(currentLevel, player.GetHeroLevel(SingleHeroId));

        var heroData = MockHeroCatalog.GetHeroById(SingleHeroId);
        Assert.NotNull(heroData);
        var cost = heroData.GetLevelUpCost(currentLevel);
        foreach (var resource in cost.Resources)
        {
            player.ApplyResourceDelta(resource.ResourceId, resource.Amount, Now,
                MockResourceCatalog, "test");
        }

        var result = player.BuyLevelUpHero(SingleHeroId, MockHeroCatalog, MockResourceCatalog, Now);
        Assert.Equal(currentLevel + 1, player.GetHeroLevel(SingleHeroId));

        TestUtils.VerifyBuyHeroLedger(cost, SingleHeroId, result.LedgerChanges, currentLevel + 1);
    }

    [Theory]
    [InlineData(1), InlineData(4)]
    public void BuyLevelUpHero_DoesntHaveResources_Throws(int currentLevel)
    {
        var player = Player.Create("player", Now);
        player.DevUnlockHero(SingleHeroId, currentLevel, MockHeroCatalog, Now);
        Assert.Equal(currentLevel, player.GetHeroLevel(SingleHeroId));
        var heroData = MockHeroCatalog.GetHeroById(SingleHeroId);
        Assert.NotNull(heroData);
        Assert.False(player.CanSpend(heroData.GetLevelUpCost(currentLevel)));

        Assert.Throws<DomainException>(() => player.BuyLevelUpHero(
            SingleHeroId, MockHeroCatalog, MockResourceCatalog, Now));
    }
}
