namespace BusinessTwin.Server.Modules.Simulation;

public sealed class SimulationService
{
    private readonly List<SimulationScenario> scenarios = [];

    public SimulationScenario Run(RunSimulationRequest request, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Scenario name is required.", nameof(request.Name));
        }

        var parameters = request.Parameters;
        var revenue = request.BaseState.Revenue
            * (1 + parameters.PriceChangePercent / 100)
            * (1 + parameters.DemandChangePercent / 100);
        var cost = request.BaseState.Revenue - request.BaseState.Profit;
        cost *= 1 + (parameters.PurchaseCostChangePercent
            + parameters.MarketingCostChangePercent
            + parameters.OperatingCostChangePercent) / 100;
        var inventoryValue = request.BaseState.InventoryValue
            * (1 - parameters.DemandChangePercent / 100)
            * (1 - parameters.SupplierLeadTimeChangePercent / 200);
        var profit = revenue - cost;
        var cashFlow = request.BaseState.CashFlow + profit - request.BaseState.Profit;
        var stockoutRisk = inventoryValue <= 0
            ? "High"
            : inventoryValue < request.BaseState.InventoryValue * 0.75m
                ? "Medium"
                : "Low";

        var result = new SimulationResult(revenue, cost, profit, inventoryValue, cashFlow, stockoutRisk);
        var scenario = new SimulationScenario(
            Guid.NewGuid(),
            request.Name,
            createdAt,
            request.BaseState,
            parameters,
            result);
        scenarios.Add(scenario);
        return scenario;
    }

    public IReadOnlyList<SimulationScenario> GetScenarios() => scenarios.AsReadOnly();
}