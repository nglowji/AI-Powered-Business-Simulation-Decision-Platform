using BusinessTwin.Server.Modules.DigitalTwin;

namespace BusinessTwin.Server.Modules.Simulation;

public sealed record SimulationParameters(
    decimal PriceChangePercent,
    decimal DemandChangePercent,
    decimal PurchaseCostChangePercent,
    decimal MarketingCostChangePercent,
    decimal OperatingCostChangePercent,
    decimal SupplierLeadTimeChangePercent);

public sealed record RunSimulationRequest(
    string Name,
    BusinessState BaseState,
    SimulationParameters Parameters);

public sealed record SimulationResult(
    decimal Revenue,
    decimal Cost,
    decimal Profit,
    decimal InventoryValue,
    decimal CashFlow,
    string StockoutRisk);

public sealed record SimulationScenario(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    BusinessState BaseState,
    SimulationParameters Parameters,
    SimulationResult Result);