namespace BaroBackend.Domain.Metagame;

public sealed class PlayerHero
{
    public string HeroId { get; private set; } = null!;
    public int Level { get; private set; }

    private PlayerHero() { }

    internal PlayerHero(string heroId, int level = 0)
    {
        HeroId = heroId;
        Level = level;
    }

    internal void LevelUp() => ++Level;
}