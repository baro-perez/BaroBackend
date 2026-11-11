using BaroBackend.Domain.Common;

namespace BaroBackend.Domain.MetaGame;

public sealed class Player
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<ResourceBalance> Balances => balances;

    private readonly List<ResourceBalance> balances = [];

    private Player() { }

    public static Player Create(string name, DateTimeOffset now)
    {
        name = name?.Trim() ?? "";
        if (name.Length is < 3 or > 32)
        {
            throw new DomainException("Name must be between 3 and 32 characters.");
        }

        return new Player
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            CreatedAt = now
        };
    }

    public long GetBalance(string resourceId)
    {
        return balances.FirstOrDefault(b => b.ResourceId == resourceId)?.Amount ?? 0;
    }

    public LedgerEntry ApplyResourceDelta(
        string resourceId, long delta, DateTimeOffset now, string context)
    {
        if (delta == 0)
        {
            throw new DomainException("Delta cannot be zero.");
        }

        var balance = balances.FirstOrDefault(b => b.ResourceId == resourceId);
        if (balance is null)
        {
            balance = new ResourceBalance(Id, resourceId);
            balances.Add(balance);
        }

        var newAmount = checked(balance.Amount + delta);
        if (newAmount < 0)
        {
            throw new DomainException($"Insufficient {resourceId}: have {balance.Amount}, need {-delta}.");
        }

        balance.Set(newAmount);
        return new LedgerEntry(Guid.CreateVersion7(), Id, resourceId, delta, newAmount, context, now);
    }
}