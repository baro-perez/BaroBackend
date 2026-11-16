using BaroBackend.Domain.Metagame;

namespace BaroBackend.Api.Metagame.EndpointDtos;

public sealed record LevelUpHeroResultDto(PlayerHeroDto PlayerHeroDto,
    IReadOnlyList<LedgerEntryDto> Ledgerchanges)
{
    internal static LevelUpHeroResultDto From(LevelUpHeroResult levelUpResult)
    {
        return new LevelUpHeroResultDto(
            PlayerHeroDto.From(levelUpResult.PlayerHero),
            levelUpResult.LedgerChanges.Select(LedgerEntryDto.From).ToArray());
    }
}