using BusinessTwin.Server.Modules.DigitalTwin;
using BusinessTwin.Server.Modules.Simulation;

namespace BusinessTwin.Server.Tests;

public sealed class SimulationServiceTests
{
    [Fact]
    public void Run_UsesCopiedBusinessStateAndCalculatesScenarioResult()
    {
        var service = new SimulationService();
        var baseState = new BusinessState(1000m, 300m, 800m, 100m, 50, 10, 3);
        var request = new RunSimulationRequest(
            "Price increase",
            baseState,
            new SimulationParameters(5m, 10m, 3m, 0m, 0m, 0m));

        var scenario = service.Run(request, DateTimeOffset.UtcNow);

        Assert.Equal(1155m, scenario.Result.Revenue);
        Assert.Equal(700m * 1.03m, scenario.Result.Cost);
        Assert.Equal("Low", scenario.Result.StockoutRisk);
        Assert.Equal(baseState, scenario.BaseState);
    }

    [Fact]
    public void Run_WithSevereDemandChange_ReportsHighStockoutRisk()
    {
        var service = new SimulationService();
        var request = new RunSimulationRequest(
            "Demand spike",
            new BusinessState(1000m, 300m, 100m, 100m, 50, 10, 3),
            new SimulationParameters(0m, 200m, 0m, 0m, 0m, 0m));

        var scenario = service.Run(request, DateTimeOffset.UtcNow);

        Assert.Equal("High", scenario.Result.StockoutRisk);
    }
}