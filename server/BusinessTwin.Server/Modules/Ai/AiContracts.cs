namespace BusinessTwin.Server.Modules.Ai;

public sealed record AiAssistantRequest(
    string Question,
    BusinessFacts? Facts);

public sealed record BusinessFacts(
    decimal? Revenue,
    decimal? Profit,
    int? Orders,
    decimal? InventoryValue,
    DateOnly? AsOfDate);

public sealed record AiAssistantResponse(
    string Intent,
    string Answer,
    IReadOnlyList<string> Sources,
    bool RequiresRealtimeData);

public sealed record AiAnalyticsRequest(
    BusinessFacts Current,
    BusinessFacts? Previous,
    decimal? ExpectedDemandPerDay,
    int ForecastDays);

public sealed record AiForecastResponse(
    decimal? ForecastRevenue,
    decimal? ForecastInventoryValue,
    decimal? RevenueGrowthPercent,
    bool RequiresRealtimeData,
    IReadOnlyList<string> Sources);

public sealed record AiRecommendation(
    string Action,
    string Reason,
    string Priority);