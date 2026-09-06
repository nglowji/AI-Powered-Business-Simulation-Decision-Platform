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