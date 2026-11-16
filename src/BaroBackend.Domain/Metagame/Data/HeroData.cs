namespace BaroBackend.Domain.Metagame.Data;
public sealed record HeroData(
        string HeroId,
        string Name,
        HeroStatData Health,
        HeroStatData Power,
        HeroStatData Speed,
        IReadOnlyList<Cost> LevelUpCosts)
{
    public Cost GetUnlockCost()
    {
        return LevelUpCosts.Count == 0 ? new([]) : LevelUpCosts[0];
    }

    public Cost GetLevelUpCost(int currentLevel)
    {
        return LevelUpCosts.Count < currentLevel
            ? LevelUpCosts.LastOrDefault() ?? new([])
            : LevelUpCosts[currentLevel];
    }
}