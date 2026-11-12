using BaroBackend.Domain.Metagame;

using Microsoft.AspNetCore.Http.HttpResults;

namespace BaroBackend.Api.Metagame.EndpointDtos;

public sealed record PlayerResponse(Guid Id, string Name, DateTimeOffset CreatedAt)
{
    internal static PlayerResponse? From(Player p)
    {
        return new PlayerResponse(p.Id, p.Name, p.CreatedAt);
    }
}
