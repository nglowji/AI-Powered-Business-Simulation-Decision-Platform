using BusinessTwin.Server.Modules.DigitalTwin;

namespace BusinessTwin.Server.Tests;

public sealed class DigitalTwinServiceTests
{
    [Fact]
    public void CreateSnapshot_AssignsImmutableIncreasingVersions()
    {
        var service = new DigitalTwinService();
        var first = service.CreateSnapshot(CreateRequest(100m, 50m, 500m, 20m, 10), DateTimeOffset.UtcNow);
        var second = service.CreateSnapshot(CreateRequest(120m, 60m, 450m, 30m, 12), DateTimeOffset.UtcNow);

        Assert.Equal(1, first.Version);
        Assert.Equal(2, second.Version);
        Assert.Equal(2, service.GetTimeline().Count);
    }

    [Fact]
    public void Compare_ReturnsMetricDifferencesBetweenVersions()
    {
        var service = new DigitalTwinService();
        service.CreateSnapshot(CreateRequest(100m, 50m, 500m, 20m, 10), DateTimeOffset.UtcNow);
        service.CreateSnapshot(CreateRequest(120m, 60m, 450m, 30m, 12), DateTimeOffset.UtcNow);

        var comparison = service.Compare(1, 2);

        Assert.Equal(20m, comparison.RevenueChange);
        Assert.Equal(10m, comparison.ProfitChange);
        Assert.Equal(-50m, comparison.InventoryValueChange);
        Assert.Equal(2, comparison.OrdersChange);
    }

    private static CreateSnapshotRequest CreateRequest(
        decimal revenue,
        decimal profit,
        decimal inventoryValue,
        decimal cashFlow,
        int orders)
    {
        return new CreateSnapshotRequest(
            new BusinessState(revenue, profit, inventoryValue, cashFlow, orders, 5, 2),
            "test");
    }
}