
namespace BaroBackend.Domain.Metagame.Data;

public sealed record Cost(IReadOnlyCollection<ResourceAmount> Resources)
{
    public bool IsAffordable(Func<string, long> getResourceAmount, IList<ResourceCostDiscrepancy>? unsatisfied = null)
    {
        bool result = true;

        // Resources could be repeated!
        var groupedResources = new Dictionary<string, long>();
        foreach (var resourceCost in Resources)
        {
            var id = resourceCost.ResourceId;
            if (groupedResources.TryGetValue(id, out var current))
            {
                groupedResources[id] = current + resourceCost.Amount;
            }
            else
            {
                groupedResources.Add(id, resourceCost.Amount);
            }
        }

        // Check from the grouped collection
        foreach (var resourceCost in groupedResources)
        {
            var current = getResourceAmount(resourceCost.Key);
            if (current < resourceCost.Value)
            {
                result = false;
                unsatisfied?.Add(new(resourceCost.Key, resourceCost.Value, current));
            }
        }

        return result;
    }
}
