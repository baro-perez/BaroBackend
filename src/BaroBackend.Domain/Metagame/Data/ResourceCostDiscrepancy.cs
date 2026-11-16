namespace BaroBackend.Domain.Metagame.Data;

public sealed record ResourceCostDiscrepancy(string ResourceId, long Cost, long CurrentAmount)
{
    public long Difference => Cost - CurrentAmount;
}
