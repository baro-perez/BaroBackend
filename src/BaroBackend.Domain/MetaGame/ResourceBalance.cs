namespace BaroBackend.Domain.MetaGame;

public sealed class ResourceBalance
{
    public Guid PlayerId { get; private set; }
    public string ResourceId { get; private set; } = null!;
    public long Amount { get; private set; }

    private ResourceBalance() { }

    internal ResourceBalance(Guid playerId, string resourceId, long amount = 0)
    {
        PlayerId = playerId;
        ResourceId = resourceId;
        Amount = amount;
    }

    internal void Set(long amount) => Amount = amount;
}
