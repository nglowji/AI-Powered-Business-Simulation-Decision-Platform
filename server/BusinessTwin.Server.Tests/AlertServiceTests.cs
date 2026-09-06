using BusinessTwin.Server.Modules.Ai;

namespace BusinessTwin.Server.Tests;

public sealed class AlertServiceTests
{
    [Fact]
    public void Evaluate_ReturnsAlertsForLowStockRevenueDeclineAndOverduePayables()
    {
        var service = new AlertService();
        var facts = new AlertFacts(20m, 100m, 700m, 1000m, 100m, 100m, 500m, 50m);

        var alerts = service.Evaluate(facts);

        Assert.Contains(alerts, alert => alert.Code == "inventory_low");
        Assert.Contains(alerts, alert => alert.Code == "revenue_decline");
        Assert.Contains(alerts, alert => alert.Code == "overdue_payables");
    }

    [Fact]
    public void Evaluate_WithStableFacts_ReturnsNoAlerts()
    {
        var service = new AlertService();
        var facts = new AlertFacts(80m, 100m, 1000m, 950m, 500m, 600m, 500m, 0m);

        var alerts = service.Evaluate(facts);

        Assert.Empty(alerts);
    }
}