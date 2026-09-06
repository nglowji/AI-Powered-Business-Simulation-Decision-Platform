namespace BusinessTwin.Server.Modules.Ai;

public sealed class AiAnalyticsService
{
    public AiForecastResponse Forecast(AiAnalyticsRequest request)
    {
        if (request.ForecastDays <= 0 || request.Current.Revenue is null || request.Current.InventoryValue is null)
        {
            return new AiForecastResponse(null, null, null, true, []);
        }

        var growthPercent = CalculateGrowth(request.Previous?.Revenue, request.Current.Revenue.Value);
        var forecastRevenue = request.Current.Revenue.Value * (1 + growthPercent / 100);
        var forecastInventory = request.ExpectedDemandPerDay is null
            ? request.Current.InventoryValue
            : request.Current.InventoryValue.Value - request.ExpectedDemandPerDay.Value * request.ForecastDays;

        return new AiForecastResponse(
            forecastRevenue,
            forecastInventory,
            growthPercent,
            false,
            ["business-facts-tool"]);
    }

    public IReadOnlyList<AiRecommendation> Recommend(AiAnalyticsRequest request)
    {
        var recommendations = new List<AiRecommendation>();

        if (request.Current.InventoryValue is null || request.ExpectedDemandPerDay is null)
        {
            return recommendations;
        }

        var projectedInventory = request.Current.InventoryValue.Value
            - request.ExpectedDemandPerDay.Value * Math.Max(request.ForecastDays, 1);
        if (projectedInventory < 0)
        {
            recommendations.Add(new AiRecommendation(
                "Review replenishment for inventory",
                "Projected demand exceeds the supplied inventory value over the forecast period.",
                "high"));
        }

        return recommendations;
    }

    private static decimal CalculateGrowth(decimal? previousRevenue, decimal currentRevenue)
    {
        if (previousRevenue is null || previousRevenue == 0)
        {
            return 0;
        }

        return (currentRevenue - previousRevenue.Value) / previousRevenue.Value * 100;
    }
}