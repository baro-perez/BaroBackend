namespace BaroBackend.Domain.Metagame;
public sealed class LedgerEntry
{
    public Guid Id { get; private set; }
    public Guid PlayerId { get; private set; }
    public string ItemId { get; private set; } = null!;
    public PlayerItem ItemType { get; private set; }
    public long Delta { get; private set; }
    public long BalanceAfter { get; private set; }
    public string Reason { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    private LedgerEntry() { }

    internal LedgerEntry(
        Guid playerId,
        string itemId,
        PlayerItem itemType,
        long delta,
        long balanceAfter,
        string reason,
        DateTimeOffset createdAt)
    {
        Id = Guid.CreateVersion7();
        PlayerId = playerId;
        ItemId = itemId;
        ItemType = itemType;
        Delta = delta;
        BalanceAfter = balanceAfter;
        Reason = reason;
        CreatedAt = createdAt;
    }
}
