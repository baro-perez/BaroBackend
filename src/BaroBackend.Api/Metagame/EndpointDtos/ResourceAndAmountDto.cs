using BaroBackend.Domain.Metagame;

namespace BaroBackend.Api.Metagame.EndpointDtos;

public sealed record ResourceAndAmountDto(string ResourceId, long Amount)
{
    internal static ResourceAndAmountDto? From(ResourceBalance b)
    {
        return new ResourceAndAmountDto(b.ResourceId, b.Amount);
    }
}
