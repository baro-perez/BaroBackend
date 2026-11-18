using BaroBackend.Domain.Common;
using BaroBackend.Domain.Metagame.Data;
using BaroBackend.Domain.Metagame.Data.Catalog;

namespace BaroBackend.Domain.Metagame;

public sealed class Player
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<ResourceBalance> Balances => balances;
    public IReadOnlyCollection<PlayerHero> Heroes => heroes;

    private readonly List<ResourceBalance> balances = [];
    private readonly List<PlayerHero> heroes = [];

    private Player() { }

    public static Player Create(string name, DateTimeOffset now)
    {
        name = name?.Trim() ?? "";
        if (name.Length is < 3 or > 32)
        {
            throw new DomainException("Name must be between 3 and 32 characters.");
        }

        return new Player
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            CreatedAt = now
        };
    }

    public int GetHeroLevel(string heroId)
    {
        return heroes.FirstOrDefault(b => b.HeroId == heroId)?.Level ?? 0;
    }

    public long GetResourceAmount(string resourceId)
    {
        return balances.FirstOrDefault(b => b.ResourceId == resourceId)?.Amount ?? 0;
    }

    public LedgerEntry ApplyResourceDelta(
        string resourceId,
        long delta,
        DateTimeOffset now,
        IResourceCatalog resourceCatalog, 
        string context)
    {
        if (delta == 0)
        {
            throw new DomainException("Delta cannot be zero.");
        }

        if (!resourceCatalog.ResourceIdIsValid(resourceId))
        {
            throw new DomainException("Delta cannot be zero.");
        }

        var balance = balances.FirstOrDefault(b => b.ResourceId == resourceId);
        var currentAmount = balance?.Amount ?? 0;
        var newAmount = checked(currentAmount + delta);
        if (newAmount < 0)
        {
            throw new DomainException($"Insufficient {resourceId}: have {currentAmount}, need {-delta}.");
        }

        if (balance is null)
        {
            balance = new ResourceBalance(resourceId);
            balances.Add(balance);
        }
        balance.Set(newAmount);

        return new LedgerEntry(Id, 
            resourceId, PlayerItem.Resource, delta, newAmount, context, now);
    }

    public bool CanSpend(Cost cost, IList<ResourceCostDiscrepancy>? unsatisfied = null)
    {
        return cost.IsAffordable(GetResourceAmount, unsatisfied);
    }

    public LevelUpHeroResult DevUnlockHero(
        string heroId, 
        int level,
        IHeroCatalog heroCatalog,
        DateTimeOffset now)
    {
        if (level <= 0)
        {
            throw new DomainException($"Can't unlock at level 0, hero levels start at 1");
        }
        if (heroes.Any(h => h.HeroId == heroId))
        {
            throw new DomainException($"Player already had hero '{heroId}'.");
        }

        if (!heroCatalog.HeroIdIsValid(heroId))
        {
            throw new DomainException($"Requesting an unlock for non-existent hero: '{heroId}'.");
        }

        var hero = new PlayerHero(heroId, level);
        heroes.Add(hero);
        return new(hero, [new LedgerEntry(Id, heroId, PlayerItem.Hero, level, level, nameof(DevUnlockHero), now)]);
    }

    public LevelUpHeroResult BuyUnlockHero(
        string heroId,
        IHeroCatalog heroCatalog,
        IResourceCatalog resourceCatalog,
        DateTimeOffset now)
    {
        if (heroes.Any(h => h.HeroId == heroId))
        {
            throw new DomainException($"Player already had hero '{heroId}'.");
        }

        var heroData = heroCatalog.GetHeroById(heroId);
        if (heroData is null)
        {
            throw new DomainException($"Requesting an unlock for non-existent hero: '{heroId}'.");
        }

        var cost = heroData.GetUnlockCost();
        var ledger = new List<LedgerEntry>();
        foreach (var resourceCost in cost.Resources)
        {
            var newLedgerEntry = ApplyResourceDelta(
                resourceCost.ResourceId, -resourceCost.Amount, now,
                resourceCatalog,
                GetLedgerReasonBuyUnlockHero(heroId));

            ledger.Add(newLedgerEntry);
        }

        var hero = new PlayerHero(heroId, 1);
        heroes.Add(hero);
        ledger.Add(new(Id, heroId, PlayerItem.Hero, 1, 1, GetLedgerReasonBuyUnlockHero(heroId), now));
        return new(hero, ledger);
    }

    public LevelUpHeroResult BuyLevelUpHero(
        string heroId,
        IHeroCatalog heroCatalog,
        IResourceCatalog resourceCatalog,
        DateTimeOffset now)
    {
        var hero = heroes.FirstOrDefault(h => h.HeroId == heroId);
        if (hero is null)
        {
            throw new DomainException($"Player doesn't have hero '{heroId}'.");
        }

        var heroData = heroCatalog.GetHeroById(heroId);
        if (heroData is null)
        {
            throw new DomainException($"Requesting a level up but hero doesn't exist in the catalog: '{heroId}'.");
        }

        var ledger = new List<LedgerEntry>();
        var cost = heroData.GetLevelUpCost(hero.Level);
        foreach (var resourceCost in cost.Resources)
        {
            var newLedgerEntry = ApplyResourceDelta(
                resourceCost.ResourceId, -resourceCost.Amount, now,
                resourceCatalog,
                GetLedgerReasonBuyLevelUpHero(heroId));

            ledger.Add(newLedgerEntry);
        }

        hero.LevelUp();
        ledger.Add(new(Id, heroId, PlayerItem.Hero, 1, hero.Level, GetLedgerReasonBuyLevelUpHero(heroId), now));
        return new(hero, ledger);
    }

    public static string GetLedgerReasonBuyLevelUpHero(string heroId)
    {
        return $"levelup {heroId}";
    }

    public static string GetLedgerReasonBuyUnlockHero(string heroId)
    {
        return $"purchase {heroId}";
    }
}