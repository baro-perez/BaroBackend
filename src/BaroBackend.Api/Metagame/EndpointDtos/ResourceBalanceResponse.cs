using BaroBackend.Domain.Metagame;

namespace BaroBackend.Api.Metagame.EndpointDtos;

public sealed record ResourceBalanceResponse(string ResourceId, long Amount)
{
    internal static ResourceBalanceResponse? From(ResourceBalance b)
    {
        return new ResourceBalanceResponse(b.ResourceId, b.Amount);
    }
}