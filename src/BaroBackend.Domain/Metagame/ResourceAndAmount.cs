namespace BaroBackend.Domain.Metagame;

public sealed record ResourceAndAmount
{
    public string ResourceId { get; private set; } = null!;
    public long Amount { get; private set; }

    private ResourceAndAmount() { }

    internal ResourceAndAmount(string resourceId, long amount = 0)
    {
        ResourceId = resourceId;
        Amount = amount;
    }

    internal void Set(long amount) => Amount = amount;
}