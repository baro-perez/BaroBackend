using BaroBackend.Domain.Metagame;
using BaroBackend.Domain.Metagame.Data;
using BaroBackend.Domain.Metagame.Data.Catalog;

using NSubstitute;

namespace BaroBackend.Domain.Tests;

public static class TestUtils
{
    public static IHeroCatalog CreateHeroCatalog(Dictionary<string, HeroData> data)
    {
        return new DictionaryHeroCatalog(data);
    }
    public static IResourceCatalog CreateResourceCatalog(Dictionary<string, ResourceData> data)
    {
        return new DictionaryResourceCatalog(data);
    }

    public static HeroStatData CreateFakeHeroStatData()
    {
        return new(100, 10);
    }

    public static HeroData CreateFakeHeroData(string v)
    {
        return new(v, v, CreateFakeHeroStatData(), CreateFakeHeroStatData(), CreateFakeHeroStatData(),
            [new([]), new([]), new([]), new([]), new([]), new([]), new([]), new([]), new([]), new([])]);
    }

    public static IHeroCatalog CreateHeroCatalogWithSingleHero(string heroId)
    {
        return CreateHeroCatalog(new()
        {
            [heroId] = CreateFakeHeroData(heroId),
        });
    }

    public static IHeroCatalog CreateMockHeroCatalog(
        long baseLevelUpCost = 10, long increasePerLevel = 1,
        string? overrideLevelUpCurrency = null,
        int nLevels = 10)
    {
        var result = Substitute.For<IHeroCatalog>();
        result.HeroIdIsValid(string.Empty).ReturnsForAnyArgs(true);
        result.GetHeroById(string.Empty).ReturnsForAnyArgs(callInfo =>
        {
            var id = callInfo.Arg<string>();
            var levelUpCurrency = overrideLevelUpCurrency ?? Resources.HeroTokenId(id);
            return new HeroData(id, id,
                CreateFakeHeroStatData(),
                CreateFakeHeroStatData(),
                CreateFakeHeroStatData(),
                Enumerable.Range(0, nLevels)
                    .Select(i => new Cost([new(levelUpCurrency, baseLevelUpCost + increasePerLevel * i)]))
                    .ToArray());
        });
        return result;
    }

    public static IResourceCatalog CreateMockResourceCatalog()
    {
        var result = Substitute.For<IResourceCatalog>();
        result.ResourceIdIsValid(string.Empty).ReturnsForAnyArgs(true);
        result.GetResourceById(string.Empty).ReturnsForAnyArgs(callInfo =>
            {
                var id = callInfo.Arg<string>();
                return new ResourceData(id, id);
            });
        return result;
    }

    public static void VerifyBuyHeroLedger(
        Cost cost, 
        string heroId, 
        IReadOnlyList<LedgerEntry> ledgerChanges,
        int expectedLevel = 1)
    {
        var hadHeroIdEntry = false;
        foreach (var ledgerEntry in ledgerChanges)
        {
            if (ledgerEntry.ItemType == PlayerItem.Hero)
            {
                Assert.Equal(heroId, ledgerEntry.ItemId);
                Assert.Equal(1, ledgerEntry.Delta);
                Assert.Equal(expectedLevel, ledgerEntry.BalanceAfter);
                hadHeroIdEntry = true;
            }
            else
            {
                Assert.Equal(PlayerItem.Resource, ledgerEntry.ItemType);
                var totalCost = cost.Resources
                    .Where(r => r.ResourceId == ledgerEntry.ItemId)
                    .Sum(r => r.Amount);
                Assert.Equal(-totalCost, ledgerEntry.Delta);
                Assert.Equal(0, ledgerEntry.BalanceAfter);
            }
        }
        Assert.True(hadHeroIdEntry, nameof(hadHeroIdEntry));
    }
}
