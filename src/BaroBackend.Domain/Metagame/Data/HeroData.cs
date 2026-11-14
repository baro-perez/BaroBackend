namespace BaroBackend.Domain.Metagame.Data;
public sealed record HeroData(
        string HeroId,
        string Name,
        HeroStatData Health,
        HeroStatData Power,
        HeroStatData Speed,
        IReadOnlyCollection<Cost> LevelUpCosts);
