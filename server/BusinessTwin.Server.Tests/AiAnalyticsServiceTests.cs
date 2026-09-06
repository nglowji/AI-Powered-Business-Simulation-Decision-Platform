using BusinessTwin.Server.Modules.Ai;

namespace BusinessTwin.Server.Tests;

public sealed class AiAnalyticsServiceTests
{
    [Fact]
    public void Forecast_UsesCurrentAndPreviousRevenueToCalculateGrowth()
    {
        var service = new AiAnalyticsService();
        var request = new AiAnalyticsRequest(
            new BusinessFacts(1100m, 300m, 10, 500m, new DateOnly(2026, 9, 6)),
            new BusinessFacts(1000m, 280m, 9, 600m, new DateOnly(2026, 9, 5)),
            20m,
            5);

        var forecast = service.Forecast(request);

        Assert.Equal(10m, forecast.RevenueGrowthPercent);
        Assert.Equal(1210m, forecast.ForecastRevenue);
        Assert.Equal(400m, forecast.ForecastInventoryValue);
        Assert.False(forecast.RequiresRealtimeData);
    }

    [Fact]
    public void Recommend_WhenProjectedInventoryIsNegative_ReturnsHighPriorityAction()
    {
        var service = new AiAnalyticsService();
        var request = new AiAnalyticsRequest(
            new BusinessFacts(1100m, 300m, 10, 50m, new DateOnly(2026, 9, 6)),
            null,
            20m,
            5);

        var recommendations = service.Recommend(request);

        var recommendation = Assert.Single(recommendations);
        Assert.Equal("high", recommendation.Priority);
    }

    [Fact]
    public void Forecast_WithoutRequiredRealtimeFacts_DoesNotGuess()
    {
        var service = new AiAnalyticsService();
        var request = new AiAnalyticsRequest(
            new BusinessFacts(null, null, null, null, null),
            null,
            null,
            7);

        var forecast = service.Forecast(request);

        Assert.True(forecast.RequiresRealtimeData);
        Assert.Null(forecast.ForecastRevenue);
    }
}