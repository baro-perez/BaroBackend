namespace BaroBackend.Domain.Metagame;

public record LevelUpHeroResult(PlayerHero PlayerHero, IReadOnlyList<LedgerEntry> LedgerChanges);
