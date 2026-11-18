using BaroBackend.Domain.Metagame.Data;

namespace BaroBackend.Domain.Tests;

public class CostTests
{
    [Theory]
    [InlineData(0), InlineData(10)]
    public void IsAffordable_WithResources_ReturnsTrue(int extraResources)
    {
        var cost = new Cost([
            new("a", 10),
            new("b", 20),
            ]);
        var resources = new Dictionary<string, long>()
        {
            ["a"] = 10 + extraResources,
            ["b"] = 20 + extraResources,
        };

        Assert.True(cost.IsAffordable(resources.GetValueOrDefault));
    }

    [Fact]
    public void IsAffordable_WithoutResources_ReturnsFalse()
    {
        var cost = new Cost([
            new("a", 10),
            new("b", 20),
            ]);
        var resources = new Dictionary<string, long>()
        {
            ["a"] = 1,
            ["b"] = 1,
        };

        Assert.False(cost.IsAffordable(resources.GetValueOrDefault));
    }

    [Fact]
    public void IsAffordable_GroupedWithResource_ReturnsTrue()
    {
        var cost = new Cost([
            new("a", 10),
            new("b", 20),
            new("a", 10),
            ]);
        var resources = new Dictionary<string, long>()
        {
            ["a"] = 20,
            ["b"] = 20,
        };

        Assert.True(cost.IsAffordable(resources.GetValueOrDefault));
    }

    [Fact]
    public void IsAffordable_GroupedWithoutResources_ReturnsFalse()
    {
        var cost = new Cost([
            new("a", 10),
            new("b", 20),
            new("a", 10),
            ]);
        var resources = new Dictionary<string, long>()
        {
            ["a"] = 10,
            ["b"] = 20,
        };

        Assert.False(cost.IsAffordable(resources.GetValueOrDefault));
    }
}
