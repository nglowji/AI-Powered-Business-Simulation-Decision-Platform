namespace BusinessTwin.Server.Modules.Ai;

public sealed record AlertFacts(
    decimal? InventoryValue,
    decimal? InventoryTargetValue,
    decimal? Revenue,
    decimal? PreviousRevenue,
    decimal? Expenses,
    decimal? PreviousExpenses,
    decimal? Payables,
    decimal? OverduePayables);

public sealed record BusinessAlert(
    string Code,
    string Severity,
    string Message,
    string Source);