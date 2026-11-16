using BaroBackend.Domain.Metagame;

namespace BaroBackend.Api.Metagame.EndpointDtos;

public sealed record LedgerEntryDto(
    Guid Id,
    Guid PlayerId,
    string ResourceId,
    long Delta,
    long BalanceAfter,
    string Reason,
    DateTimeOffset CreatedAt)
{
    internal static LedgerEntryDto From(LedgerEntry l)
    {
        return new LedgerEntryDto(
            l.Id,
            l.PlayerId,
            l.ItemId,
            l.Delta,
            l.BalanceAfter,
            l.Reason,
            l.CreatedAt);
    }
}
