namespace BusinessTwin.Server.Modules.Ai;

public sealed class AlertService
{
    public IReadOnlyList<BusinessAlert> Evaluate(AlertFacts facts)
    {
        var alerts = new List<BusinessAlert>();

        if (facts.InventoryValue is not null && facts.InventoryTargetValue is not null
            && facts.InventoryValue < facts.InventoryTargetValue * 0.25m)
        {
            alerts.Add(new BusinessAlert(
                "inventory_low",
                "high",
                "Inventory value is below 25% of its target.",
                "business-facts-tool"));
        }

        if (facts.InventoryValue is not null && facts.InventoryTargetValue is not null
            && facts.InventoryValue > facts.InventoryTargetValue * 1.5m)
        {
            alerts.Add(new BusinessAlert(
                "inventory_high",
                "medium",
                "Inventory value is above 150% of its target.",
                "business-facts-tool"));
        }

        if (facts.Revenue is not null && facts.PreviousRevenue > 0
            && facts.Revenue < facts.PreviousRevenue * 0.8m)
        {
            alerts.Add(new BusinessAlert(
                "revenue_decline",
                "high",
                "Revenue is down by more than 20% compared with the previous period.",
                "business-facts-tool"));
        }

        if (facts.Expenses is not null && facts.PreviousExpenses > 0
            && facts.Expenses > facts.PreviousExpenses * 1.2m)
        {
            alerts.Add(new BusinessAlert(
                "expense_increase",
                "medium",
                "Expenses are up by more than 20% compared with the previous period.",
                "business-facts-tool"));
        }

        if (facts.OverduePayables is > 0)
        {
            alerts.Add(new BusinessAlert(
                "overdue_payables",
                "high",
                "Overdue supplier payables require attention.",
                "business-facts-tool"));
        }

        return alerts;
    }
}