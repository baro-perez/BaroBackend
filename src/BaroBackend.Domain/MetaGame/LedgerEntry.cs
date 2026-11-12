namespace BaroBackend.Domain.Metagame;

public sealed class LedgerEntry
{
    public Guid Id { get; private set; }
    public Guid PlayerId { get; private set; }
    public string ResourceId { get; private set; } = null!;
    public long Delta { get; private set; }
    public long BalanceAfter { get; private set; }
    public string Reason { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    private LedgerEntry() { }

    internal LedgerEntry(Guid id, Guid playerId, string resourceId,
        long delta, long balanceAfter, string reason, DateTimeOffset createdAt)
    {
        Id = id;
        PlayerId = playerId;
        ResourceId = resourceId;
        Delta = delta;
        BalanceAfter = balanceAfter;
        Reason = reason;
        CreatedAt = createdAt;
    }
}
