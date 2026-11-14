using BaroBackend.Domain.Metagame;

namespace BaroBackend.Api.Metagame.EndpointDtos;

public sealed record ResourceBalanceResponse(string ResourceId, long Amount)
{
    internal static ResourceBalanceResponse? From(ResourceAndAmount b)
    {
        return new ResourceBalanceResponse(b.ResourceId, b.Amount);
    }
}