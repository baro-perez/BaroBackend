namespace BaroBackend.Domain.Metagame;

public sealed class ResourceBalance
{
    public string ResourceId { get; private set; } = null!;
    public long Amount { get; private set; }

    private ResourceBalance() { }

    internal ResourceBalance(string resourceId, long amount = 0)
    {
        ResourceId = resourceId;
        Amount = amount;
    }

    internal void Set(long amount) => Amount = amount;
}
