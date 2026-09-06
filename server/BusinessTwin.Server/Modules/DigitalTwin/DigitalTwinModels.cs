namespace BusinessTwin.Server.Modules.DigitalTwin;

public sealed record BusinessState(
    decimal Revenue,
    decimal Profit,
    decimal InventoryValue,
    decimal CashFlow,
    int Orders,
    int ActiveCustomers,
    int ActiveSuppliers);

public sealed record CreateSnapshotRequest(BusinessState State, string Source);

public sealed record DigitalTwinSnapshot(
    Guid Id,
    int Version,
    DateTimeOffset CapturedAt,
    string Source,
    BusinessState State);

public sealed record SnapshotComparison(
    int FromVersion,
    int ToVersion,
    decimal RevenueChange,
    decimal ProfitChange,
    decimal InventoryValueChange,
    decimal CashFlowChange,
    int OrdersChange);