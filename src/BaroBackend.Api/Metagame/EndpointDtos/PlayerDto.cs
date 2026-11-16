using BaroBackend.Domain.Metagame;

namespace BaroBackend.Api.Metagame.EndpointDtos;

public sealed record PlayerDto(Guid Id, string Name, DateTimeOffset CreatedAt)
{
    internal static PlayerDto From(Player p)
    {
        return new PlayerDto(p.Id, p.Name, p.CreatedAt);
    }
}
