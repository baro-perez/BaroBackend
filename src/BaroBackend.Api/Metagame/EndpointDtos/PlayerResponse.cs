using BaroBackend.Domain.Metagame;

namespace BaroBackend.Api.Metagame.EndpointDtos;

public sealed record PlayerResponse(Guid Id, string Name)
{
    internal static PlayerResponse? FromPlayer(Player newPlayer)
    {
        return new PlayerResponse(newPlayer.Id, newPlayer.Name);
    }
}
