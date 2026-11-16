using BaroBackend.Domain.Metagame;

namespace BaroBackend.Api.Metagame.EndpointDtos;

public sealed record PlayerHeroDto(
    string HeroId,
    int Level)
{
    internal static PlayerHeroDto From(PlayerHero hero)
    {
        return new PlayerHeroDto(
            hero.HeroId,
            hero.Level);
    }
}