using BaroBackend.Domain.Metagame;

namespace BaroBackend.Api.Metagame.EndpointDtos;

public sealed record LedgerEntryResponse(
    Guid Id,
    Guid PlayerId,
    string ResourceId,
    long Delta,
    long BalanceAfter,
    string Reason,
    DateTimeOffset CreatedAt)
{
    internal static LedgerEntryResponse? From(LedgerEntry l)
    {
        return new LedgerEntryResponse(
            l.Id,
            l.PlayerId,
            l.ResourceId,
            l.Delta,
            l.BalanceAfter,
            l.Reason,
            l.CreatedAt);
    }
}